// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Threading.Tasks;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXPool 是 XPool 的单元测试。
/// </summary>
public class TestXPool
{
    [Test]
    public void Object()
    {
        // 测试基本的Get/Put功能
        var obj1 = XPool.Object<List<int>>.Get();
        Assert.That(obj1, Is.Not.Null, "从对象池获取的实例不应为空");
        obj1.Add(1);
        XPool.Object<List<int>>.Put(obj1);

        // 测试对象复用
        var obj2 = XPool.Object<List<int>>.Get();
        Assert.That(obj2, Is.SameAs(obj1), "对象池应返回之前缓存的同一个实例");
        Assert.That(obj2, Has.Count.EqualTo(1), "复用的对象应保持原有状态");

        // 测试池子上限
        var objects = new List<List<int>>();
        for (int i = 0; i < XPool.Object<List<int>>.PoolMax + 10; i++)
        {
            objects.Add(XPool.Object<List<int>>.Get());
        }
        objects.ForEach(XPool.Object<List<int>>.Put);
        Assert.That(XPool.Object<List<int>>.pools, Has.Count.LessThanOrEqualTo(XPool.Object<List<int>>.PoolMax), "对象池数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 1000;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var obj = XPool.Object<List<int>>.Get();
                    Assert.That(obj, Is.Not.Null, "多线程环境下从对象池获取的实例不应为空");
                    XPool.Object<List<int>>.Put(obj);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }

    [Test]
    public void StreamBuffer()
    {
        // 测试Get创建新对象
        var buffer1 = XPool.StreamBuffer.Get(1024);
        Assert.That(buffer1, Is.Not.Null, "从缓冲池获取的字节流不应为空");
        Assert.That(buffer1.Capacity, Is.EqualTo(1024), "字节流容量应与请求的大小一致");
        Assert.That(buffer1.Length, Is.EqualTo(0), "新创建的字节流长度应为0");
        Assert.That(buffer1.Position, Is.EqualTo(0), "新创建的字节流位置应为0");

        // 测试写入和Flush
        var testData = new byte[] { 1, 2, 3, 4 };
        buffer1.Writer.Write(testData);
        Assert.That(buffer1.Position, Is.EqualTo(4), "写入数据后流位置应等于写入的数据长度");
        buffer1.Flush();
        Assert.That(buffer1.Length, Is.EqualTo(4), "Flush后流长度应等于最后写入位置");
        Assert.That(buffer1.Position, Is.EqualTo(0), "Flush后流位置应重置为0");

        // 测试Put和对象池
        var originalBuffer = buffer1.Buffer;
        XPool.StreamBuffer.Put(buffer1);
        var buffer2 = XPool.StreamBuffer.Get(1024);
        Assert.That(buffer2, Is.SameAs(buffer1), "缓冲池应返回之前缓存的同一个字节流实例");
        Assert.That(buffer2.Length, Is.EqualTo(-1), "复用的字节流长度应被重置为-1");
        Assert.That(buffer2.Position, Is.EqualTo(0), "复用的字节流位置应被重置为0");

        // 测试获取更大容量的buffer
        var buffer3 = XPool.StreamBuffer.Get(2048);
        Assert.That(buffer3, Is.Not.SameAs(buffer1), "请求更大容量时应创建新的字节流实例");
        Assert.That(buffer3.Capacity, Is.EqualTo(2048), "新字节流容量应与请求的大小一致");

        // 测试ByteMax限制
        var largeBuffer = XPool.StreamBuffer.Get(XPool.StreamBuffer.ByteMax + 1);
        var largeBufferArray = largeBuffer.Buffer;
        XPool.StreamBuffer.Put(largeBuffer);
        var newLargeBuffer = XPool.StreamBuffer.Get(XPool.StreamBuffer.ByteMax + 1);
        Assert.That(newLargeBuffer, Is.Not.SameAs(largeBuffer), "超过最大字节限制的缓冲不应被复用");

        // 测试PoolMax限制
        var buffers = new List<XPool.StreamBuffer>();
        for (int i = 0; i < XPool.StreamBuffer.PoolMax + 10; i++)
        {
            buffers.Add(XPool.StreamBuffer.Get(1024));
        }
        buffers.ForEach(XPool.StreamBuffer.Put);
        Assert.That(XPool.StreamBuffer.Buffers, Has.Count.LessThanOrEqualTo(XPool.StreamBuffer.PoolMax), "缓冲池中的实例数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 100;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var buffer = XPool.StreamBuffer.Get(1024);
                    buffer.Writer.Write(j);
                    buffer.Flush();
                    XPool.StreamBuffer.Put(buffer);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
}
