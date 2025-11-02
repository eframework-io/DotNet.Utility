// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XApp 提供了应用程序的状态管理，用于控制应用程序的启动、运行和退出流程。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 应用程序状态控制：提供 Awake、Start、Stop 等状态函数和事件
    /// - 优雅的启动和退出：支持启动前环境检查和退出时的资源清理
    /// 
    /// 使用手册
    /// 1. 生命周期
    /// 详细的应用状态转换流程请查阅模块文档。
    /// 
    /// 2. 应用启动
    /// 应用程序需要实现 IBase 接口：
    /// 
    ///     public class MyApplication : XApp.IBase
    ///     {
    ///         public bool Awake()
    ///         {
    ///             // 启动前检查，返回 false 将终止启动
    ///             return true;
    ///         }
    /// 
    ///         public void Start()
    ///         {
    ///             // 启动时初始化；请避免阻塞该方法
    ///         }
    /// 
    ///         public void Stop(CountdownEvent counter)
    ///         {
    ///             // 退出清理；如需异步清理请调用 counter.AddCount()
    ///             // 完成后调用 counter.Signal()
    ///         }
    ///     }
    /// 
    ///     public static class Program
    ///     {
    ///         public static void Main(string[] args)
    ///         {
    ///             // 启动应用（会阻塞直至退出）
    ///             var application = new MyApplication();
    ///             XApp.Run(application);
    ///         }
    ///     }
    /// 
    /// 3. 应用退出
    ///     XApp.Quit();
    /// 
    /// 4. 事件监听
    /// 内置了应用程序的状态转换事件监听：
    /// 
    ///     // 初始化事件
    ///     XApp.OnAwake += () => { };
    /// 
    ///     // 启动事件
    ///     XApp.OnStart += () => { };
    /// 
    ///     // 退出事件
    ///     XApp.OnStop += (counter) =>
    ///     {
    ///         // 异步清理示例
    ///         counter.AddCount();
    ///         Task.Run(() =>
    ///         {
    ///             // 执行清理工作...
    ///             counter.Signal();
    ///         });
    ///     };
    /// 
    /// 5. 单例访问
    /// 在任意位置通过 Instance&lt;T&gt;() 获取应用实例（未启动前为 null）：
    /// 
    ///     var instance = XApp.Instance&lt;MyApplication&gt;();
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class XApp
    {
        /// <summary>
        /// IBase 定义了应用程序的生命周期接口。
        /// 实现此接口的类型可以通过 Run 函数启动并由框架管理其生命周期。
        /// </summary>
        public interface IBase
        {
            /// <summary>
            /// Awake 在应用程序启动前调用，用于进行初始化检查。
            /// 返回 false 将导致应用程序终止启动。
            /// </summary>
            /// <returns>true 表示初始化检查通过，false 表示初始化失败</returns>
            bool Awake();

            /// <summary>
            /// Start 在应用程序启动时调用，用于执行初始化操作。
            /// </summary>
            void Start();

            /// <summary>
            /// Stop 在应用程序退出时调用，用于执行清理操作。
            /// </summary>
            /// <param name="counter">用于同步等待清理完成的 CountdownEvent</param>
            void Stop(CountdownEvent counter);
        }

        /// <summary>
        /// instance 是应用程序的单例实例。
        /// </summary>
        internal static IBase instance;

        /// <summary>
        /// runOnce 确保 Run 方法只执行一次。
        /// </summary>
        internal static int runOnce = 0;

        /// <summary>
        /// quitOnce 确保 Quit 方法只执行一次。
        /// </summary>
        internal static int quitOnce = 0;

        /// <summary>
        /// quitSource 是退出信号源。
        /// </summary>
        internal static readonly CancellationTokenSource quitSource = new();

        /// <summary>
        /// quitLock 是退出操作的互斥锁。
        /// </summary>
        internal static readonly object quitLock = new();

        /// <summary>
        /// Instance 返回应用程序的单例实例。
        /// </summary>
        /// <typeparam name="T">IBase 接口类型</typeparam>
        /// <returns>应用程序实例，在应用程序启动前调用将返回 null</returns>
        public static T Instance<T>() where T : class, IBase => instance as T;

        /// <summary>
        /// OnAwake 事件在应用程序启动前调用。
        /// </summary>
        public static event Action OnAwake;

        /// <summary>
        /// OnStart 事件在应用程序启动时调用。
        /// </summary>
        public static event Action OnStart;

        /// <summary>
        /// OnStop 事件在应用程序退出时调用。
        /// </summary>
        public static event Action<CountdownEvent> OnStop;

        /// <summary>
        /// Run 启动并运行应用程序。
        /// 此函数会阻塞直到应用程序退出，可以通过以下方式退出：
        /// - 调用 Quit 函数
        /// - 接收到 SIGTERM 或 SIGINT 信号（Ctrl+C）
        /// - Awake 返回 false
        /// </summary>
        /// <param name="application">IBase 接口实例</param>
        public static void Run(IBase application)
        {
            if (Interlocked.CompareExchange(ref runOnce, 1, 0) != 0)
            {
                return; // 已经运行过，直接返回
            }

            instance = application ?? throw new Exception("XApp.Run: application is null.");
            if (!instance.Awake()) throw new Exception("XApp.Run: application awake failed.");

            OnAwake?.Invoke();
            XLog.Notice("XApp.Run: application has been awaked.");

            application.Start();
            OnStart?.Invoke();
            XLog.Notice("XApp.Run: application has been started.");

            try
            {
                // 监听 Ctrl + C 信号 (SIGINT)
                var cancelSource = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // 阻止默认的退出行为
                    XLog.Notice("XApp.Run: receive signal of interrupt.");
                    cancelSource.Cancel();
                };

                // 监听主动/被动退出信号
                try
                {
                    var waitTask = Task.Run(async () =>
                    {
                        var quitTask = Task.Run(() =>
                        {
                            try { quitSource.Token.WaitHandle.WaitOne(); }
                            catch (ObjectDisposedException) { }
                        });

                        var cancelTask = Task.Run(() =>
                        {
                            try { cancelSource.Token.WaitHandle.WaitOne(); }
                            catch (ObjectDisposedException) { }
                        });

                        await Task.WhenAny(quitTask, cancelTask);
                    });

                    waitTask.Wait();
                }
                catch (OperationCanceledException) { }
                finally { cancelSource.Dispose(); }
            }
            finally
            {
                var counter = new CountdownEvent(1);
                try
                {
                    application.Stop(counter);
                    OnStop?.Invoke(counter);
                    counter.Signal();
                    counter.Wait();
                }
                catch (Exception e) { XLog.Panic(e, "XApp.Run: application stop failed."); }
                finally { counter.Dispose(); }
                XLog.Notice("XApp.Run: application has been stopped.");
            }
        }

        /// <summary>
        /// Quit 触发应用程序退出。
        /// 此函数可以在任意线程中安全调用，多次调用只有第一次会生效。
        /// </summary>
        public static void Quit()
        {
            if (Interlocked.CompareExchange(ref quitOnce, 1, 0) != 0)
            {
                return; // 已经调用过，直接返回
            }

            lock (quitLock)
            {
                try { quitSource.Cancel(); }
                catch (ObjectDisposedException) { }
            }

            XLog.Notice("XApp.Quit: receive signal of quit.");
        }
    }
}