// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXLogTag 是 XLog.Tag 的单元测试。
/// </summary>
public class TestXLogTag
{
    [Test]
    public void Basic()
    {
        // 测试基本的Set/Get操作
        var tag = XLog.GetTag();
        tag.Set("key1", "value1");
        tag.Set("key2", "value2");

        Assert.That(tag.Get("key1"), Is.EqualTo("value1"), "期望 key1 的值为 'value1'");
        Assert.That(tag.Get("key2"), Is.EqualTo("value2"), "期望 key2 的值为 'value2'");
        Assert.That(tag.Get("nonexistent"), Is.EqualTo(""), "期望不存在的键返回空字符串");

        // 测试文本格式化
        Assert.That(tag.Text, Is.EqualTo("[key1=value1, key2=value2]"), "期望标签文本格式正确");

        // 测试数据字典
        var data = tag.Data;
        Assert.That(data.Count, Is.EqualTo(2), "期望字典包含2个键值对");
        Assert.That(data["key1"], Is.EqualTo("value1"), "期望字典中 key1 的值为 'value1'");
        Assert.That(data["key2"], Is.EqualTo("value2"), "期望字典中 key2 的值为 'value2'");

        // 测试日志级别
        tag.Level = XLog.LevelType.Debug;
        Assert.That(tag.Level, Is.EqualTo(XLog.LevelType.Debug), "期望日志级别设置为 Debug");

        tag.Level = XLog.LevelType.Info;
        Assert.That(tag.Level, Is.EqualTo(XLog.LevelType.Info), "期望日志级别设置为 Info");

        // 测试克隆功能
        var clonedTag = tag.Clone();
        Assert.That(clonedTag.Get("key1"), Is.EqualTo("value1"), "期望克隆标签包含原始标签的 key1 值");
        Assert.That(clonedTag.Get("key2"), Is.EqualTo("value2"), "期望克隆标签包含原始标签的 key2 值");
        Assert.That(clonedTag.Text, Is.EqualTo(tag.Text), "期望克隆标签的文本表示与原始标签相同");

        clonedTag.Set("key3", "value3");
        Assert.That(tag.Get("key3"), Is.EqualTo(""), "期望原始标签不受克隆标签修改的影响");
        Assert.That(clonedTag.Get("key3"), Is.EqualTo("value3"), "期望克隆标签可以独立设置新的键值对");

        // 测试空标签
        var emptyTag = XLog.GetTag();
        Assert.That(emptyTag.Text, Is.EqualTo(""), "期望空标签的文本表示为空字符串");
        Assert.That(emptyTag.Data.Count, Is.EqualTo(0), "期望空标签的字典为空");

        // 清理资源
        XLog.PutTag(tag);
        XLog.PutTag(clonedTag);
        XLog.PutTag(emptyTag);
    }

    [Test]
    public void Context()
    {
        var threadCount = 4;
        var tasks = new Task[threadCount];
        var exceptions = new List<Exception>();
        var lockObj = new object();

        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    // 每个线程设置自己的tag
                    var tag = XLog.GetTag();
                    tag.Set("thread", $"thread_{threadId}");
                    XLog.Watch(tag);

                    // 验证每个线程都能获取到自己的tag
                    var myTag = XLog.Tag();
                    Assert.That(myTag, Is.SameAs(tag));
                    Assert.That(myTag.Get("thread"), Is.EqualTo($"thread_{threadId}"));

                    // 添加更多的键值对，验证不会影响其他线程
                    myTag = XLog.Tag("key1", $"value1_{threadId}", "key2", $"value2_{threadId}");
                    Assert.That(myTag, Is.SameAs(tag));
                    Assert.That(myTag.Get("thread"), Is.EqualTo($"thread_{threadId}"));
                    Assert.That(myTag.Get("key1"), Is.EqualTo($"value1_{threadId}"));
                    Assert.That(myTag.Get("key2"), Is.EqualTo($"value2_{threadId}"));

                    // 清理当前线程的tag
                    XLog.Defer();
                    Assert.That(XLog.Tag(), Is.Null);
                }
                catch (Exception ex)
                {
                    lock (lockObj) exceptions.Add(ex);
                }
            });
        }

        // 等待所有线程完成
        Task.WaitAll(tasks);

        // 检查是否有异常发生
        Assert.That(exceptions, Is.Empty, "并发测试过程中不应出现异常");
    }
}
