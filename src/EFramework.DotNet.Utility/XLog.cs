// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Text;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XLog 提供了一个遵循 RFC5424 标准的日志系统，支持多适配器管理、日志轮转和结构化标签等特性。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 内置标准输出和文件存储适配器
    /// - 支持日志文件的自动轮转和清理
    /// - 支持异步写入和线程安全操作
    /// - 支持结构化的日志标签系统
    /// 
    /// 使用手册
    /// 
    /// 1. 日志输出
    ///     // 不同级别的日志记录（按严重程度从高到低排序）
    ///     XLog.Emergency("系统崩溃");
    ///     XLog.Alert("立即处理");
    ///     XLog.Critical("严重错误");
    ///     XLog.Error("操作失败");
    ///     XLog.Warn("潜在问题");
    ///     XLog.Notice("重要信息");
    ///     XLog.Info("一般信息");
    ///     XLog.Debug("调试信息");
    /// 
    ///     // 检查日志级别
    ///     var currentLevel = XLog.Level();
    ///     var logable = XLog.Able(XLog.LevelType.Debug);
    /// 
    /// 2. 多适配器
    /// 
    /// 2.1 标准输出
    ///     // 标准输出适配器支持以下配置项：
    ///     var preferences = new XPrefs.IBase();
    ///     var config = new XPrefs.IBase();
    ///     config.Set("Level", "Info");     // 日志级别
    ///     config.Set("Color", true);       // 着色输出
    ///     preferences.Set("XLog/Std", config);
    ///     XLog.Initialize(preferences);
    /// 
    ///     // 日志内容着色：
    ///     //   Emergency: 黑色背景
    ///     //   Alert: 青色
    ///     //   Critical: 品红色
    ///     //   Error: 红色
    ///     //   Warn: 黄色
    ///     //   Notice: 绿色
    ///     //   Info: 灰色
    ///     //   Debug: 蓝色
    /// 
    /// 2.2 文件存储
    ///     // 文件存储适配器支持以下配置项：
    ///     preferences = new XPrefs.IBase();
    ///     config = new XPrefs.IBase();
    /// 
    ///     // 基础配置
    ///     config.Set("Path", "${Environment.LocalPath}/Log/app.log");  // 日志文件
    ///     config.Set("Level", "Debug");                                // 日志级别
    /// 
    ///     // 轮转配置
    ///     config.Set("Rotate", true);        // 是否启用日志轮转
    ///     config.Set("Daily", true);         // 是否按天轮转
    ///     config.Set("MaxDay", 7);           // 日志文件保留天数
    ///     config.Set("Hourly", false);       // 是否按小时轮转
    ///     config.Set("MaxHour", 168);        // 日志文件保留小时数
    /// 
    ///     // 文件限制
    ///     config.Set("MaxFile", 100);        // 最大文件数量
    ///     config.Set("MaxLine", 1000000);    // 单文件最大行数
    ///     config.Set("MaxSize", 134217728);  // 单文件最大体积（128MB）
    /// 
    ///     preferences.Set("XLog/File", config);
    ///     XLog.Initialize(preferences);
    /// 
    ///     // 文件轮转策略与命名规则详见模块文档
    /// 
    /// 3. 日志标签
    /// 
    /// 3.1 基本用法
    ///     // 创建和使用标签
    ///     var tag = XLog.GetTag()
    ///         .Set("module", "network")
    ///         .Set("action", "connect")
    ///         .Set("userId", "12345");
    /// 
    ///     // 使用标签记录日志
    ///     XLog.Info(tag, "用户连接成功");
    /// 
    ///     // 使用完后回收标签
    ///     XLog.PutTag(tag);
    /// 
    /// 3.2 上下文
    ///     // 设置当前线程的标签
    ///     tag = XLog.GetTag()
    ///         .Set("threadId", "main")
    ///         .Set("session", "abc123");
    ///     XLog.Watch(tag);
    /// 
    ///     // 使用当前线程的标签
    ///     XLog.Info("处理请求");  // 自动附加上下文标签
    /// 
    ///     // 清理上下文标签
    ///     XLog.Defer();
    /// 
    /// 3.3 格式化
    ///     var t = XLog.GetTag()
    ///         .Set("module", "auth")
    ///         .Set("userId", "12345")
    ///         .Set("ip", "192.168.1.1");
    ///     // 输出格式：[时间] [级别] [module=auth, userId=12345, ip=192.168.1.1] 消息内容
    ///     XLog.Info(t, "用户登录成功");
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XLog
    {
        /// <summary>
        /// LevelType 是日志的等级类型。
        /// 遵循 RFC5424 日志标准，规定了八个日志消息的严重性级别，用于表示被记录事件的严重程度或紧急程度。
        /// </summary>
        public enum LevelType : short
        {
            /// <summary>
            /// Undefined 表示未定义的日志类型。
            /// </summary>
            Undefined = -1,

            /// <summary>
            /// Emergency 表示紧急（0）的日志类型，系统不可用，通常用于灾难性故障。
            /// </summary>
            Emergency = 0,

            /// <summary>
            /// Alert 表示警报（1）的日志类型，必须立即采取行动，指示需要立即注意的情况。
            /// </summary>
            Alert = 1,

            /// <summary>
            /// Critical 表示严重（2）的日志类型，指示需要立即注意的严重故障。
            /// </summary>
            Critical = 2,

            /// <summary>
            /// Error 表示错误（3）的日志类型，指示应该解决的错误。
            /// </summary>
            Error = 3,

            /// <summary>
            /// Warn 表示警告（4）的日志类型，指示潜在问题，如果不解决可能会导致错误。
            /// </summary>
            Warn = 4,

            /// <summary>
            /// Notice 表示通知（5）的日志类型，指示值得注意但不一定有问题的事件。
            /// </summary>
            Notice = 5,

            /// <summary>
            /// Info 表示信息（6）的日志类型，用于系统操作的一般信息。
            /// </summary>
            Info = 6,

            /// <summary>
            /// Debug 表示调试（7）的日志类型，用于调试和故障排除目的的消息
            /// </summary>
            Debug = 7,
        }

        /// <summary>
        /// LogData 是日志的数据类，用于封装单条日志的所有相关信息。
        /// </summary>
        internal class LogData
        {
            /// <summary>
            /// pooled 表示标记对象是否已被池化。
            /// </summary>
            private bool pooled;

            /// <summary>
            /// Level 是日志的级别。
            /// </summary>
            public LevelType Level;

            /// <summary>
            /// Force 表示是否强制输出，忽略日志级别限制。
            /// </summary>
            public bool Force;

            /// <summary>
            /// Data 是日志的内容。
            /// </summary>
            public object Data;

            /// <summary>
            /// Args 是格式化的参数。
            /// </summary>
            public object[] Args;

            /// <summary>
            /// Tag 是日志的标签文本。
            /// </summary>
            public string Tag;

            /// <summary>
            /// Time 是日志的时间戳，单位：毫秒。
            /// </summary>
            public long Time;

            /// <summary>
            /// Text 是格式化的日志文本。
            /// </summary>
            /// <param name="tag">是否包含标签信息</param>
            /// <returns>格式化后的日志文本</returns>
            public string Text(bool tag)
            {
                var fmt = Data is string str ? str : null;
                return tag ? logLabels[(int)Level] + " " + (string.IsNullOrEmpty(Tag) ? "" : Tag + " ") + (fmt != null ? XString.Format(fmt, Args) : Data.ToString()) :
                    logLabels[(int)Level] + " " + (fmt != null ? XString.Format(fmt, Args) : Data.ToString());
            }

            /// <summary>
            /// Reset 重置日志数据的所有字段为默认值。
            /// </summary>
            public void Reset()
            {
                Level = LevelType.Undefined;
                Force = false;
                Data = null;
                Args = null;
                Tag = null;
                Time = 0;
            }

            /// <summary>
            /// Get 从对象池获取一个日志数据对象。
            /// </summary>
            /// <returns>日志数据对象</returns>
            public static LogData Get()
            {
                var data = XPool.Object<LogData>.Get();
                data.pooled = false;
                return data;
            }

            /// <summary>
            /// Put 将日志数据对象返回到对象池。
            /// </summary>
            /// <param name="data">要返回的日志数据对象</param>
            public static void Put(LogData data)
            {
                if (data != null)
                {
                    if (!data.pooled)
                    {
                        data.Reset();
                        data.pooled = true;
                        XPool.Object<LogData>.Put(data);
                    }
                }
            }
        }

        /// <summary>
        /// IAdapter 是日志适配器的接口，定义了日志输出的基本操作。
        /// </summary>
        internal interface IAdapter
        {
            /// <summary>
            /// Initialize 初始化日志适配器。
            /// </summary>
            /// <param name="preferences">配置参数</param>
            /// <returns>日志输出级别</returns>
            LevelType Initialize(XPrefs.IBase preferences);

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            void Write(LogData data);

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            void Flush();

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            void Close();
        }

        /// <summary>
        /// logLabels 是日志的标签数组，用于标识不同级别的日志。
        /// </summary>
        internal static readonly string[] logLabels = new string[] {
            "[M]", // Emergency
            "[A]", // Alert
            "[C]", // Critical
            "[E]", // Error
            "[W]", // Warn
            "[N]", // Notice
            "[I]", // Info
            "[D]", // Debug
        };

        /// <summary>
        /// levelMax 是当前最高的日志级别。
        /// </summary>
        internal static LevelType levelMax = LevelType.Undefined;

        /// <summary>
        /// adapters 是日志适配器的映射表。
        /// </summary>
        internal static readonly Dictionary<string, IAdapter> adapters = new();

        /// <summary>
        /// initializeLock 是初始化的互斥锁。
        /// </summary>
        internal static readonly object initializeLock = new();

        /// <summary>
        /// initialized 表示是否初始化完成。
        /// </summary>
        internal static bool initialized = false;

        static XLog()
        {
            AppDomain.CurrentDomain.UnhandledException += static (_, e) =>
            {
                if (e.ExceptionObject != null && e.ExceptionObject is Exception exception)
                {
                    Panic(exception);
                    var panicFile = XFile.PathJoin(XEnv.LocalPath, "Panic", $"{XTime.Format(XTime.GetTimestamp(), XTime.FormatCompact)}.log");
                    XFile.SaveText(panicFile, exception.ToString());
                }
            };
            XApp.OnStop += static _ => Flush();
            XApp.OnStop += static _ => Close();
            Initialize(XPrefs.Asset);
        }

        /// <summary>
        /// Initialize 设置日志系统的配置。
        /// </summary>
        /// <param name="preferences">配置参数</param>
        public static void Initialize(XPrefs.IBase preferences)
        {
            // 清理旧的适配器
            Flush();
            Close();

            // 初始化适配器
            lock (initializeLock)
            {
                var tempLevel = LevelType.Undefined;
                foreach (var kvp in preferences)
                {
                    if (!kvp.Key.StartsWith("XLog/")) continue;

                    var name = kvp.Key.Split('/')[1];
                    var conf = preferences.Get<XPrefs.IBase>(kvp.Key);
                    if (conf == null) continue;

                    IAdapter adapter = null;
                    switch (name)
                    {
                        case "Std": adapter = new StdAdapter(); break;
                        case "File": adapter = new FileAdapter(); break;
                        default: break;
                    }

                    if (adapter != null)
                    {
                        var level = adapter.Initialize(conf);
                        if (level > tempLevel) tempLevel = level;
                        adapters[name] = adapter;
                    }
                }

                if (adapters.Count == 0 || !adapters.ContainsKey("Std"))
                {
                    // 设置默认的输出适配器，避免调用 XLog.* 无法输出
                    tempLevel = LevelType.Debug;
                    adapters["Std"] = new StdAdapter() { level = LevelType.Debug, colored = true };
                }

                // 更新最大日志级别
                levelMax = tempLevel;
                initialized = true;

                Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Initialize: performed initialize with {0} adapter(s), max level is {1}.", adapters.Count, levelMax);
            }
        }

        /// <summary>
        /// Flush 将所有缓冲的日志条目写入到目标位置。
        /// </summary>
        public static void Flush()
        {
            if (adapters.Count > 0)
            {
                foreach (var adapter in adapters.Values) adapter.Flush();
                Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Flush: performed flush with {0} adapter(s).", adapters.Count);
            }
        }

        /// <summary>
        /// Close 刷新并关闭日志系统。
        /// </summary>
        public static void Close()
        {
            lock (initializeLock)
            {
                initialized = false;
                if (adapters.Count > 0)
                {
                    Print(level: LevelType.Notice, force: true, tag: null, data: "XLog.Close: performed close with {0} adapter(s).", adapters.Count);
                    foreach (var adapter in adapters.Values) adapter.Close();
                    adapters.Clear();
                }
            }
        }

        /// <summary>
        /// Level 获取当前日志最大级别。
        /// </summary>
        /// <returns>当前日志最大级别</returns>
        public static LevelType Level() { return levelMax; }

        /// <summary>
        /// Able 检查给定的日志级别是否可以根据配置的最大级别输出。
        /// </summary>
        /// <param name="level">需要检查的日志级别</param>
        /// <returns>是否可以输出</returns>
        public static bool Able(LevelType level)
        {
            Condition(level, null, out var able, out _, out var _, out var _);
            return able;
        }

        /// <summary>
        /// Condition 检查给定的日志级别是否可以根据配置的最大级别输出。
        /// </summary>
        /// <param name="level">需要检查的日志级别</param>
        /// <param name="args">格式参数</param>
        /// <param name="able">是否可以输出</param>
        /// <param name="force">是否强制输出</param>
        /// <param name="tag">日志标签</param>
        /// <param name="nargs">处理后的格式参数</param>
        internal static void Condition(LevelType level, object[] args, out bool able, out bool force, out LogTag tag, out object[] nargs)
        {
            // 未初始化时使用默认的适配器输出
            if (!initialized)
            {
                nargs = args;
                tag = Tag();
                able = true;
                force = true;
                return;
            }

            // 优化1: 提前处理 null 或空数组的情况
            if (args == null || args.Length == 0)
            {
                nargs = args;
                tag = Tag();
                able = level <= levelMax;
                force = false;
                return;
            }

            // 优化2: 避免重复的类型检查，一次性完成类型转换
            if (args[0] is LogTag logTag)
            {
                tag = logTag;
                int newLength = args.Length - 1;
                if (newLength > 0)
                {
                    // 优化3: 只在必要时创建新数组
                    nargs = new object[newLength];
                    Array.Copy(args, 1, nargs, 0, newLength);
                }
                else
                {
                    nargs = Array.Empty<object>();
                }

                // 优化4: 简化条件判断，减少分支
                if (tag.Level != LevelType.Undefined)
                {
                    able = level <= tag.Level;
                    force = true;
                    return;
                }
            }
            else
            {
                tag = Tag();
                nargs = args;
            }

            // 优化5: 默认情况的处理
            able = level <= levelMax;
            force = false;
        }

        /// <summary>
        /// Panic 输出异常的日志。
        /// </summary>
        /// <param name="exception">异常信息</param>
        /// <param name="extras">附加信息</param>
        public static void Panic(Exception exception, string extras = "")
        {
            if (string.IsNullOrEmpty(extras)) Console.Error.WriteLine(exception);
            else Console.Error.WriteLine(new Exception(extras, exception));
        }

        /// <summary>
        /// Emergency 输出紧急（0）级别的日志，系统不可用，通常用于灾难性故障。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Emergency(object data, params object[] args)
        {
            Condition(LevelType.Emergency, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Emergency, force, tag, data, nargs);
        }

        /// <summary>
        /// Alert 输出警报（1）级别的日志，必须立即采取行动，指示需要立即注意的情况。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Alert(object data, params object[] args)
        {
            Condition(LevelType.Alert, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Alert, force, tag, data, nargs);
        }

        /// <summary>
        /// Critical 输出严重（2）级别的日志，指示需要立即注意的严重故障。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Critical(object data, params object[] args)
        {
            Condition(LevelType.Critical, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Critical, force, tag, data, nargs);
        }

        /// <summary>
        /// Error 输出错误（3）级别的日志，指示应该解决的错误。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Error(object data, params object[] args)
        {
            Condition(LevelType.Error, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Error, force, tag, data, nargs);
        }

        /// <summary>
        /// Warn 输出警告（4）级别的日志，指示潜在问题，如果不解决可能会导致错误。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Warn(object data, params object[] args)
        {
            Condition(LevelType.Warn, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Warn, force, tag, data, nargs);
        }

        /// <summary>
        /// Notice 输出通知（5）级别的日志：正常但重要的情况，指示值得注意但不一定有问题的事件。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Notice(object data, params object[] args)
        {
            Condition(LevelType.Notice, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Notice, force, tag, data, nargs);
        }

        /// <summary>
        /// Info 输出信息（6）级别的日志，用于系统操作的一般信息。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Info(object data, params object[] args)
        {
            Condition(LevelType.Info, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Info, force, tag, data, nargs);
        }

        /// <summary>
        /// Debug 输出调试（7）级别的日志：调试级别的消息，用于调试和故障排除目的的消息。
        /// </summary>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式参数</param>
        public static void Debug(object data, params object[] args)
        {
            Condition(LevelType.Debug, args, out var able, out var force, out var tag, out var nargs);
            if (able) Print(LevelType.Debug, force, tag, data, nargs);
        }

        /// <summary>
        /// Print 格式化并输出日志。
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="force">强制输出</param>
        /// <param name="tag">日志标签</param>
        /// <param name="data">日志内容</param>
        /// <param name="args">格式化参数</param>
        public static void Print(LevelType level, bool force, LogTag tag, object data, params object[] args)
        {
            if (data == null) return;

            var rawLog = LogData.Get();
            rawLog.Level = level;
            rawLog.Force = force;
            rawLog.Data = data;
            rawLog.Args = args;
            rawLog.Time = XTime.GetMillisecond();
            rawLog.Tag = tag?.Text ?? string.Empty;

            if (!initialized)
            {
                Console.Out.WriteLine("[{0}] {1}".Format(XTime.Format(rawLog.Time, "MM/dd HH:mm:ss.fff"), rawLog.Text(true)));
            }
            else
            {
                foreach (var adapter in adapters.Values)
                {
                    var log = LogData.Get();
                    log.Level = rawLog.Level;
                    log.Force = rawLog.Force;
                    log.Data = rawLog.Data;
                    log.Args = rawLog.Args;
                    log.Time = rawLog.Time;
                    log.Tag = rawLog.Tag;
                    adapter.Write(log);
                }
            }

            LogData.Put(rawLog);
        }
    }
}
