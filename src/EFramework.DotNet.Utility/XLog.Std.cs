// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;

namespace EFramework.DotNet.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// StdAdapter 是日志标准输出适配器，实现日志的控制台输出功能。
        /// 支持日志着色和级别过滤等特性。
        /// </summary>
        internal partial class StdAdapter : IAdapter
        {
            /// <summary>
            /// Preferences 是标准输出适配器的配置项。
            /// </summary>
            internal class Preferences
            {
                /// <summary>
                /// Config 是标准输出适配器的配置键。
                /// </summary>
                internal const string Config = "XLog/Std";

                /// <summary>
                /// ConfigDefault 是标准输出适配器的默认值。
                /// </summary>
                internal static readonly XPrefs.IBase ConfigDefault = new();

                /// <summary>
                /// Level 是日志输出级别的配置键。
                /// </summary>
                internal const string Level = "Level";

                /// <summary>
                /// LevelDefault 是日志输出级别的默认值。
                /// </summary>
                internal static readonly string LevelDefault = LevelType.Info.ToString();

                /// <summary>
                /// Color 是日志着色的配置键。
                /// </summary>
                internal const string Color = "Color";

                /// <summary>
                /// ColorDefault 是日志着色的默认值。
                /// </summary>
                internal static readonly bool ColorDefault = true;
            }

            /// <summary>
            /// LogBrush 是日志的着色器，用于生成带颜色的日志文本。
            /// </summary>
            /// <param name="color">ANSI 颜色代码</param>
            /// <returns>着色函数</returns>
            internal static Func<string, string> LogBrush(string color)
            {
                var pre = "\u001b[";  // ANSI 转义序列前缀
                var reset = "\u001b[0m";  // 重置颜色
                return (text) => pre + color + "m" + text + reset;
            }

            /// <summary>
            /// LogBrushes 是日志级别对应的着色函数数组。
            /// </summary>
            internal static readonly Func<string, string>[] LogBrushes = new Func<string, string>[] {
                LogBrush("1;39"), // Emergency          black
                LogBrush("1;36"), // Alert              cyan
                LogBrush("1;35"), // Critical           magenta
                LogBrush("1;31"), // Error              red
                LogBrush("1;33"), // Warn               yellow
                LogBrush("1;32"), // Notice             green
                LogBrush("1;30"), // Info               grey
                LogBrush("1;34"), // Debug              blue
            };

            /// <summary>
            /// level 是日志输出的级别。
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// colored 表示是否启用日志着色。
            /// </summary>
            internal bool colored;

            /// <summary>
            /// Initialize 初始化标准输出适配器。
            /// </summary>
            /// <param name="preferences">配置参数</param>
            /// <returns>日志输出级别</returns>
            public LevelType Initialize(XPrefs.IBase preferences)
            {
                if (preferences == null) return LevelType.Undefined;
                if (!Enum.TryParse(preferences.GetString(Preferences.Level, Preferences.LevelDefault), out level))
                {
                    level = LevelType.Undefined;
                }
                colored = preferences.GetBool(Preferences.Color, Preferences.ColorDefault);
                return level;
            }

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            public void Write(LogData data)
            {
                try
                {
                    if (data == null) return;
                    if (data.Level > level && !data.Force) return;
                    var text = data.Text(true);
                    if (colored)
                    {
                        var idx = (int)data.Level;
                        text = text.Replace(logLabels[idx], LogBrushes[idx](logLabels[idx]));
                    }

                    var timeStr = XTime.Format(data.Time, "MM/dd HH:mm:ss.fff");
                    var fullText = $"[{timeStr}] {text}";

                    Console.Out.WriteLine(fullText);
                }
                catch (Exception e) { Console.Error.WriteLine($"XLog.StdAdapter.Write: write log error: {e.Message}"); }
                finally { LogData.Put(data); }
            }

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            public void Flush() { }

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            public void Close() { }
        }
    }
}
