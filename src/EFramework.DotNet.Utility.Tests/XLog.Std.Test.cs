// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXLogStd 是 XLog.Std 的单元测试。
/// </summary>
public class TestXLogStd
{
    private XLog.StdAdapter adapter;

    private XPrefs.IBase preferences;

    private StringWriter redirectedOutput;

    private TextWriter originalOutput;

    [SetUp]
    public void Setup()
    {
        adapter = new XLog.StdAdapter();
        preferences = new XPrefs.IBase();
        preferences.Set(XLog.StdAdapter.Preferences.Color, false);
        redirectedOutput = new StringWriter();
        originalOutput = Console.Out;
        Console.SetOut(redirectedOutput);
    }

    [TearDown]
    public void Reset()
    {
        adapter = null;
        preferences = null;
        redirectedOutput.Dispose();
        Console.SetOut(originalOutput);
    }

    [Test]
    public void Initialize()
    {
        // 测试默认配置
        Assert.That(adapter.Initialize(preferences), Is.EqualTo(XLog.LevelType.Info), "期望默认日志级别为 Info");

        // 测试自定义日志级别
        preferences.Set(XLog.StdAdapter.Preferences.Level, XLog.LevelType.Debug.ToString());
        Assert.That(adapter.Initialize(preferences), Is.EqualTo(XLog.LevelType.Debug), "期望自定义日志级别设置为 Debug");

        // 测试无效日志级别
        preferences.Set(XLog.StdAdapter.Preferences.Level, "InvalidLevel");
        Assert.That(adapter.Initialize(preferences), Is.EqualTo(XLog.LevelType.Undefined), "期望无效日志级别返回 Undefined");
    }

    [Test]
    public void Write()
    {
        // 1. 测试不同级别的日志输出
        adapter.Initialize(preferences);

        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Test info message" });
        Assert.That(redirectedOutput.ToString(), Does.Match(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Test info message"));

        // 2. 测试带标签的日志
        var tag = XLog.GetTag();
        tag.Set("key", "value");
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Tagged message", Tag = tag.Text });
        Assert.That(redirectedOutput.ToString(), Does.Match(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] \[key=value\] Tagged message"));
        XLog.PutTag(tag);

        // 3. 测试强制输出
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Forced message", Force = true });
        Assert.That(redirectedOutput.ToString(), Does.Match(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Forced message"));

        // 4. 测试日志级别过滤
        preferences.Set(XLog.StdAdapter.Preferences.Level, XLog.LevelType.Error.ToString());
        adapter.Initialize(preferences);
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Should not appear" });
    }

    [Test]
    public void Color()
    {
        // 1. 测试彩色输出
        adapter.Initialize(preferences);
        adapter.colored = true;
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Colored message" });
        Assert.That(redirectedOutput.ToString(), Does.Match(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \u001b\[1;30m\[I\]\u001b\[0m Colored message"));

        // 2. 测试禁用彩色输出
        adapter.Initialize(preferences);
        adapter.colored = false;
        adapter.Write(new XLog.LogData { Level = XLog.LevelType.Info, Data = "Non-colored message" });
        Assert.That(redirectedOutput.ToString(), Does.Match(@"\[\d{2}/\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[I\] Non-colored message"));
    }
}
