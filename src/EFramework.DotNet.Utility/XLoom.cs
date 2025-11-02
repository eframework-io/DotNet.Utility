// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XLoom 提供了一个轻量级的任务调度系统，用于管理异步任务、定时器和多线程并发。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 异步任务：支持执行和异常恢复异步任务
    /// - 线程管理：支持任务管理、线程暂停/恢复控制、指标监控（FPS/QPS）
    /// - 定时器管理：支持设置/取消超时和间歇调用
    /// 
    /// 使用手册
    /// 
    /// 1. 异步任务
    /// 
    /// 1.1 基础操作
    ///     // 无参数异步执行
    ///     XLoom.RunAsync(() => {
    ///         // 异步逻辑
    ///     });
    /// 
    ///     // 带参数异步执行
    ///     XLoom.RunAsync((int id) => {
    ///         // 异步逻辑
    ///     }, 1);
    /// 
    ///     // 异常恢复的异步执行
    ///     XLoom.RunAsync(() => {
    ///         // 异步逻辑
    ///     }, true);
    /// 
    /// 2. 线程管理
    /// 
    /// 2.1 任务调度
    ///     // 在指定业务线程执行任务
    ///     XLoom.RunIn(() => {
    ///         Console.WriteLine("在业务线程 0 中执行。");
    ///     }, 0);
    /// 
    ///     // 获取当前的业务线程标识
    ///     var id = XLoom.ID();
    /// 
    ///     // 获取业务线程的性能指标
    ///     var fps = XLoom.FPS(0); // 业务线程 0 的帧率
    ///     var qps = XLoom.QPS(0); // 业务线程 0 的速率
    /// 
    /// 2.2 线程控制
    ///     // 暂停/恢复单个业务线程
    ///     XLoom.Pause(0);  // 暂停业务线程 0
    ///     XLoom.Resume(0); // 恢复业务线程 0
    /// 
    ///     // 暂停/恢复所有业务线程
    ///     XLoom.Pause();
    ///     XLoom.Resume();
    /// 
    /// 2.3 指标监控（Prometheus）
    ///     // 查阅文档以获取指标详情。
    /// 
    /// 2.4 可选配置（首选项）
    ///     // 支持通过首选项配置对线程系统进行调整：
    ///     // XLoom/Count：业务线程池大小，默认为 1
    ///     // XLoom/Step：业务线程更新频率（毫秒），默认为 10
    ///     // XLoom/Queue：每个业务线程的任务队列容量，默认为 50000
    /// 
    ///     // 配置示例（JSON）：
    ///     {
    ///         "XLoom/Count": 8,
    ///         "XLoom/Step": 10,
    ///         "XLoom/Queue": 50000
    ///     }
    /// 
    /// 3. 定时器管理
    /// 
    /// 3.1 超时调用
    ///     // 延迟执行
    ///     var id = XLoom.SetTimeout(() => {
    ///         Console.WriteLine("1 秒后执行。");
    ///     }, 1000);
    /// 
    ///     // 取消超时调用
    ///     XLoom.ClearTimeout(id);
    /// 
    /// 3.2 间歇调用
    ///     // 周期执行
    ///     var id = XLoom.SetInterval(() => {
    ///         Console.WriteLine("间隔 1 秒执行。");
    ///     }, 1000);
    /// 
    ///     // 取消间歇调用
    ///     XLoom.ClearInterval(id);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class XLoom
    {
        #region 首选项
        internal static class Preferences
        {
            /// <summary>
            /// Count 是业务线程数量的配置键。
            /// </summary>
            internal const string Count = "XLoom/Count";

            /// <summary>
            /// CountDefault 是业务线程数量的默认值。
            /// </summary>
            internal const int CountDefault = 1;

            /// <summary>
            /// Step 是业务线程更新步长的配置键。
            /// </summary>
            internal const string Step = "XLoom/Step";

            /// <summary>
            /// StepDefault 是业务线程更新步长的默认值。
            /// </summary>
            internal const int StepDefault = 10;

            /// <summary>
            /// Queue 是业务线程队列大小的配置键。
            /// </summary>
            internal const string Queue = "XLoom/Queue";

            /// <summary>
            /// QueueDefault 是业务线程队列大小的默认值。
            /// </summary>
            internal const int QueueDefault = 50000;
        }
        #endregion

        #region 静态变量
        /// <summary>
        /// InitializeLock 是初始化的互斥锁。
        /// </summary>
        internal static readonly Mutex InitializeLock = new();

        /// <summary>
        /// LoomPauseStatuses 是线程暂停的状态。
        /// </summary>
        internal static bool[] LoomPauseStatuses = Array.Empty<bool>();

        /// <summary>
        /// LoomThreads 是业务线程和系统线程的映射表。
        /// </summary>
        internal static readonly ConcurrentDictionary<Thread, int> LoomThreads = new();

        /// <summary>
        /// LoomCount 是业务线程的总数。
        /// </summary>
        internal static int LoomCount = 0;

        /// <summary>
        /// LoomQueueLimit 是业务线程队列的最大容量限制。
        /// </summary>
        internal static int LoomQueueLimit = Preferences.QueueDefault;

        /// <summary>
        /// LoomTasks 是业务线程任务的队列。
        /// </summary>
        internal static ConcurrentQueue<Action>[] LoomTasks = Array.Empty<ConcurrentQueue<Action>>();

        /// <summary>
        /// LoomFPSs 是业务线程刷新帧率统计。
        /// </summary>
        internal static int[] LoomFPSs = Array.Empty<int>();

        /// <summary>
        /// LoomQPSs 是业务线程处理速率统计。
        /// </summary>
        internal static int[] LoomQPSs = Array.Empty<int>();

        /// <summary>
        /// LoomFPSGauges 是业务线程刷新帧率 Prometheus 指标。
        /// </summary>
        internal static Gauge[] LoomFPSGauges = Array.Empty<Gauge>();

        /// <summary>
        /// LoomQPSGauges 是业务线程处理速率 Prometheus 指标。
        /// </summary>
        internal static Gauge[] LoomQPSGauges = Array.Empty<Gauge>();

        /// <summary>
        /// LoomQueryCounters 是业务线程处理总数 Prometheus 指标。
        /// </summary>
        internal static Counter[] LoomQueryCounters = Array.Empty<Counter>();

        /// <summary>
        /// LoomQueryCounter 是所有业务线程处理总数 Prometheus 指标。
        /// </summary>
        internal static Counter LoomQueryCounter;
        #endregion

        #region 定时器相关
        /// <summary>
        /// TimerIncrement 是定时器自增标识。
        /// </summary>
        internal static long TimerIncrement = 0;

        /// <summary>
        /// AllTimers 是所有定时器。
        /// </summary>
        internal static List<Timer>[] AllTimers = Array.Empty<List<Timer>>();

        /// <summary>
        /// NewTimers 是新的定时器。
        /// </summary>
        internal static List<Timer>[] NewTimers = Array.Empty<List<Timer>>();

        /// <summary>
        /// NewTimerLocks 是新定时器锁。
        /// </summary>
        internal static readonly object[] NewTimerLocks = Array.Empty<object>();

        /// <summary>
        /// RemoveTimers 是待删除的定时器。
        /// </summary>
        internal static List<int>[] RemoveTimers = Array.Empty<List<int>>();

        /// <summary>
        /// RemoveTimerLocks 是删除定时器锁。
        /// </summary>
        internal static readonly object[] RemoveTimerLocks = Array.Empty<object>();
        #endregion

        #region 定时器类
        /// <summary>
        /// Timer 定义了定时器的基本结构。
        /// </summary>
        internal class Timer
        {
            /// <summary>
            /// ID 是定时器的唯一标识。
            /// </summary>
            internal int ID { get; set; }

            /// <summary>
            /// Callback 是定时器触发时的回调函数。
            /// </summary>
            internal Action Callback { get; set; }

            /// <summary>
            /// Initial 是定时器的初始时间（毫秒）。
            /// </summary>
            internal long Initial { get; set; }

            /// <summary>
            /// Period 是定时器的触发周期（毫秒）。
            /// </summary>
            internal long Period { get; set; }

            /// <summary>
            /// Trigger 是定时器的触发时间（毫秒）。
            /// </summary>
            internal long Trigger { get; set; }

            /// <summary>
            /// Repeat 是定时器的重复次数。
            /// </summary>
            internal long Repeat { get; set; }

            /// <summary>
            /// Panic 表示是否发生异常，用于异常恢复控制。
            /// </summary>
            internal bool Panic { get; set; }

            /// <summary>
            /// Reset 重置定时器到初始状态。
            /// </summary>
            internal Timer Reset()
            {
                ID = 0;
                Callback = null;
                Initial = 0;
                Period = 0;
                Trigger = 0;
                Repeat = 0;
                Panic = false;
                return this;
            }
        }
        #endregion

        #region 系统初始化
        /// <summary>
        /// XLoom 在程序初始化时自动执行初始化操作。
        /// </summary>
        static XLoom()
        {
            XApp.OnStop += static _ =>
            {
                foreach (var thread in LoomThreads.Keys)
                {
                    try { thread.Interrupt(); }
                    catch { }
                }
            };
            Initialize(XPrefs.Asset);
        }

        /// <summary>
        /// Initialize 执行初始化操作。
        /// </summary>
        /// <param name="preferences">配置参数</param>
        internal static void Initialize(XPrefs.IBase preferences)
        {
            if (preferences == null)
            {
                XLog.Panic(new Exception("XLoom.Initialize: preferences is null."));
                return;
            }

            var count = preferences.GetInt(Preferences.Count, Preferences.CountDefault);
            var step = preferences.GetInt(Preferences.Step, Preferences.StepDefault);
            var queue = preferences.GetInt(Preferences.Queue, Preferences.QueueDefault);
            if (count <= 0 || step <= 0 || queue <= 0)
            {
                XLog.Panic(new Exception($"XLoom.Initialize: invalid parameters, count: {count}, step: {step}, queue: {queue}."));
                return;
            }

            lock (InitializeLock)
            {
                // 关闭所有业务线程
                foreach (var thread in LoomThreads.Keys)
                {
                    try { thread.Interrupt(); }
                    catch { }
                }
                LoomThreads.Clear();

                LoomCount = count;
                LoomQueueLimit = queue;
                LoomTasks = new ConcurrentQueue<Action>[count];
                LoomPauseStatuses = new bool[count];
                LoomFPSs = new int[count];
                LoomQPSs = new int[count];
                LoomFPSGauges = new Gauge[count];
                LoomQPSGauges = new Gauge[count];
                LoomQueryCounters = new Counter[count];

                for (int i = 0; i < count; i++)
                {
                    LoomTasks[i] = new ConcurrentQueue<Action>();
                    LoomPauseStatuses[i] = false;
                }

                AllTimers = new List<Timer>[count];
                NewTimers = new List<Timer>[count];
                RemoveTimers = new List<int>[count];

                for (int i = 0; i < count; i++)
                {
                    AllTimers[i] = new List<Timer>();
                    NewTimers[i] = new List<Timer>();
                    RemoveTimers[i] = new List<int>();
                }

                LoomQueryCounter = Metrics.CreateCounter("xloom_query_total", "Total number of queries processed by all looms.");
                for (int i = 0; i < count; i++)
                {
                    LoomFPSGauges[i] = Metrics.CreateGauge($"xloom_fps_{i}", $"Frames per second for loom {i}.");
                    LoomQPSGauges[i] = Metrics.CreateGauge($"xloom_qps_{i}", $"Queries per second for loom {i}.");
                    LoomQueryCounters[i] = Metrics.CreateCounter($"xloom_query_total_{i}", $"Total number of queries processed by loom {i}.");
                }

                for (int i = 0; i < count; i++)
                {
                    var loomID = i; // 局部化变量以避免闭包
                    var thread = new Thread(() => Loop(loomID, step));
                    LoomThreads[thread] = loomID;
                    thread.Start();
                }

                XLog.Notice($"XLoom.Initialize: allocated {count} loom(s).");
            }
        }
        #endregion

        #region 系统自循环
        /// <summary>
        /// Loop 线程主循环。
        /// </summary>
        /// <param name="loomID">业务线程标识</param>
        /// <param name="step">更新步长（毫秒）</param>
        internal static void Loop(int loomID, int step)
        {
            try
            {
                var threadID = Environment.CurrentManagedThreadId;
                var lastTime = XTime.GetMillisecond();
                long metricsTime = 0;
                var frameCount = 0;
                var queryCount = 0;

                while (true)
                {
                    if (LoomPauseStatuses[loomID])
                    {
                        try
                        {
                            Thread.Sleep(step);
                            // 在暂停状态下重置计数器和指标
                            frameCount = 0;
                            queryCount = 0;
                            LoomFPSs[loomID] = 0;
                            LoomQPSs[loomID] = 0;
                            if (LoomFPSGauges.Length > loomID) LoomFPSGauges[loomID].Set(0);
                            if (LoomQPSGauges.Length > loomID) LoomQPSGauges[loomID].Set(0);
                            lastTime = XTime.GetMillisecond(); // 更新时间戳，避免恢复后的突然跳变
                        }
                        catch (Exception e) when (e is OperationCanceledException || e is ThreadInterruptedException)
                        {
                            XLog.Notice($"XLoom.Loop({loomID}): receive signal of interrupt.");
                            break;
                        }
                    }
                    else
                    {
                        var currentTime = XTime.GetMillisecond();
                        var deltaTime = currentTime - lastTime;
                        lastTime = currentTime;

                        metricsTime += deltaTime;
                        if (metricsTime >= 1000)
                        {
                            var fps = (float)frameCount * 1000 / metricsTime;
                            var ifps = (int)fps;
                            var qps = (float)queryCount * 1000 / metricsTime;
                            var iqps = (int)qps;
                            LoomFPSs[loomID] = ifps;
                            LoomQPSs[loomID] = iqps;
                            if (LoomFPSGauges.Length > loomID) LoomFPSGauges[loomID].Set(fps);
                            if (LoomQPSGauges.Length > loomID) LoomQPSGauges[loomID].Set(qps);
                            frameCount = 0;
                            queryCount = 0;
                            metricsTime = 0;
                        }

                        // 优化：批量处理任务队列，直到队列为空
                        // 这样可以提高吞吐量，特别是在任务积压的情况下
                        while (LoomTasks[loomID].TryDequeue(out var runIn))
                        {
                            queryCount++;
                            LoomQueryCounters[loomID].Inc();
                            LoomQueryCounter.Inc();
                            try { runIn(); }
                            catch (Exception e) { XLog.Panic(e, $"XLoom.Loop({loomID}): execute runin failed."); }
                        }

                        // 处理定时器更新
                        try
                        {
                            Thread.Sleep(step);
                            frameCount++;
                            Tick(loomID);
                        }
                        catch (Exception e) when (e is OperationCanceledException || e is ThreadInterruptedException)
                        {
                            XLog.Notice($"XLoom.Loop({loomID}): receive signal of interrupt.");
                            break;
                        }
                    }
                }
            }
            catch (Exception e) { XLog.Panic(e, $"XLoom.Loop({loomID}): unexpected exception."); }
        }

        /// <summary>
        /// Tick 更新业务线程的定时器状态。
        /// </summary>
        /// <param name="loomID">业务线程标识</param>
        internal static void Tick(int loomID)
        {
            // 添加新定时器
            if (NewTimers[loomID].Count > 0)
            {
                lock (NewTimers[loomID])
                {
                    AllTimers[loomID].AddRange(NewTimers[loomID]);
                    NewTimers[loomID].Clear();
                }
            }

            // 删除定时器
            if (RemoveTimers[loomID].Count > 0)
            {
                lock (RemoveTimers[loomID])
                {
                    foreach (var id in RemoveTimers[loomID])
                    {
                        for (int i = AllTimers[loomID].Count - 1; i >= 0; i--)
                        {
                            if (AllTimers[loomID][i].ID == id)
                            {
                                var timer = AllTimers[loomID][i];
                                AllTimers[loomID].RemoveAt(i);
                                XPool.Object<Timer>.Put(timer.Reset());
                                break;
                            }
                        }
                    }
                    RemoveTimers[loomID].Clear();
                }
            }

            // 更新定时器
            if (AllTimers[loomID] != null)
            {
                var currentTime = XTime.GetMillisecond();
                for (int i = AllTimers[loomID].Count - 1; i >= 0; i--)
                {
                    var timer = AllTimers[loomID][i];
                    if (timer.Panic)
                    {
                        if (timer.Repeat > 0) // interval 发生 panic 不取消定时器
                        {
                            timer.Panic = false;
                            timer.Trigger = timer.Initial + timer.Period * (++timer.Repeat);
                        }
                        else // timeout 发生 panic 则直接移除
                        {
                            lock (RemoveTimers[loomID]) RemoveTimers[loomID].Add(timer.ID);
                            continue;
                        }
                    }
                    if (timer.Trigger <= currentTime) // 因存在固定刷新间歇，可能会导致间歇调用的周期越来越长
                    {
                        if (timer.Callback != null)
                        {
                            timer.Panic = true;
                            try { timer.Callback(); }
                            catch (Exception e) { XLog.Panic(e, $"XLoom.Tick({loomID}): execute timer callback failed."); }
                            timer.Panic = false;
                        }
                        if (timer.Repeat == 0)
                        {
                            lock (RemoveTimers[loomID]) RemoveTimers[loomID].Add(timer.ID);
                        }
                        else timer.Trigger = timer.Initial + timer.Period * (++timer.Repeat);
                    }
                }
            }
        }
        #endregion

        #region 公共接口
        /// <summary>
        /// Pause 暂停业务线程的运行。
        /// </summary>
        /// <param name="loomID">业务线程标识，若未指定，则暂停所有业务线程</param>
        public static void Pause(int? loomID = null)
        {
            if (loomID != null)
            {
                if (loomID.Value < 0)
                {
                    XLog.Critical($"XLoom.Pause: loom id of {loomID.Value} can not be zero or negative.");
                    return;
                }
                if (loomID.Value >= LoomCount)
                {
                    XLog.Critical($"XLoom.Pause: loom id of {loomID.Value} can not equals or greater than: {Count()}");
                    return;
                }
                LoomPauseStatuses[loomID.Value] = true;
            }
            else
            {
                for (int id = 0; id < LoomPauseStatuses.Length; id++)
                {
                    LoomPauseStatuses[id] = true;
                }
            }
        }

        /// <summary>
        /// Resume 恢复业务线程的运行。
        /// </summary>
        /// <param name="loomID">业务线程标识，若未指定，则恢复所有业务线程</param>
        public static void Resume(int? loomID = null)
        {
            if (loomID != null)
            {
                if (loomID.Value < 0)
                {
                    XLog.Critical($"XLoom.Resume: loom id of {loomID.Value} can not be zero or negative.");
                    return;
                }
                if (loomID.Value >= LoomCount)
                {
                    XLog.Critical($"XLoom.Resume: loom id of {loomID.Value} can not equals or greater than: {Count()}");
                    return;
                }
                LoomPauseStatuses[loomID.Value] = false;
            }
            else
            {
                for (int id = 0; id < LoomPauseStatuses.Length; id++)
                {
                    LoomPauseStatuses[id] = false;
                }
            }
        }

        /// <summary>
        /// RunIn 在指定业务线程中执行任务。
        /// </summary>
        /// <param name="callback">任务函数</param>
        /// <param name="loomID">业务线程标识，若未指定，默认在业务线程 0 中执行任务</param>
        public static void RunIn(Action callback, int? loomID = null)
        {
            if (callback == null)
            {
                XLog.Critical("XLoom.RunIn: callback can not be nil.");
                return;
            }
            loomID ??= 0;
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.RunIn: loom id of {loomID.Value} can not be zero or negative.");
                return;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.RunIn: loom id of {loomID.Value} can not equals or greater than: {Count()}.");
                return;
            }
            // 检查队列容量限制，ConcurrentQueue.Count 在并发环境下可能不准确，但用于容量检查仍然有效
            if (LoomTasks[loomID.Value].Count >= LoomQueueLimit)
            {
                XLog.Critical($"XLoom.RunIn: too many runins of {loomID.Value} (queue limit: {LoomQueueLimit}).");
                return;
            }
            LoomTasks[loomID.Value].Enqueue(callback);
        }

        /// <summary>
        /// Count 返回业务线程总数。
        /// </summary>
        /// <returns>业务线程总数</returns>
        public static int Count() => LoomCount;

        /// <summary>
        /// ID 获取系统线程对应的业务线程标识。
        /// </summary>
        /// <param name="threadID">系统线程标识，若未指定，则使用当前系统线程标识</param>
        /// <returns>业务线程标识，如果系统线程未绑定业务线程，返回 -1</returns>
        public static int ID(int? threadID = null)
        {
            if (threadID != null)
            {
                foreach (var thread in LoomThreads.Keys)
                {
                    if (thread.ManagedThreadId == threadID)
                    {
                        return LoomThreads[thread];
                    }
                }
                return -1;
            }
            else return LoomThreads.TryGetValue(Thread.CurrentThread, out var loomID) ? loomID : -1;
        }

        /// <summary>
        /// FPS 获取线程的刷新帧率。
        /// </summary>
        /// <param name="loomID">业务线程标识，若未指定，返回当前业务线程的刷新帧率</param>
        /// <returns>每秒帧数，如果业务线程标识无效则返回 0</returns>
        public static int FPS(int? loomID = null)
        {
            loomID ??= ID();
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.FPS: loom id of {loomID.Value} can not be zero or negative.");
                return 0;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.FPS: loom id of {loomID.Value} can not equals or greater than: {Count()}.");
                return 0;
            }
            return LoomFPSs[loomID.Value];
        }

        /// <summary>
        /// QPS 获取业务线程的处理速率。
        /// </summary>
        /// <param name="loomID">业务线程标识，若未指定，返回当前业务线程的处理速率</param>
        /// <returns>每秒处理的任务数，如果业务线程标识无效则返回 0</returns>
        public static int QPS(int? loomID = null)
        {
            loomID ??= ID();
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.QPS: loom id of {loomID.Value} can not be zero or negative.");
                return 0;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.QPS: loom id of {loomID.Value} can not equals or greater than: {Count()}.");
                return 0;
            }
            return LoomQPSs[loomID.Value];
        }
        #endregion

        #region 定时器接口
        /// <summary>
        /// SetTimeout 设置超时调用。
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <param name="loomID">业务线程标识，若未指定，在当前业务线程中执行</param>
        /// <returns>定时器标识，如果参数无效则返回 -1</returns>
        public static int SetTimeout(Action callback, long timeout, int? loomID = null)
        {
            if (callback == null)
            {
                XLog.Critical("XLoom.SetTimeout: callback can not be nil.");
                return -1;
            }
            if (timeout < 0)
            {
                XLog.Critical($"XLoom.SetTimeout: timeout of {timeout} can not be zero or negative.");
                return -1;
            }

            loomID ??= ID();
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.SetTimeout: loom id of {loomID.Value} can not be zero or negative.");
                return -1;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.SetTimeout: loom id of {loomID.Value} can not equals or greater than: {Count()}");
                return -1;
            }

            var timer = XPool.Object<Timer>.Get();
            timer.Reset();
            timer.ID = (int)Interlocked.Increment(ref TimerIncrement);
            timer.Callback = callback;
            timer.Initial = XTime.GetMillisecond();
            timer.Period = timeout;
            timer.Repeat = 0;
            timer.Trigger = timer.Initial + timer.Period;
            lock (NewTimers[loomID.Value]) NewTimers[loomID.Value].Add(timer);

            return timer.ID;
        }

        /// <summary>
        /// ClearTimeout 取消超时调用。
        /// </summary>
        /// <param name="id">定时器标识</param>
        /// <param name="loomID">业务线程标识，若未指定，在当前业务线程中取消</param>
        public static void ClearTimeout(int id, int? loomID = null)
        {
            loomID ??= ID();
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.ClearTimeout: loom id of {loomID.Value} can not be zero or negative.");
                return;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.ClearTimeout: loom id of {loomID.Value} can not equals or greater than: {Count()}");
                return;
            }
            lock (RemoveTimers[loomID.Value]) RemoveTimers[loomID.Value].Add(id);
        }

        /// <summary>
        /// SetInterval 设置间歇调用。
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="interval">调用间歇（毫秒）</param>
        /// <param name="loomID">业务线程标识，若未指定，在当前业务线程中执行</param>
        /// <returns>定时器标识，如果参数无效则返回 -1</returns>
        public static int SetInterval(Action callback, long interval, int? loomID = null)
        {
            if (callback == null)
            {
                XLog.Critical("XLoom.SetInterval: callback can not be nil.");
                return -1;
            }
            if (interval < 0)
            {
                XLog.Critical($"XLoom.SetInterval: interval of {interval} can not be zero or negative.");
                return -1;
            }

            loomID ??= ID();
            if (loomID.Value < 0)
            {
                XLog.Critical($"XLoom.SetInterval: loom id of {loomID.Value} can not be zero or negative.");
                return -1;
            }
            if (loomID.Value >= LoomCount)
            {
                XLog.Critical($"XLoom.SetInterval: loom id of {loomID.Value} can not equals or greater than: {Count()}");
                return -1;
            }

            var timer = XPool.Object<Timer>.Get();
            timer.Reset();
            timer.ID = (int)Interlocked.Increment(ref TimerIncrement);
            timer.Callback = callback;
            timer.Initial = XTime.GetMillisecond();
            timer.Period = interval;
            timer.Repeat = 1;
            timer.Trigger = timer.Initial + timer.Period;
            lock (NewTimers[loomID.Value]) NewTimers[loomID.Value].Add(timer);

            return timer.ID;
        }

        /// <summary>
        /// ClearInterval 取消间歇调用。
        /// </summary>
        /// <param name="id">定时器标识</param>
        /// <param name="loomID">业务线程标识，若未指定，在当前业务线程中取消</param>
        public static void ClearInterval(int id, int? loomID = null) => ClearTimeout(id, loomID);
        #endregion

        #region 异步执行
        /// <summary>
        /// RunAsync 异步执行指定的函数。
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <param name="recover">异常恢复，如果为 true，则在发生异常时会自动重试</param>
        /// <returns>表示异步操作的任务</returns>
        public static async Task RunAsync(Action callback, bool recover = false)
        {
            if (callback == null) return;

            try { await Task.Run(callback); }
            catch (Exception e)
            {
                XLog.Panic(e, $"XLoom.RunAsync: execute callback failed.");
                if (recover)
                {
                    await Task.Delay(100); // 短暂延迟
                    await RunAsync(callback, recover); // 执行重试
                }
            }
        }

        /// <summary>
        /// RunAsync 异步执行带参数的函数。
        /// </summary>
        /// <typeparam name="T1">参数类型</typeparam>
        /// <param name="callback">回调函数</param>
        /// <param name="arg1">函数参数</param>
        /// <param name="recover">异常恢复，如果为 true，则在发生异常时会自动重试</param>
        /// <returns>表示异步操作的任务</returns>
        public static async Task RunAsync<T1>(Action<T1> callback, T1 arg1, bool recover = false)
        {
            if (callback == null) return;

            try { await Task.Run(() => callback(arg1)); }
            catch (Exception e)
            {
                XLog.Panic(e, $"XLoom.RunAsync: execute callback failed.");
                if (recover)
                {
                    await Task.Delay(100); // 短暂延迟
                    await RunAsync(callback, arg1, recover); // 执行重试
                }
            }
        }

        /// <summary>
        /// RunAsync 异步执行带两个参数的函数。
        /// </summary>
        /// <typeparam name="T1">参数类型1</typeparam>
        /// <typeparam name="T2">参数类型2</typeparam>
        /// <param name="callback">回调函数</param>
        /// <param name="arg1">函数参数1</param>
        /// <param name="arg2">函数参数2</param>
        /// <param name="recover">异常恢复，如果为 true，则在发生异常时会自动重试</param>
        /// <returns>表示异步操作的任务</returns>
        public static async Task RunAsync<T1, T2>(Action<T1, T2> callback, T1 arg1, T2 arg2, bool recover = false)
        {
            if (callback == null) return;

            try { await Task.Run(() => callback(arg1, arg2)); }
            catch (Exception e)
            {
                XLog.Panic(e, $"XLoom.RunAsync: execute callback failed.");
                if (recover)
                {
                    await Task.Delay(100); // 短暂延迟
                    await RunAsync(callback, arg1, arg2, recover); // 执行重试
                }
            }
        }

        /// <summary>
        /// RunAsync 异步执行带三个参数的函数。
        /// </summary>
        /// <typeparam name="T1">参数类型1</typeparam>
        /// <typeparam name="T2">参数类型2</typeparam>
        /// <typeparam name="T3">参数类型3</typeparam>
        /// <param name="callback">回调函数</param>
        /// <param name="arg1">函数参数1</param>
        /// <param name="arg2">函数参数2</param>
        /// <param name="arg3">函数参数3</param>
        /// <param name="recover">异常恢复，如果为 true，则在发生异常时会自动重试</param>
        /// <returns>表示异步操作的任务</returns>
        public static async Task RunAsync<T1, T2, T3>(Action<T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3, bool recover = false)
        {
            if (callback == null) return;

            try { await Task.Run(() => callback(arg1, arg2, arg3)); }
            catch (Exception e)
            {
                XLog.Panic(e, $"XLoom.RunAsync: execute callback failed.");
                if (recover)
                {
                    await Task.Delay(100); // 短暂延迟
                    await RunAsync(callback, arg1, arg2, arg3, recover); // 执行重试
                }
            }
        }
        #endregion
    }
}