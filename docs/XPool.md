# XPool

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了一个对象缓存工具集，实现了基础对象和字节流的实例的缓存和复用。

## 功能特性

- 基础对象缓存：提供线程安全的泛型对象池，支持自动创建和复用对象
- 字节流缓存：提供高性能的字节缓冲池，支持自动扩容和复用
- 线程安全设计：所有缓存操作都是线程安全的，支持多线程并发访问

## 使用手册

### 1. 基础对象

#### 1.1 泛型
```csharp
// 获取对象
var obj = XPool.Object<List<int>>.Get();
obj.Add(1);

// 回收对象
XPool.Object<List<int>>.Put(obj);

// 对象会被自动复用
var obj2 = XPool.Object<List<int>>.Get();
Assert.That(obj2, Is.SameAs(obj));  // true
```

#### 1.2 非泛型
```csharp
// 使用类型创建对象池
var pool = new XPool.Object(typeof(List<int>));

// 使用委托创建对象池
var pool2 = new XPool.Object(() => new List<int>());

// 获取和回收对象
var obj = pool.Get();
pool.Put(obj);
```

### 2. 字节流

#### 2.1 获取
```csharp
// 创建指定大小的缓冲区
var buffer = XPool.StreamBuffer.Get(1024);

// 写入数据
buffer.Writer.Write(new byte[] { 1, 2, 3, 4 });
buffer.Flush();  // 更新长度并重置位置

// 读取数据
var data = buffer.ToArray();
```
注意：Get() 方法会优先查找大于等于请求大小的缓存对象

#### 2.2 拷贝
```csharp
// 创建目标数组
var data = new byte[1024];

// 复制数据
buffer.CopyTo(srcOffset: 0, data, dstOffset: 0, count: 1024);
```
注意：
- Length 表示有效数据长度，而不是底层数组容量
- 写入数据后必须调用 Flush() 更新 Length
- Reset() 会将 Length 重置为 -1
- 使用 ToArray() 时以 Length 为准截取数据

#### 2.3 回收
```csharp
// 回收到缓冲池
XPool.StreamBuffer.Put(buffer);

// 释放资源
buffer.Dispose();
```
注意：
- Put() 方法仅缓存小于 60KB 的对象
- 当池满时（500个），会释放最早缓存的对象
- 使用完毕后应调用 Put() 而不是 Dispose()

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
