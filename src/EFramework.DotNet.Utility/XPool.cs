// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XPool 提供了一个对象缓存工具集，实现了基础对象和字节流的实例的缓存和复用。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 基础对象缓存：提供线程安全的泛型对象池，支持自动创建和复用对象
    /// - 字节流缓存：提供高性能的字节缓冲池，支持自动扩容和复用
    /// - 线程安全设计：所有缓存操作都是线程安全的，支持多线程并发访问
    /// 
    /// 使用手册
    /// 
    /// 1. 基础对象
    /// 
    /// 1.1 泛型
    ///     // 获取对象
    ///     var obj = XPool.Object&lt;List&lt;int&gt;&gt;.Get();
    ///     obj.Add(1);
    /// 
    ///     // 回收对象
    ///     XPool.Object&lt;List&lt;int&gt;&gt;.Put(obj);
    /// 
    ///     // 对象会被自动复用
    ///     var obj2 = XPool.Object&lt;List&lt;int&gt;&gt;.Get();
    ///     Assert.That(obj2, Is.SameAs(obj));  // true
    /// 
    /// 1.2 非泛型
    ///     // 使用类型创建对象池
    ///     var pool = new XPool.Object(typeof(List&lt;int&gt;));
    /// 
    ///     // 使用委托创建对象池
    ///     var pool2 = new XPool.Object(() =&lt; new List&lt;int&gt;());
    /// 
    ///     // 获取和回收对象
    ///     var obj = pool.Get();
    ///     pool.Put(obj);
    /// 
    /// 2. 字节流
    /// 
    /// 2.1 获取
    ///     // 创建指定大小的缓冲区
    ///     var buffer = XPool.StreamBuffer.Get(1024);
    /// 
    ///     // 写入数据
    ///     buffer.Writer.Write(new byte[] { 1, 2, 3, 4 });
    ///     buffer.Flush();  // 更新长度并重置位置
    /// 
    ///     // 读取数据
    ///     var data = buffer.ToArray();
    /// 
    ///     注意：Get() 方法会优先查找大于等于请求大小的缓存对象
    /// 
    /// 2.2 拷贝
    ///     // 创建目标数组
    ///     var data = new byte[1024];
    /// 
    ///     // 复制数据
    ///     buffer.CopyTo(srcOffset: 0, data, dstOffset: 0, count: 1024);
    /// 
    ///     注意：
    ///     - Length 表示有效数据长度，而不是底层数组容量
    ///     - 写入数据后必须调用 Flush() 更新 Length
    ///     - Reset() 会将 Length 重置为 -1
    ///     - 使用 ToArray() 时以 Length 为准截取数据
    /// 
    /// 2.3 回收
    ///     // 回收到缓冲池
    ///     XPool.StreamBuffer.Put(buffer);
    /// 
    ///     // 释放资源
    ///     buffer.Dispose();
    /// 
    ///     注意：
    ///     - Put() 方法仅缓存小于 60KB 的对象
    ///     - 当池满时（500个），会释放最早缓存的对象
    ///     - 使用完毕后应调用 Put() 而不是 Dispose()
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XPool
    {
        /// <summary>
        /// Object 是基础对象（System.Object）的缓存池，提供线程安全的泛型对象池实现。
        /// </summary>
        /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
        public class Object<T> where T : class
        {
            /// <summary>
            /// PoolMax 是对象池的最大容量。
            /// </summary>
            internal const int PoolMax = 500;

            /// <summary>
            /// pools 是对象池的队列。
            /// </summary>
            internal static readonly Queue<T> pools = new();

            /// <summary>
            /// Get 从对象池获取对象实例。
            /// 如果池中有可用对象则返回缓存的对象，否则创建新对象。
            /// </summary>
            /// <returns>对象实例</returns>
            public static T Get()
            {
                T ret = null;
                if (pools.Count > 0)
                {
                    lock (pools)
                    {
                        try { ret = pools.Dequeue(); }
                        catch (Exception e)
                        {
                            XLog.Warn($"XPool.Object({typeof(T).FullName}): pools dequeue error: {e.Message}");
                        }
                    }
                }
                ret ??= Activator.CreateInstance<T>();
                return ret;
            }

            /// <summary>
            /// Put 回收对象实例到对象池。
            /// 如果池未满则缓存对象，否则丢弃。
            /// </summary>
            /// <param name="obj">要回收的对象实例</param>
            public static void Put(T obj)
            {
                if (obj == null) return;
                if (pools.Count < PoolMax)
                {
                    lock (pools) pools.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// Object 是基础对象（System.Object）的缓存池，提供线程安全的非泛型对象池实现。
        /// </summary>
        public class Object
        {
            /// <summary>
            /// PoolMax 是对象池最大容量。
            /// </summary>
            internal const int PoolMax = 500;

            /// <summary>
            /// pools 是对象池的队列。
            /// </summary>
            internal readonly Queue pools = new();

            /// <summary>
            /// type 是对象的类型。
            /// </summary>
            internal readonly Type type;

            /// <summary>
            /// activator 是对象的创建器。
            /// </summary>
            internal readonly Func<object> activator;

            /// <summary>
            /// 使用类型构造对象池 Object 实例。
            /// </summary>
            /// <param name="type">对象类型</param>
            public Object(Type type) { this.type = type; }

            /// <summary>
            /// 使用创建器构造对象池 Object 实例。
            /// </summary>
            /// <param name="activator">对象创建器</param>
            public Object(Func<object> activator) { this.activator = activator; }

            /// <summary>
            /// Get 从对象池获取对象实例。
            /// 如果池中有可用对象则返回缓存的对象，否则使用类型或创建器创建新对象。
            /// </summary>
            /// <returns>对象实例</returns>
            public object Get()
            {
                object ret = null;
                if (pools.Count > 0)
                {
                    lock (pools)
                    {
                        try { ret = pools.Dequeue(); }
                        catch (Exception e)
                        {
                            var str = type != null ? type.FullName : activator != null ? activator.Method.DeclaringType.Name : "null";
                            XLog.Warn($"XPool.Object({str}): pools dequeue error: {e.Message}");
                        }
                    }
                }
                ret ??= activator != null ? activator.Invoke() : Activator.CreateInstance(type);
                return ret;
            }

            /// <summary>
            /// Put 回收对象实例到对象池。
            /// 如果池未满则缓存对象，否则丢弃。
            /// </summary>
            /// <param name="obj">要回收的对象实例</param>
            public void Put(object obj)
            {
                if (obj == null) return;
                if (pools.Count < PoolMax)
                {
                    lock (pools) pools.Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// StreamBuffer 是字节流的缓存池，提供高性能的字节数组缓冲和复用。
        /// </summary>
        public sealed class StreamBuffer : IDisposable
        {
            /// <summary>
            /// buffer 是字节数组。
            /// </summary>
            internal byte[] buffer;

            /// <summary>
            /// stream 是内存流。
            /// </summary>
            internal MemoryStream stream;

            /// <summary>
            /// reader 是二进制读取器。
            /// </summary>
            internal BinaryReader reader;

            /// <summary>
            /// writer 是二进制写入器。
            /// </summary>
            internal BinaryWriter writer;

            /// <summary>
            /// Capacity 获取字节容量。
            /// </summary>
            public int Capacity { get => buffer.Length; }

            /// <summary>
            /// Length 获取或设置有效字节长度。
            /// </summary>
            public int Length { get; internal set; }

            /// <summary>
            /// Position 获取或设置当前读写位置。
            /// </summary>
            public int Position { get => (int)Stream.Position; set => Stream.Position = value; }

            /// <summary>
            /// Stream 获取内存流实例。
            /// </summary>
            public MemoryStream Stream
            {
                get
                {
                    stream ??= new MemoryStream(buffer, 0, buffer.Length, true, true);
                    return stream;
                }
            }

            /// <summary>
            /// Reader 获取二进制读取器。
            /// </summary>
            public BinaryReader Reader
            {
                get
                {
                    reader ??= new BinaryReader(Stream);
                    return reader;
                }
            }

            /// <summary>
            /// Writer 获取二进制写入器。
            /// </summary>
            public BinaryWriter Writer
            {
                get
                {
                    writer ??= new BinaryWriter(Stream);
                    return writer;
                }
            }

            /// <summary>
            /// Buffer 获取底层字节数组。
            /// </summary>
            public byte[] Buffer { get => buffer; }

            /// <summary>
            /// 使用现有字节数组构造缓冲区 StreamBuffer 实例。
            /// </summary>
            /// <param name="buffer">字节数组</param>
            /// <param name="offset">起始偏移</param>
            public StreamBuffer(byte[] buffer, int offset = 0)
            {
                this.buffer = buffer;
                Length = buffer.Length;
                Position = offset;
            }

            /// <summary>
            /// 构造指定大小的缓冲区 StreamBuffer 实例。
            /// </summary>
            /// <param name="size">缓冲区大小</param>
            public StreamBuffer(int size)
            {
                if (size < 0) throw new Exception("size must >= 0");
                buffer = new byte[size];
            }

            /// <summary>
            /// ToArray 将缓冲区数据转换为字节数组。
            /// </summary>
            /// <param name="offset">起始偏移</param>
            /// <param name="count">复制长度</param>
            /// <returns>字节数组</returns>
            public byte[] ToArray(int offset = 0, int count = 0)
            {
                if (count == 0) count = Length;
                byte[] bytes = new byte[count - offset];
                CopyTo(offset, bytes, 0, bytes.Length);
                return bytes;
            }

            /// <summary>
            /// CopyTo 将缓冲区数据复制到目标数组。
            /// </summary>
            /// <param name="srcOffset">源偏移</param>
            /// <param name="dst">目标数组</param>
            /// <param name="dstOffset">目标偏移</param>
            /// <param name="count">复制长度</param>
            public void CopyTo(int srcOffset, Array dst, int dstOffset, int count) { System.Buffer.BlockCopy(buffer, srcOffset, dst, dstOffset, count); }

            /// <summary>
            /// Flush 完成写入操作，更新数据长度并重置位置。
            /// </summary>
            public void Flush() { Length = Position; Stream.Seek(0, SeekOrigin.Begin); }

            /// <summary>
            /// Reset 重置缓冲区的状态。
            /// </summary>
            public void Reset() { Length = -1; Stream.Seek(0, SeekOrigin.Begin); }

            /// <summary>
            /// Dispose 释放缓冲区的资源。
            /// </summary>
            public void Dispose()
            {
                Length = -1;
                buffer = null;
                try { reader?.Close(); } catch { }
                try { writer?.Close(); } catch { }
                try { stream?.Close(); } catch { }
                try { stream?.Dispose(); } catch { }
                reader = null;
                writer = null;
                stream = null;
            }

            /// <summary>
            /// PoolMax 是缓冲池的最大容量。
            /// </summary>
            public static int PoolMax = 500;

            /// <summary>
            /// ByteMax 是单个缓冲区的最大字节数。
            /// </summary>
            public static int ByteMax = 60 * 1024;

            /// <summary>
            /// Buffers 是缓冲池的列表。
            /// </summary>
            internal static List<StreamBuffer> Buffers = new();

            /// <summary>
            /// BuffersHash 是缓冲池的哈希表。
            /// </summary>
            internal static Dictionary<int, byte> BuffersHash = new();

            /// <summary>
            /// Get 获取指定大小的缓冲区。
            /// 如果池中有合适大小的缓冲区则返回缓存的实例，否则创建新实例。
            /// </summary>
            /// <param name="expected">预期大小</param>
            /// <returns>缓冲区实例</returns>
            public static StreamBuffer Get(int expected)
            {
                if (expected < 0) throw new Exception("expected size must >= 0");
                StreamBuffer buffer = null;
                if (expected < ByteMax)
                {
                    lock (Buffers)
                    {
                        for (int i = Buffers.Count - 1; i >= 0; i--)
                        {
                            var tmp = Buffers[i];
                            if (tmp.Capacity >= expected)
                            {
                                buffer = tmp;
                                buffer.Reset();
                                Buffers.RemoveAt(i);
                                BuffersHash.Remove(buffer.GetHashCode());
                                break;
                            }
                        }
                    }
                }
                buffer ??= new StreamBuffer(expected);
                return buffer;
            }

            /// <summary>
            /// Put 回收缓冲区到缓冲池。
            /// 如果缓冲区大小超过限制或池已满则不缓存。
            /// </summary>
            /// <param name="buffer">缓冲区实例</param>
            public static void Put(StreamBuffer buffer)
            {
                if (buffer == null || buffer.Length == 0) return;
                if (buffer.Length > ByteMax) return;
                lock (Buffers)
                {
                    buffer.Reset();
                    if (!BuffersHash.ContainsKey(buffer.GetHashCode()))
                    {
                        if (Buffers.Count >= PoolMax)
                        {
                            var tmp = Buffers[0];
                            tmp.Dispose();
                            Buffers.RemoveAt(0);
                            BuffersHash.Remove(tmp.GetHashCode());
                        }
                        Buffers.Add(buffer);
                        BuffersHash.Add(buffer.GetHashCode(), 0);
                    }
                }
            }
        }
    }
}
