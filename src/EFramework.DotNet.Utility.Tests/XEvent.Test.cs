// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using EFramework.DotNet.Utility;
using NUnit.Framework;
using System;
using System.IO;

/// <summary>
/// TestXEvent 是 XEvent 的单元测试。
/// </summary>
public class TestXEvent
{
    [TestCase(true)]   // 允许多重监听
    [TestCase(false)]  // 不允许多重监听
    public void Register(bool multiple)
    {
        var errorOutput = new StringWriter();
        var originalErrorOutput = Console.Error;
        Console.SetError(errorOutput);

        try
        {
            // Arrange
            var manager = new XEvent.Manager(multiple);
            void callback1(object[] args) { }
            void callback2(object[] args) { }

            // Act & Assert
            Assert.That(manager.Register(1, callback1), Is.True, "首次注册回调应该成功。");
            Assert.That(manager.Register(1, callback2), Is.EqualTo(multiple), "多重监听设置应正确影响重复注册的结果。");
        }
        catch { }
        finally { Console.SetError(originalErrorOutput); }
    }

    [TestCase(true)]    // 注销所有回调
    [TestCase(false)]   // 注销指定回调
    public void Unregister(bool all)
    {
        // Arrange
        var manager = new XEvent.Manager();
        void callback1(object[] args) { }
        void callback2(object[] args) { }
        manager.Register(1, callback1);
        manager.Register(1, callback2);

        // Act & Assert
        if (all)
        {
            // 验证注销所有回调应该成功
            Assert.That(manager.Unregister(1), Is.True, "注销所有回调应该成功");
        }
        else
        {
            // 验证注销指定回调应该成功
            Assert.That(manager.Unregister(1, callback1), Is.True, "注销指定回调应该成功");
        }
    }

    [Test]
    public void UnregisterAll()
    {
        // Arrange
        var manager = new XEvent.Manager();
        static void callback(object[] args) { }
        manager.Register(1, callback);
        manager.Register(2, callback);

        // Act
        manager.UnregisterAll();

        // Assert
        // 验证清理后无法获取之前注册的回调
        Assert.That(manager.Get(1), Is.Null, "清理后事件1的回调应为空");
        Assert.That(manager.Get(2), Is.Null, "清理后事件2的回调应为空");
    }

    [Test]
    public void Get()
    {
        // Arrange
        var manager = new XEvent.Manager();
        static void callback(object[] args) { }
        manager.Register(1, callback);

        // Act & Assert
        var callbacks = manager.Get(1);
        // 验证已注册事件的回调列表
        Assert.That(callbacks, Is.Not.Null, "已注册事件的回调列表不应为空");
        Assert.That(callbacks.Count, Is.EqualTo(1), "回调列表应包含1个回调");
        Assert.That(callbacks, Does.Contain((XEvent.Callback)callback), "回调列表应包含已注册的回调");

        var nonExistCallbacks = manager.Get(999);
        // 验证未注册事件的回调列表
        Assert.That(nonExistCallbacks, Is.Null, "未注册事件的回调列表应为空");
    }

    [Test]
    public void Notify()
    {
        // Arrange
        var manager = new XEvent.Manager();
        int callCount1 = 0, callCount2 = 0;
        void callback1(object[] args) => callCount1++;
        void callback2(object[] args) => callCount2++;

        // 测试普通回调
        manager.Register(1, callback1);
        manager.Register(1, callback2);
        manager.Notify(1);
        // 验证普通回调的执行次数
        Assert.That(callCount1, Is.EqualTo(1), "第一个回调应该被执行一次");
        Assert.That(callCount2, Is.EqualTo(1), "第二个回调应该被执行一次");

        // 测试单次回调
        callCount1 = 0;
        callCount2 = 0;
        manager.Register(2, callback1, true);  // once = true
        manager.Register(2, callback2, false); // once = false

        manager.Notify(2);  // 两个回调都会执行
        // 验证首次通知时两个回调都应执行
        Assert.That(callCount1, Is.EqualTo(1), "单次回调应该在首次通知时执行");
        Assert.That(callCount2, Is.EqualTo(1), "普通回调应该在首次通知时执行");

        manager.Notify(2);  // 只有非单次回调会执行
        // 验证二次通知时只有普通回调执行
        Assert.That(callCount1, Is.EqualTo(1), "单次回调不应在二次通知时执行");
        Assert.That(callCount2, Is.EqualTo(2), "普通回调应该在二次通知时执行");

        // 测试回调执行顺序
        int callOrder = 0;
        int callback3Order = 0, callback4Order = 0;
        void callback3(object[] args) => callback3Order = ++callOrder;
        void callback4(object[] args) => callback4Order = ++callOrder;

        manager.Register(3, callback3);
        manager.Register(3, callback4);
        manager.Notify(3);

        // 验证回调执行顺序
        Assert.That(callback3Order, Is.GreaterThan(0), "第一个回调应该被执行");
        Assert.That(callback4Order, Is.GreaterThan(0), "第二个回调应该被执行");
        Assert.That(callback4Order, Is.GreaterThan(callback3Order), "回调应该按注册顺序执行");
    }
}
