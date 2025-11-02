// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XTime 提供了一组时间常量定义及工具函数，支持时间戳转换和格式化等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 提供秒、分钟、小时、天的常量定义
    /// - 提供时间戳（秒/毫秒）的获取方法
    /// - 提供时间格式化和转换功能
    /// - 提供零点时间相关的计算功能
    /// 
    /// 使用手册
    /// 1. 时间常量
    /// 
    ///     时间常量提供了从秒到天的时间单位定义，所有常量均以秒为基本单位。
    /// 
    ///     // 基本单位示例
    ///     var oneSecond = XTime.Second1;        // 1秒
    ///     var oneMinute = XTime.Minute1;        // 60秒
    ///     var oneHour = XTime.Hour1;            // 3600秒
    ///     var oneDay = XTime.Day1;              // 86400秒
    /// 
    /// 2. 时间戳操作
    /// 
    ///     提供秒级和毫秒级时间戳的获取方法，基于 1970-01-01 计算。
    /// 
    ///     // 获取当前时间戳
    ///     var timestamp = XTime.GetTimestamp();  // 获取秒级时间戳
    ///     var millis = XTime.GetMillisecond();  // 获取毫秒级时间戳
    /// 
    /// 3. 时间转换
    /// 
    ///     支持时间戳与 DateTime 之间的相互转换。
    /// 
    ///     // 时间戳转换示例
    ///     var dateTime = XTime.ToTime(1234567890);    // 时间戳转DateTime
    ///     var now = XTime.NowTime();                  // 获取当前DateTime
    /// 
    /// 4. 零点时间计算
    /// 
    ///     提供基于本地时区的零点时间计算功能。
    /// 
    ///     // 零点计算示例
    ///     var secondsToZero = XTime.TimeToZero();     // 距离下个零点的秒数
    ///     var zeroTimestamp = XTime.ZeroTime();       // 获取当天零点时间戳
    /// 
    /// 5. 时间格式化
    /// 
    ///     支持多种格式的时间格式化，可自定义格式字符串。
    /// 
    ///     // 格式化示例
    ///     XTime.Format(timestamp);                    // "2024-03-21 14:30:00"
    ///     XTime.Format(millis);                       // "2024-03-21 14:30:00.123"
    ///     XTime.Format(DateTime.Now);                 // "2024-03-21 14:30:00.123"
    ///     XTime.Format(timestamp, "yyyy-MM-dd");      // "2024-03-21"
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class XTime
    {
        /// <summary>
        /// FormatStandard 是标准的日期格式化模版（含时区信息），示例：2025-10-13 15:04:05 +0800 CST。
        /// </summary>
        public const string FormatStandard = "yyyy-MM-dd HH:mm:ss zzz";

        /// <summary>
        /// FormatSimple 是简易的日期格式化模版（不含时区），示例：2025-10-13 15:04:05。
        /// </summary>
        public const string FormatSimple = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// FormatCompact 是紧凑的日期格式化模版（适合文件名），示例：2025-10-13_15_04_05。
        /// </summary>
        public const string FormatCompact = "yyyy-MM-dd_HH_mm_ss";

        /// <summary>
        /// Second1 表示 1 秒的时间常量，值为 1。
        /// </summary>
        public const int Second1 = 1;

        /// <summary>
        /// Second2 表示 2 秒的时间常量，值为 2。
        /// </summary>
        public const int Second2 = 2;

        /// <summary>
        /// Second3 表示 3 秒的时间常量，值为 3。
        /// </summary>
        public const int Second3 = 3;

        /// <summary>
        /// Second4 表示 4 秒的时间常量，值为 4。
        /// </summary>
        public const int Second4 = 4;

        /// <summary>
        /// Second5 表示 5 秒的时间常量，值为 5。
        /// </summary>
        public const int Second5 = 5;

        /// <summary>
        /// Second6 表示 6 秒的时间常量，值为 6。
        /// </summary>
        public const int Second6 = 6;

        /// <summary>
        /// Second7 表示 7 秒的时间常量，值为 7。
        /// </summary>
        public const int Second7 = 7;

        /// <summary>
        /// Second8 表示 8 秒的时间常量，值为 8。
        /// </summary>
        public const int Second8 = 8;

        /// <summary>
        /// Second9 表示 9 秒的时间常量，值为 9。
        /// </summary>
        public const int Second9 = 9;

        /// <summary>
        /// Second10 表示 10 秒的时间常量，值为 10。
        /// </summary>
        public const int Second10 = 10;

        /// <summary>
        /// Second15 表示 15 秒的时间常量，值为 15。
        /// </summary>
        public const int Second15 = 15;

        /// <summary>
        /// Second20 表示 20 秒的时间常量，值为 20。
        /// </summary>
        public const int Second20 = 20;

        /// <summary>
        /// Second25 表示 25 秒的时间常量，值为 25。
        /// </summary>
        public const int Second25 = 25;

        /// <summary>
        /// Second30 表示 30 秒的时间常量，值为 30。
        /// </summary>
        public const int Second30 = 30;

        /// <summary>
        /// Second35 表示 35 秒的时间常量，值为 35。
        /// </summary>
        public const int Second35 = 35;

        /// <summary>
        /// Second40 表示 40 秒的时间常量，值为 40。
        /// </summary>
        public const int Second40 = 40;

        /// <summary>
        /// Second45 表示 45 秒的时间常量，值为 45。
        /// </summary>
        public const int Second45 = 45;

        /// <summary>
        /// Second50 表示 50 秒的时间常量，值为 50。
        /// </summary>
        public const int Second50 = 50;

        /// <summary>
        /// Second55 表示 55 秒的时间常量，值为 55。
        /// </summary>
        public const int Second55 = 55;

        /// <summary>
        /// Minute1 表示 1 分钟的时间常量，值为 60。
        /// </summary>
        public const int Minute1 = 60;

        /// <summary>
        /// Minute2 表示 2 分钟的时间常量，值为 120。
        /// </summary>
        public const int Minute2 = 120;

        /// <summary>
        /// Minute3 表示 3 分钟的时间常量，值为 180。
        /// </summary>
        public const int Minute3 = 180;

        /// <summary>
        /// Minute4 表示 4 分钟的时间常量，值为 240。
        /// </summary>
        public const int Minute4 = 240;

        /// <summary>
        /// Minute5 表示 5 分钟的时间常量，值为 300。
        /// </summary>
        public const int Minute5 = 300;

        /// <summary>
        /// Minute6 表示 6 分钟的时间常量，值为 360。
        /// </summary>
        public const int Minute6 = 360;

        /// <summary>
        /// Minute7 表示 7 分钟的时间常量，值为 420。
        /// </summary>
        public const int Minute7 = 420;

        /// <summary>
        /// Minute8 表示 8 分钟的时间常量，值为 480。
        /// </summary>
        public const int Minute8 = 480;

        /// <summary>
        /// Minute9 表示 9 分钟的时间常量，值为 540。
        /// </summary>
        public const int Minute9 = 540;

        /// <summary>
        /// Minute10 表示 10 分钟的时间常量，值为 600。
        /// </summary>
        public const int Minute10 = 600;

        /// <summary>
        /// Minute12 表示 12 分钟的时间常量，值为 720。
        /// </summary>
        public const int Minute12 = 720;

        /// <summary>
        /// Minute15 表示 15 分钟的时间常量，值为 900。
        /// </summary>
        public const int Minute15 = 900;

        /// <summary>
        /// Minute20 表示 20 分钟的时间常量，值为 1200。
        /// </summary>
        public const int Minute20 = 1200;

        /// <summary>
        /// Minute25 表示 25 分钟的时间常量，值为 1500。
        /// </summary>
        public const int Minute25 = 1500;

        /// <summary>
        /// Minute30 表示 30 分钟的时间常量，值为 1800。
        /// </summary>
        public const int Minute30 = 1800;

        /// <summary>
        /// Minute35 表示 35 分钟的时间常量，值为 2100。
        /// </summary>
        public const int Minute35 = 2100;

        /// <summary>
        /// Minute40 表示 40 分钟的时间常量，值为 2400。
        /// </summary>
        public const int Minute40 = 2400;

        /// <summary>
        /// Minute45 表示 45 分钟的时间常量，值为 2700。
        /// </summary>
        public const int Minute45 = 2700;

        /// <summary>
        /// Minute50 表示 50 分钟的时间常量，值为 3000。
        /// </summary>
        public const int Minute50 = 3000;

        /// <summary>
        /// Minute55 表示 55 分钟的时间常量，值为 3300。
        /// </summary>
        public const int Minute55 = 3300;

        /// <summary>
        /// Hour1 表示 1 小时的时间常量，值为 3600。
        /// </summary>
        public const int Hour1 = 3600;

        /// <summary>
        /// Hour2 表示 2 小时的时间常量，值为 7200。
        /// </summary>
        public const int Hour2 = 7200;

        /// <summary>
        /// Hour3 表示 3 小时的时间常量，值为 10800。
        /// </summary>
        public const int Hour3 = 10800;

        /// <summary>
        /// Hour4 表示 4 小时的时间常量，值为 14400。
        /// </summary>
        public const int Hour4 = 14400;

        /// <summary>
        /// Hour5 表示 5 小时的时间常量，值为 18000。
        /// </summary>
        public const int Hour5 = 18000;

        /// <summary>
        /// Hour6 表示 6 小时的时间常量，值为 21600。
        /// </summary>
        public const int Hour6 = 21600;

        /// <summary>
        /// Hour7 表示 7 小时的时间常量，值为 25200。
        /// </summary>
        public const int Hour7 = 25200;

        /// <summary>
        /// Hour8 表示 8 小时的时间常量，值为 28800。
        /// </summary>
        public const int Hour8 = 28800;

        /// <summary>
        /// Hour9 表示 9 小时的时间常量，值为 32400。
        /// </summary>
        public const int Hour9 = 32400;

        /// <summary>
        /// Hour10 表示 10 小时的时间常量，值为 36000。
        /// </summary>
        public const int Hour10 = 36000;

        /// <summary>
        /// Hour11 表示 11 小时的时间常量，值为 39600。
        /// </summary>
        public const int Hour11 = 39600;

        /// <summary>
        /// Hour12 表示 12 小时的时间常量，值为 43200。
        /// </summary>
        public const int Hour12 = 43200;

        /// <summary>
        /// Hour13 表示 13 小时的时间常量，值为 46800。
        /// </summary>
        public const int Hour13 = 46800;

        /// <summary>
        /// Hour14 表示 14 小时的时间常量，值为 50400。
        /// </summary>
        public const int Hour14 = 50400;

        /// <summary>
        /// Hour15 表示 15 小时的时间常量，值为 54000。
        /// </summary>
        public const int Hour15 = 54000;

        /// <summary>
        /// Hour16 表示 16 小时的时间常量，值为 57600。
        /// </summary>
        public const int Hour16 = 57600;

        /// <summary>
        /// Hour17 表示 17 小时的时间常量，值为 61200。
        /// </summary>
        public const int Hour17 = 61200;

        /// <summary>
        /// Hour18 表示 18 小时的时间常量，值为 64800。
        /// </summary>
        public const int Hour18 = 64800;

        /// <summary>
        /// Hour19 表示 19 小时的时间常量，值为 68400。
        /// </summary>
        public const int Hour19 = 68400;

        /// <summary>
        /// Hour20 表示 20 小时的时间常量，值为 72000。
        /// </summary>
        public const int Hour20 = 72000;

        /// <summary>
        /// Hour21 表示 21 小时的时间常量，值为 75600。
        /// </summary>
        public const int Hour21 = 75600;

        /// <summary>
        /// Hour22 表示 22 小时的时间常量，值为 79200。
        /// </summary>
        public const int Hour22 = 79200;

        /// <summary>
        /// Hour23 表示 23 小时的时间常量，值为 82800。
        /// </summary>
        public const int Hour23 = 82800;

        /// <summary>
        /// Day1 表示 1 天的时间常量，值为 86400。
        /// </summary>
        public const int Day1 = 86400;

        /// <summary>
        /// Day2 表示 2 天的时间常量，值为 172800。
        /// </summary>
        public const int Day2 = 172800;

        /// <summary>
        /// Day3 表示 3 天的时间常量，值为 259200。
        /// </summary>
        public const int Day3 = 259200;

        /// <summary>
        /// Day4 表示 4 天的时间常量，值为 345600。
        /// </summary>
        public const int Day4 = 345600;

        /// <summary>
        /// Day5 表示 5 天的时间常量，值为 432000。
        /// </summary>
        public const int Day5 = 432000;

        /// <summary>
        /// Day6 表示 6 天的时间常量，值为 518400。
        /// </summary>
        public const int Day6 = 518400;

        /// <summary>
        /// Day7 表示 7 天的时间常量，值为 604800。
        /// </summary>
        public const int Day7 = 604800;

        /// <summary>
        /// Day8 表示 8 天的时间常量，值为 691200。
        /// </summary>
        public const int Day8 = 691200;

        /// <summary>
        /// Day9 表示 9 天的时间常量，值为 777600。
        /// </summary>
        public const int Day9 = 777600;

        /// <summary>
        /// Day10 表示 10 天的时间常量，值为 864000。
        /// </summary>
        public const int Day10 = 864000;

        /// <summary>
        /// Day15 表示 15 天的时间常量，值为 1296000。
        /// </summary>
        public const int Day15 = 1296000;

        /// <summary>
        /// Day20 表示 20 天的时间常量，值为 1728000。
        /// </summary>
        public const int Day20 = 1728000;

        /// <summary>
        /// Day30 表示 30 天的时间常量，值为 2592000。
        /// </summary>
        public const int Day30 = 2592000;

        /// <summary>
        /// Initial 表示初始时间，1970-01-01，已根据本地时区进行调整。
        /// </summary>
        public static DateTime Initial = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.BaseUtcOffset;

        /// <summary>
        /// GetTimestamp 获取当前时间的秒级时间戳。
        /// </summary>
        /// <returns>从1970年1月1日至今的秒数。</returns>
        /// <remarks>
        /// 时间戳基于本地时区计算，已考虑时区偏移。
        /// </remarks>
        public static int GetTimestamp()
        {
            var ts = DateTime.Now - Initial;
            return Convert.ToInt32(ts.TotalSeconds);
        }

        /// <summary>
        /// GetMillisecond 获取当前时间的毫秒级时间戳。
        /// </summary>
        /// <returns>从1970年1月1日至今的毫秒数。</returns>
        /// <remarks>
        /// 时间戳基于本地时区计算，已考虑时区偏移。
        /// </remarks>
        public static long GetMillisecond()
        {
            var ts = DateTime.Now - Initial;
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        /// <summary>
        /// NowTime 获取当前系统时间。
        /// </summary>
        /// <returns>当前的 DateTime 对象。</returns>
        /// <remarks>
        /// 直接返回 DateTime.Now，包含日期和时间信息。
        /// </remarks>
        public static DateTime NowTime() { return DateTime.Now; }

        /// <summary>
        /// ToTime 将秒级时间戳转换为 DateTime 对象。
        /// </summary>
        /// <param name="timestamp">秒级时间戳。</param>
        /// <returns>对应的 DateTime 对象。</returns>
        /// <remarks>
        /// 基于初始时间（1970-01-01）进行计算，考虑了本地时区。
        /// </remarks>
        public static DateTime ToTime(int timestamp) { return Initial.AddSeconds(timestamp); }

        /// <summary>
        /// TimeToZero 计算距离下一个零点的秒数。
        /// </summary>
        /// <param name="timestamp">指定时间戳，默认使用当前时间。</param>
        /// <returns>距离下一个零点的秒数。</returns>
        /// <remarks>
        /// 考虑了时区偏移（+8小时），确保在本地时间计算零点。
        /// </remarks>
        public static int TimeToZero(int timestamp = -1)
        {
            var t = timestamp;
            if (timestamp == -1) t = GetTimestamp();
            return Day1 - (t + Hour8) % Day1;
        }

        /// <summary>
        /// ZeroTime 获取指定时间当天零点的时间戳。
        /// </summary>
        /// <param name="timestamp">指定时间戳，默认使用当前时间。</param>
        /// <returns>当天零点的秒级时间戳。</returns>
        /// <remarks>
        /// 考虑了时区偏移（+8小时），确保在本地时间计算零点。
        /// </remarks>
        public static int ZeroTime(int timestamp = -1)
        {
            var t = timestamp;
            if (timestamp == -1) t = GetTimestamp();
            return t - ((t + Hour8) % Day1);
        }

        /// <summary>
        /// Format 格式化秒级时间戳为字符串。
        /// </summary>
        /// <param name="timestamp">秒级时间戳。</param>
        /// <param name="format">日期时间格式字符串，默认为 "yyyy-MM-dd HH:mm:ss"。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public static string Format(int timestamp, string format = "yyyy-MM-dd HH:mm:ss") { return Initial.AddSeconds(timestamp).ToString(format); }

        /// <summary>
        /// Format 格式化毫秒级时间戳为字符串。
        /// </summary>
        /// <param name="timestamp">毫秒级时间戳。</param>
        /// <param name="format">日期时间格式字符串，默认为 "yyyy-MM-dd HH:mm:ss.fff"。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public static string Format(long timestamp, string format = "yyyy-MM-dd HH:mm:ss.fff") { return Initial.AddMilliseconds(timestamp).ToString(format); }

        /// <summary>
        /// Format 格式化 DateTime 对象为字符串。
        /// </summary>
        /// <param name="time">DateTime 对象。</param>
        /// <param name="format">日期时间格式字符串，默认为 "yyyy-MM-dd HH:mm:ss.fff"。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public static string Format(DateTime time, string format = "yyyy-MM-dd HH:mm:ss.fff") { return time.ToString(format); }
    }
}
