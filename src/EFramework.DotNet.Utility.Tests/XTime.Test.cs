// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXTime 是 XTime 的单元测试。
/// </summary>
public class TestXTime
{
    private DateTime testDateTime;
    private int testTimestamp;
    private long testMilliseconds;

    [SetUp]
    public void Setup()
    {
        // 准备测试数据：2024年3月21日14时30分0秒123毫秒
        testDateTime = new DateTime(2024, 3, 21, 14, 30, 0, 123);
        testTimestamp = (int)(testDateTime - XTime.Initial).TotalSeconds;
        testMilliseconds = (long)(testDateTime - XTime.Initial).TotalMilliseconds;
    }

    [Test]
    public void Constants()
    {
        // 准备测试数据
        var expectedMinute = XTime.Second1 * 60;
        var expectedHour = XTime.Minute1 * 60;
        var expectedDay = XTime.Hour1 * 24;

        // 验证基本时间单位常量
        Assert.That(XTime.Second1, Is.EqualTo(1), "一秒应等于1秒");
        Assert.That(XTime.Minute1, Is.EqualTo(expectedMinute), "一分钟应等于60秒");
        Assert.That(XTime.Hour1, Is.EqualTo(expectedHour), "一小时应等于3600秒");
        Assert.That(XTime.Day1, Is.EqualTo(expectedDay), "一天应等于86400秒");

        // 验证复合时间常量
        Assert.That(XTime.Minute30, Is.EqualTo(XTime.Minute1 * 30), "30分钟应等于1800秒");
        Assert.That(XTime.Hour12, Is.EqualTo(XTime.Hour1 * 12), "12小时应等于43200秒");
        Assert.That(XTime.Day7, Is.EqualTo(XTime.Day1 * 7), "7天应等于604800秒");
    }

    [Test]
    public void GetTimestamp()
    {
        // 准备测试数据
        var now = DateTime.Now;
        var expectedTimestamp = (int)(now - XTime.Initial).TotalSeconds;

        // 获取当前时间戳
        var timestamp = XTime.GetTimestamp();

        // 验证时间戳在1秒误差范围内
        Assert.That(Math.Abs(timestamp - expectedTimestamp), Is.LessThanOrEqualTo(1), "获取的时间戳应在1秒误差范围内");
    }

    [Test]
    public void GetMillisecond()
    {
        // 准备测试数据
        var now = DateTime.Now;
        var expectedMilliseconds = (long)(now - XTime.Initial).TotalMilliseconds;

        // 获取当前毫秒时间戳
        var milliseconds = XTime.GetMillisecond();

        // 验证毫秒时间戳在100毫秒误差范围内
        Assert.That(Math.Abs(milliseconds - expectedMilliseconds), Is.LessThanOrEqualTo(100), "获取的毫秒时间戳应在100毫秒误差范围内");
    }

    [Test]
    public void NowTime()
    {
        // 准备测试数据
        var now = DateTime.Now;

        // 获取当前时间
        var time = XTime.NowTime();

        // 验证时间在100毫秒误差范围内
        Assert.That((time - now).TotalMilliseconds, Is.LessThanOrEqualTo(100), "获取的当前时间应在100毫秒误差范围内");
    }

    [Test]
    public void ToTime()
    {
        // 准备测试数据
        var expectedTime = testDateTime;

        // 时间戳转DateTime
        var time = XTime.ToTime(testTimestamp);

        // 验证转换后的时间各个部分
        Assert.That(time.Year, Is.EqualTo(expectedTime.Year), "年份应匹配");
        Assert.That(time.Month, Is.EqualTo(expectedTime.Month), "月份应匹配");
        Assert.That(time.Day, Is.EqualTo(expectedTime.Day), "日期应匹配");
        Assert.That(time.Hour, Is.EqualTo(expectedTime.Hour), "小时应匹配");
        Assert.That(time.Minute, Is.EqualTo(expectedTime.Minute), "分钟应匹配");
        Assert.That(time.Second, Is.EqualTo(expectedTime.Second), "秒数应匹配");
    }

    [Test]
    public void TimeToZero()
    {
        // 准备测试数据
        var timestamp = testTimestamp;
        var expectedSeconds = XTime.Day1 - (timestamp + XTime.Hour8) % XTime.Day1;

        // 计算距离下个零点的秒数
        var secondsToZero = XTime.TimeToZero(timestamp);

        // 验证计算结果
        Assert.That(secondsToZero, Is.EqualTo(expectedSeconds), "距离下个零点的秒数计算应正确");
    }

    [Test]
    public void ZeroTime()
    {
        // 准备测试数据
        var timestamp = testTimestamp;
        var expectedZeroTimestamp = timestamp - (timestamp + XTime.Hour8) % XTime.Day1;

        // 获取当天零点时间戳
        var zeroTimestamp = XTime.ZeroTime(timestamp);

        // 验证零点时间戳
        Assert.That(zeroTimestamp, Is.EqualTo(expectedZeroTimestamp), "当天零点时间戳计算应正确");
    }

    [Test]
    public void Format()
    {
        // 准备测试数据
        var expectedDateTimeStr = "2024-03-21 14:30:00";
        var expectedDateStr = "2024-03-21";
        var expectedMillisStr = "2024-03-21 14:30:00.123";
        var expectedChineseStr = "2024年03月21日";

        // 验证秒级时间戳格式化
        Assert.That(XTime.Format(testTimestamp), Is.EqualTo(expectedDateTimeStr), "默认格式的秒级时间戳格式化应正确");
        Assert.That(XTime.Format(testTimestamp, "yyyy-MM-dd"), Is.EqualTo(expectedDateStr), "自定义格式的秒级时间戳格式化应正确");

        // 验证毫秒级时间戳格式化
        Assert.That(XTime.Format(testMilliseconds), Is.EqualTo(expectedMillisStr), "默认格式的毫秒级时间戳格式化应正确");
        Assert.That(XTime.Format(testMilliseconds, "yyyy-MM-dd HH:mm"), Is.EqualTo("2024-03-21 14:30"), "自定义格式的毫秒级时间戳格式化应正确");

        // 验证DateTime格式化
        Assert.That(XTime.Format(testDateTime), Is.EqualTo(expectedMillisStr), "默认格式的DateTime格式化应正确");
        Assert.That(XTime.Format(testDateTime, "yyyy年MM月dd日"), Is.EqualTo(expectedChineseStr), "中文格式的DateTime格式化应正确");
    }
}
