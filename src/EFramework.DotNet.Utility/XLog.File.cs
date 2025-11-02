// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Text;
using System.Collections.Concurrent;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace EFramework.DotNet.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// FileAdapter 是日志文件适配器，实现日志的文件存储功能。
        /// 支持日志轮转、按时间分割和自动清理等特性。
        /// </summary>
        internal partial class FileAdapter : IAdapter
        {
            /// <summary>
            /// Preferences 是文件日志适配器的配置项。
            /// </summary>
            internal class Preferences
            {
                /// <summary>
                /// Config 是文件日志适配器的配置键。
                /// </summary>
                internal const string Config = "XLog/File";

                /// <summary>
                /// ConfigDefault 是文件日志适配器的默认值。
                /// </summary>
                internal static readonly XPrefs.IBase ConfigDefault = new();

                /// <summary>
                /// Level 是日志输出级别的配置键。
                /// </summary>
                internal const string Level = "Level";

                /// <summary>
                /// LevelDefault 是日志输出级别的默认值。
                /// </summary>
                internal static readonly string LevelDefault = LevelType.Notice.ToString();

                /// <summary>
                /// Rotate 是日志轮转的配置键。
                /// </summary>
                internal const string Rotate = "Rotate";

                /// <summary>
                /// RotateDefault 是日志轮转的默认值。
                /// </summary>
                internal static readonly bool RotateDefault = true;

                /// <summary>
                /// Path 是日志文件路径的配置键。
                /// </summary>
                internal const string Path = "Path";

                /// <summary>
                /// PathDefault 是日志文件路径的默认值。
                /// </summary>
                internal static readonly string PathDefault = "${Environment.LocalPath}/Log/";

                /// <summary>
                /// MaxFile 是最大文件数量的配置键。
                /// </summary>
                internal const string MaxFile = "MaxFile";

                /// <summary>
                /// MaxFileDefault 是最大文件数量的默认值。
                /// </summary>
                internal static readonly int MaxFileDefault = 100;

                /// <summary>
                /// MaxLine 是单个日志文件的最大行数的配置键。
                /// </summary>
                internal const string MaxLine = "MaxLine";

                /// <summary>
                /// MaxLineDefault 是单个日志文件的最大行数的默认值。
                /// </summary>
                internal static readonly int MaxLineDefault = 1000000;

                /// <summary>
                /// MaxSize 是单个日志文件的最大大小的配置键。
                /// </summary>
                internal const string MaxSize = "MaxSize";

                /// <summary>
                /// MaxSizeDefault 是单个日志文件的最大大小的默认值。
                /// </summary>
                internal static readonly int MaxSizeDefault = 1 << 27; // 128MB

                /// <summary>
                /// Daily 是按天轮转的配置键。
                /// </summary>
                internal const string Daily = "Daily";

                /// <summary>
                /// DailyDefault 是按天轮转的默认值。
                /// </summary>
                internal static readonly bool DailyDefault = false;

                /// <summary>
                /// MaxDay 是日志保留的最大天数的配置键。
                /// </summary>
                internal const string MaxDay = "MaxDay";

                /// <summary>
                /// MaxDayDefault 是日志保留的最大天数的默认值。
                /// </summary>
                internal static readonly int MaxDayDefault = 7;

                /// <summary>
                /// Hourly 是按小时轮转的配置键。
                /// </summary>
                internal const string Hourly = "Hourly";

                /// <summary>
                /// HourlyDefault 是按小时轮转的默认值。
                /// </summary>
                internal static readonly bool HourlyDefault = false;

                /// <summary>
                /// MaxHour 是日志保留的最大小时的配置键。
                /// </summary>
                internal const string MaxHour = "MaxHour";

                /// <summary>
                /// MaxHourDefault 是日志保留的最大小时的默认值。
                /// </summary>
                internal static readonly int MaxHourDefault = 168;
            }

            /// <summary>
            /// level 是日志输出的级别。
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// rotate 表示是否启用日志轮转。
            /// </summary>
            internal bool rotate;

            /// <summary>
            /// daily 表示是否按天分割日志。
            /// </summary>
            internal bool daily;

            /// <summary>
            /// maxDay 是日志保留的最大天数。
            /// </summary>
            internal int maxDay;

            /// <summary>
            /// 是否按小时分割日志
            /// </summary>
            internal bool hourly;

            /// <summary>
            /// maxHour 是日志保留的最大小时数。
            /// </summary>
            internal int maxHour;

            /// <summary>
            /// path 是日志文件的路径。
            /// </summary>
            internal string path;

            /// <summary>
            /// maxFile 是最大日志的文件数。
            /// </summary>
            internal int maxFile;

            /// <summary>
            /// maxLine 是单个日志文件的最大行数。
            /// </summary>
            internal int maxLine;

            /// <summary>
            /// maxSize 是单个日志文件的最大大小，单位：字节。
            /// </summary>
            internal int maxSize;

            /// <summary>
            /// currentLines 是当前日志文件的行数。
            /// </summary>
            internal int currentLines;

            /// <summary>
            /// currentFileNum 是当前日志文件的序号。
            /// </summary>
            internal int currentFileNum;

            /// <summary>
            /// currentSize 是当前日志文件的大小，单位：字节。
            /// </summary>
            internal long currentSize;

            /// <summary>
            /// dailyOpenTime 是当前日志文件的创建时间（按天）。
            /// </summary>
            internal DateTime dailyOpenTime;

            /// <summary>
            /// hourlyOpenTime 是当前日志文件的创建时间（按小时）。
            /// </summary>
            internal DateTime hourlyOpenTime;

            /// <summary>
            /// prefix 是日志文件名的前缀。
            /// </summary>
            internal string prefix;

            /// <summary>
            /// suffix 是日志文件的扩展名。
            /// </summary>
            internal string suffix;

            /// <summary>
            /// logQueue 是日志写入的队列，线程安全。
            /// </summary>
            internal readonly ConcurrentQueue<LogData> logQueue = new();

            /// <summary>
            /// setupReqEvent 是写入线程初始化的请求信号。
            /// </summary>
            internal readonly AutoResetEvent setupReqEvent = new(false);

            /// <summary>
            /// setupRespEvent 是写入线程初始化的响应信号。
            /// </summary>
            internal readonly AutoResetEvent setupRespEvent = new(false);

            /// <summary>
            /// flushReqEvent 是写入线程刷新的请求信号。
            /// </summary>
            internal readonly AutoResetEvent flushReqEvent = new(false);

            /// <summary>
            /// flushRespEvent 是写入线程刷新的响应信号。
            /// </summary>
            internal readonly AutoResetEvent flushRespEvent = new(false);

            /// <summary>
            /// closeReqEvent 是写入线程关闭的请求信号。
            /// </summary>
            internal readonly AutoResetEvent closeReqEvent = new(false);

            /// <summary>
            /// closeRespEvent 是写入线程关闭的响应信号。
            /// </summary>
            internal readonly AutoResetEvent closeRespEvent = new(false);

            /// <summary>
            /// isRunning 表示写入线程的运行状态。
            /// </summary>
            internal bool isRunning;

            /// <summary>
            /// Initialize 初始化文件日志适配器。
            /// </summary>
            /// <param name="preferences">配置参数</param>
            /// <returns>日志输出级别</returns>
            public LevelType Initialize(XPrefs.IBase preferences)
            {
                if (isRunning) Close(); // 适配单元测试
                if (preferences == null) return level;

                if (!Enum.TryParse(preferences.GetString(Preferences.Level, Preferences.LevelDefault), out level))
                {
                    level = LevelType.Undefined;
                }
                rotate = preferences.GetBool(Preferences.Rotate, Preferences.RotateDefault);
                daily = preferences.GetBool(Preferences.Daily, Preferences.DailyDefault);
                maxDay = preferences.GetInt(Preferences.MaxDay, Preferences.MaxDayDefault);
                hourly = preferences.GetBool(Preferences.Hourly, Preferences.HourlyDefault);
                maxHour = preferences.GetInt(Preferences.MaxHour, Preferences.MaxHourDefault);
                path = XString.Eval(preferences.GetString(Preferences.Path, Preferences.PathDefault), XEnv.Instance);
                maxFile = preferences.GetInt(Preferences.MaxFile, Preferences.MaxFileDefault);
                maxLine = preferences.GetInt(Preferences.MaxLine, Preferences.MaxLineDefault);
                maxSize = preferences.GetInt(Preferences.MaxSize, Preferences.MaxSizeDefault);

                // 处理路径逻辑
                suffix = Path.GetExtension(path);
                if (string.IsNullOrEmpty(suffix))
                {
                    // 如果路径没有扩展名，认为是目录
                    suffix = ".log";
                    prefix = "";
                    // 确保路径以分隔符结尾
                    path = Path.Combine(path, suffix);
                }
                else
                {
                    var fileName = Path.GetFileName(path);
                    if (fileName == suffix)
                    {
                        // 如果基本名称就是后缀（如 .log），则没有文件名
                        prefix = "";
                    }
                    else
                    {
                        // 正常的文件名情况
                        prefix = Path.GetFileNameWithoutExtension(path);
                        // 确保前缀不以点结尾
                        prefix = prefix.TrimEnd('.');
                    }
                }

                path = XFile.NormalizePath(path);

                currentFileNum = 0;

                Task.Run(AsyncWrite);
                setupRespEvent.Reset();
                setupReqEvent.Set();
                setupRespEvent.WaitOne();

                return level;
            }

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            public void Write(LogData data)
            {
                if (data == null) return;
                if (data.Level > level && !data.Force) return;

                logQueue.Enqueue(data);

                // 这里主动触发一次 Flush 事件，避免内存过高
                // 尤其在 Update 循环中发生异常时，日志会瞬间增多
                // 使用近似值判断是否需要 Flush，完整的轮转逻辑由 NeedRotate 负责
                var count = logQueue.Count;
                if ((maxLine > 0 && count >= maxLine) ||
                    (maxSize > 0 && count * 10 >= maxSize))
                {
                    flushReqEvent.Set();
                }
            }

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            public void Flush()
            {
                if (!isRunning) return;

                flushRespEvent.Reset();
                flushReqEvent.Set();
                flushRespEvent.WaitOne();
            }

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            public void Close()
            {
                if (!isRunning) return;

                closeRespEvent.Reset();
                closeReqEvent.Set();
                closeRespEvent.WaitOne();
            }

            /// <summary>
            /// AsyncWrite 启动异步写入线程。
            /// </summary>
            internal void AsyncWrite()
            {
                if (setupReqEvent.WaitOne())
                {
                    NewWriter(); // 初始化 path 等信息
                    isRunning = true;
                    setupRespEvent.Set();
                }

                var signals = new WaitHandle[] { flushReqEvent, closeReqEvent };
                var builder = new StringBuilder();

                while (true)
                {
                    var signal = WaitHandle.WaitAny(signals);

                    if (signal >= 0) // Flush/Close 事件
                    {
                        if (isRunning)
                        {
                            try
                            {
                                builder.Clear();
                                // rotateError 用于控制当本批次 Rotate 发生错误时，则不进行后续的 Rotate。
                                var rotateError = false;
                                while (logQueue.TryDequeue(out var data))
                                {
                                    var line = $"[{XTime.Format(data.Time, "MM/dd HH:mm:ss.fff")}] {data.Text(true)}\n";
                                    builder.Append(line);
                                    currentLines++;
                                    LogData.Put(data);

                                    if (!rotateError && NeedRotate())
                                    {
                                        using var writer = new StreamWriter(path, append: true, Encoding.UTF8);
                                        writer.Write(builder);
                                        writer.Close();
                                        builder.Clear();

                                        currentSize = new FileInfo(path).Length; // 只在 Flush 时计算大小，提升性能，但是统计会滞后

                                        rotateError = !DoRotate();
                                    }
                                }

                                if (builder.Length > 0)
                                {
                                    using var writer = new StreamWriter(path, append: true, Encoding.UTF8);
                                    writer.Write(builder);
                                    writer.Close();
                                    builder.Clear();

                                    currentSize = new FileInfo(path).Length; // 只在 Flush 时计算大小，提升性能，但是统计会滞后
                                }
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine($"XLog.FileAdapter.AsyncWrite: write log error: {e.Message}");
                                isRunning = false;
                            }
                        }
                        else logQueue.Clear();

                        if (signal == 0) flushRespEvent.Set();
                        else if (signal == 1) // Close 事件
                        {
                            isRunning = false;
                            closeRespEvent.Set();
                        }
                    }

                    if (!isRunning) break;
                }
            }

            /// <summary>
            /// NewWriter 创建新的日志写入流。
            /// </summary>
            /// <returns>是否创建成功</returns>
            internal bool NewWriter()
            {
                try
                {
                    dailyOpenTime = DateTime.Now;
                    hourlyOpenTime = DateTime.Now;
                    XFile.CreateDirectory(Path.GetDirectoryName(path));
                    if (XFile.HasFile(path))
                    {
                        try
                        {
                            currentSize = new FileInfo(path).Length;
                            currentLines = File.ReadLines(path).Count();
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"XLog.FileAdapter.NewWriter: failed to readout initial file info: {path}. {e.Message}");
                            currentLines = 0;
                            currentSize = 0;
                        }
                    }
                    else
                    {
                        currentLines = 0;
                        currentSize = 0;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"XLog.FileAdapter.NewWriter: failed to new writer: {path}. {e.Message}");
                    return false;
                }
            }

            /// <summary>
            /// NeedRotate 检查是否需要轮转日志。
            /// </summary>
            /// <returns>是否需要轮转</returns>
            internal bool NeedRotate()
            {
                if (!rotate) return false;

                var now = DateTime.Now;
                return (maxLine > 0 && currentLines >= maxLine) ||
                       (maxSize > 0 && currentSize >= maxSize) ||
                       (daily && now.Date != dailyOpenTime.Date) ||
                       (hourly && now.Hour != hourlyOpenTime.Hour);
            }

            /// <summary>
            /// DoRotate 执行日志轮转。
            /// </summary>
            internal bool DoRotate()
            {
                var succeeded = false;
                try
                {
                    var newPath = path;
                    var format = "";
                    var openTime = DateTime.Now;

                    // 检查原始文件是否存在
                    if (!XFile.HasFile(path))
                    {
                        goto RESTART_LOGGER;
                    }

                    if (hourly)
                    {
                        format = "yyyy-MM-dd-HH";
                        openTime = hourlyOpenTime;
                    }
                    else if (daily)
                    {
                        format = "yyyy-MM-dd";
                        openTime = dailyOpenTime;
                    }

                    var num = currentFileNum + 1;
                    var dir = Path.GetDirectoryName(path);

                    // 生成轮转文件名
                    if (maxLine > 0 || maxSize > 0)
                    {
                        for (; num <= maxFile; num++)
                        {
                            string fName;
                            if (string.IsNullOrEmpty(prefix))
                            {
                                // 无文件名情况：使用序号作为文件名
                                fName = XFile.PathJoin(dir, $"{num:000}{suffix}");
                            }
                            else
                            {
                                // 有文件名情况：在原文件名后添加序号
                                if (!string.IsNullOrEmpty(format))
                                {
                                    // 按时间轮转
                                    fName = XFile.PathJoin(dir, $"{prefix}.{DateTime.Now.ToString(format)}.{num:000}{suffix}");
                                }
                                else
                                {
                                    // 按行数或大小轮转
                                    fName = XFile.PathJoin(dir, $"{prefix}.{num:000}{suffix}");
                                }
                            }

                            if (!XFile.HasFile(fName))
                            {
                                newPath = fName;
                                currentFileNum = num;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(prefix))
                        {
                            newPath = XFile.PathJoin(dir, $"{openTime.ToString(format)}.{num:000}{suffix}");
                        }
                        else
                        {
                            newPath = XFile.PathJoin(dir, $"{prefix}.{openTime.ToString(format)}.{num:000}{suffix}");
                        }
                        currentFileNum = num;
                    }

                    newPath = XFile.NormalizePath(newPath);
                    succeeded = newPath != path;
                    if (succeeded) File.Move(path, newPath);
                    else throw new Exception($"XLog.FileAdapter.DoRotate: no free log number to rename, max file count is {maxFile}.");
                }
                catch (Exception e) { Console.Error.WriteLine($"XLog.FileAdapter.DoRotate: failed to do rotate: {e.Message}"); }

            RESTART_LOGGER:
                NewWriter();
                DeleteOld();

                return succeeded;
            }

            /// <summary>
            /// DeleteOld 清理过期的日志文件。
            /// </summary>
            internal void DeleteOld()
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    var files = Directory.GetFiles(dir);
                    var now = DateTime.Now;

                    foreach (var file in files)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var fileInfo = new FileInfo(file);

                            // 检查文件是否匹配我们的模式
                            bool isMatch;
                            if (string.IsNullOrEmpty(prefix))
                            {
                                // 无前缀情况
                                isMatch = fileName.EndsWith(suffix) &&
                                    (fileName.StartsWith("[0-9][0-9][0-9]") || // 按行数轮转
                                     fileName.Contains('.')); // 按时间轮转
                            }
                            else
                            {
                                // 有前缀情况
                                isMatch = fileName.StartsWith(prefix) && fileName.EndsWith(suffix);
                            }

                            if (!isMatch) continue;

                            // 根据轮转模式检查文件时间
                            if (hourly && fileInfo.LastWriteTime.AddHours(maxHour).CompareTo(now) < 0)
                            {
                                XFile.DeleteFile(file);
                            }
                            else if (daily && fileInfo.LastWriteTime.AddDays(maxDay).CompareTo(now) < 0)
                            {
                                XFile.DeleteFile(file);
                            }
                        }
                        catch (Exception e) { Console.Error.WriteLine($"XLog.FileAdapter.DeleteOld: failed to delete log file {file}. {e.Message}"); }
                    }
                }
                catch (Exception e) { Console.Error.WriteLine($"XLog.FileAdapter.DeleteOld: failed to delete log file(s). {e.Message}"); }
            }
        }
    }
}
