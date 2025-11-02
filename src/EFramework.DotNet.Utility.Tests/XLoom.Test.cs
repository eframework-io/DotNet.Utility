// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using EFramework.DotNet.Utility;

public class TestXLoom
{
    #region 初始化
    [OneTimeSetUp]
    public void Setup()
    {
        var preferences = new XPrefs.IBase();
        preferences.Set("XLoom/Count", 2);
        preferences.Set("XLoom/Step", 10);
        preferences.Set("XLoom/Queue", 1000);
        XLoom.Initialize(preferences);
    }
    #endregion

    #region 业务线程
    [Test]
    public void Count()
    {
        Assert.That(XLoom.Count(), Is.EqualTo(2), "应当有 2 个业务线程。");
    }

    [Test]
    public async Task ID()
    {
        {
            var tcs = new TaskCompletionSource<int>();
            XLoom.RunIn(() => tcs.SetResult(XLoom.ID()), 0); // 当前线程标识
            await Task.WhenAny(tcs.Task, Task.Delay(1000));
            Assert.That(tcs.Task.Result, Is.EqualTo(0), "业务线程标识应当为 0。");
        }

        {
            var tcs = new TaskCompletionSource<int>();
            XLoom.RunIn(() => tcs.SetResult(XLoom.ID(Environment.CurrentManagedThreadId)), 1); // 特定线程标识
            await Task.WhenAny(tcs.Task, Task.Delay(1000));
            Assert.That(tcs.Task.Result, Is.EqualTo(1), "业务线程标识应当为 1。");
        }
    }

    [Test]
    public async Task RunIn()
    {
        var tcs = new TaskCompletionSource<bool>();
        XLoom.RunIn(() => tcs.SetResult(true), 0);
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.That(tcs.Task.Result, Is.True, "任务应当被执行。");
    }

    [Test]
    public async Task Pause()
    {
        var tcs = new TaskCompletionSource<bool>();
        XLoom.Pause(0);
        XLoom.RunIn(() => tcs.SetResult(true), 0);
        XLoom.Resume(0);
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.That(tcs.Task.Result, Is.True, "任务应当在 1 秒内完成。");
    }

    [Test]
    public async Task Metrics()
    {
        // 测试正常运行时的指标
        var taskCount = 100;
        for (int i = 0; i < taskCount; i++)
        {
            XLoom.RunIn(() => { }, 0);
        }

        // 等待一个完整的统计周期
        await Task.Delay(1200);

        // 验证正常运行时的指标
        var fps = XLoom.FPS(0);
        var qps = XLoom.QPS(0);

        Assert.That(fps, Is.GreaterThan(0), "FPS 应当大于 0。");
        Assert.That(qps, Is.GreaterThan(0), "QPS 应当大于 0。");

        // 测试暂停状态下的指标
        XLoom.Pause(0);

        // 尝试发送任务
        for (int i = 0; i < taskCount; i++)
        {
            XLoom.RunIn(() => { }, 0);
        }

        // 等待一个完整的统计周期
        await Task.Delay(1200);

        // 验证暂停时的指标
        Assert.That(XLoom.FPS(0), Is.EqualTo(0), "FPS 应当为 0。");
        Assert.That(XLoom.QPS(0), Is.EqualTo(0), "QPS 应当为 0。");

        // 测试恢复后的指标
        XLoom.Resume(0);

        // 等待一个完整的统计周期
        await Task.Delay(1200);

        // 验证恢复后的指标
        Assert.That(XLoom.FPS(0), Is.GreaterThan(0), "FPS 应当大于 0。");
        Assert.That(XLoom.QPS(0), Is.GreaterThan(0), "QPS 应当大于 0。");

        // 测试无效的处理器ID
        Assert.That(XLoom.FPS(-1), Is.EqualTo(0), "超出范围的 PID 应当返回 0。");
        Assert.That(XLoom.FPS(999), Is.EqualTo(0), "超出范围的 PID 应当返回 0。");
        Assert.That(XLoom.QPS(-1), Is.EqualTo(0), "超出范围的 PID 应当返回 0。");
        Assert.That(XLoom.QPS(999), Is.EqualTo(0), "超出范围的 PID 应当返回 0。");
    }
    #endregion

    #region 异步任务
    [Test]
    public async Task Async()
    {
        var executed = false;
        await XLoom.RunAsync(() => executed = true);
        Assert.That(executed, Is.True, "任务应当被执行。");

        var executeCount = 0;
        await XLoom.RunAsync(() =>
        {
            executeCount++;
            if (executeCount == 1) throw new Exception("test panic");
        }, true);
        Assert.That(executeCount, Is.EqualTo(2), "任务应当被执行 2 次。");
    }

    [Test]
    public async Task AsyncT1()
    {
        var executed = false;
        await XLoom.RunAsync(_ => executed = true, 1);
        Assert.That(executed, Is.True, "任务应当被执行。");

        var executeCount = 0;
        await XLoom.RunAsync(_ =>
        {
            executeCount++;
            if (executeCount == 1) throw new Exception("test panic");
        }, 1, true);
        Assert.That(executeCount, Is.EqualTo(2), "任务应当被执行 2 次。");
    }

    [Test]
    public async Task AsyncT2()
    {
        var executed = false;
        await XLoom.RunAsync((_, _) => executed = true, 1, 2);
        Assert.That(executed, Is.True, "任务应当被执行。");

        var executeCount = 0;
        await XLoom.RunAsync((_, _) =>
        {
            executeCount++;
            if (executeCount == 1) throw new Exception("test panic");
        }, 1, 2, true);
        Assert.That(executeCount, Is.EqualTo(2), "任务应当被执行 2 次。");
    }

    [Test]
    public async Task AsyncT3()
    {
        var executed = false;
        await XLoom.RunAsync((_, _, _) => executed = true, 1, 2, 3);
        Assert.That(executed, Is.True, "任务应当被执行。");

        var executeCount = 0;
        await XLoom.RunAsync((_, _, _) =>
        {
            executeCount++;
            if (executeCount == 1) throw new Exception("test panic");
        }, 1, 2, 3, true);
        Assert.That(executeCount, Is.EqualTo(2), "任务应当被执行 2 次。");
    }
    #endregion

    #region 定时器
    [Test]
    public void Timeout()
    {
        var tcs = new TaskCompletionSource<bool>();
        var startTime = XTime.GetMillisecond();
        var deltaTime = 0;

        var timer1 = XLoom.SetTimeout(() =>
        {
            deltaTime = (int)(XTime.GetMillisecond() - startTime);
            tcs.SetResult(true);
        }, 500, 0);

        var clearFlag = true;
        var timer2 = XLoom.SetTimeout(() => clearFlag = false, 500, 0);
        XLoom.ClearTimeout(timer2, 0);

        Assert.That(tcs.Task.Wait(TimeSpan.FromSeconds(1)), Is.True, "定时器回调应当在 1 秒内完成。");

        Assert.That(timer1, Is.GreaterThan(0), "返回的定时器应当大于 0。");
        Assert.That(deltaTime, Is.GreaterThanOrEqualTo(500), "等待时间应当大于或等于 500 毫秒。");
        Assert.That(clearFlag, Is.True, "清除的定时器应当不会被调用。");

        // 测试无效参数
        Assert.That(XLoom.SetTimeout(null, 100, 0), Is.EqualTo(-1), "空的回调函数应当返回 -1。");
        Assert.That(XLoom.SetTimeout(() => { }, -1, 0), Is.EqualTo(-1), "负的超时时间应当返回 -1。");
        Assert.That(XLoom.SetTimeout(() => { }, 100, -1), Is.EqualTo(-1), "无效的业务线程标识应当返回 -1。");
        Assert.That(XLoom.SetTimeout(() => { }, 100, 999), Is.EqualTo(-1), "超出范围的业务线程标识应当返回 -1。");
    }

    [Test]
    public void Interval()
    {
        var count = 0;
        var tcs = new TaskCompletionSource<bool>();
        var startTime = XTime.GetMillisecond();
        var deltaTime = 0;

        var interval1 = 0;
        interval1 = XLoom.SetInterval(() =>
        {
            Interlocked.Increment(ref count);
            if (count >= 3)
            {
                deltaTime = (int)(XTime.GetMillisecond() - startTime);
                XLoom.ClearInterval(interval1, 1);
                tcs.SetResult(true);
            }
            throw new Exception("test interval panic"); // 触发 panic，下一个周期的定时器应当继续执行
        }, 200, 1);

        var clearFlag = true;
        var interval2 = XLoom.SetInterval(() => clearFlag = false, 200, 1);
        XLoom.ClearInterval(interval2, 1);

        Assert.That(tcs.Task.Wait(TimeSpan.FromSeconds(1)), Is.True, "定时器回调应当在 1 秒内完成。");

        Assert.That(interval1, Is.GreaterThan(0), "返回的定时器应当大于 0。");
        Assert.That(deltaTime, Is.GreaterThanOrEqualTo(600), "等待时间应当大于或等于 600 毫秒。");
        Assert.That(clearFlag, Is.True, "清除的定时器应当不会被调用。");

        // 测试无效参数
        Assert.That(XLoom.SetInterval(null, 100, 0), Is.EqualTo(-1), "空的回调函数应当返回 -1。");
        Assert.That(XLoom.SetInterval(() => { }, -1, 0), Is.EqualTo(-1), "负的间隔时间应当返回 -1。");
        Assert.That(XLoom.SetInterval(() => { }, 100, -1), Is.EqualTo(-1), "无效的业务线程标识应当返回 -1。");
        Assert.That(XLoom.SetInterval(() => { }, 100, 999), Is.EqualTo(-1), "超出范围的业务线程标识应当返回 -1。");
    }
    #endregion
}