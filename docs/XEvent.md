# XEvent

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了轻量级的事件管理器，支持多重监听、单次及泛型回调和批量通知等功能。

## 功能特性

- 多重监听：可配置是否允许同一事件注册多个回调
- 单次回调：可设置回调函数仅执行一次后自动注销
- 泛型回调：支持无参数、单参数和多参数的事件回调

## 使用手册

### 1. 事件管理器

#### 1.1 多重监听
```csharp
// 创建支持多重监听的事件管理器
var eventManager = new XEvent.Manager(true);

// 注册多个回调
eventManager.Register(1, (args) => Console.WriteLine("First"));
eventManager.Register(1, (args) => Console.WriteLine("Second"));
```

#### 1.2 单一监听
```csharp
// 创建单一监听的事件管理器
var singleManager = new XEvent.Manager(false);

// 注册回调，第二次注册会失败
singleManager.Register(1, (args) => Console.WriteLine("Only One"));
```

### 2. 事件注册

#### 2.1 普通事件
```csharp
// 注册无参数回调事件
eventManager.Register(1, () => Console.WriteLine("Event Triggered"));

// 注册带参数回调事件
eventManager.Register<string>(2, (msg) => Console.WriteLine(msg));
eventManager.Register<int, string>(3, (id, name) => Console.WriteLine($"{id}: {name}"));
```

#### 2.2 单次事件
```csharp
// 注册单次回调事件，执行后自动注销
eventManager.Register(1, (args) => Console.WriteLine("Once"), true);
```

### 3. 事件通知

#### 3.1 无参数
```csharp
// 通知事件，不传递参数
eventManager.Notify(1);
```

#### 3.2 带参数
```csharp
// 通知事件，传递参数
eventManager.Notify(2, "Hello World");
eventManager.Notify(3, 1, "User");
```

### 4. 事件注销

#### 4.1 指定事件
```csharp
// 注销特定事件的指定回调
void callback(object[] args) { }
eventManager.Register(1, callback);
eventManager.Unregister(1, callback);
```

#### 4.2 所有事件
```csharp
// 注销特定事件的所有回调
eventManager.Unregister(1);

// 清除所有事件的所有回调
eventManager.UnregisterAll();
```

## 常见问题

### 1. 为什么无法注册多个回调？
检查事件管理器是否以单一监听模式创建（multiple = false）。
单一监听模式下每个事件只能注册一个回调。

### 2. 多个回调的执行顺序是怎样的？
回调按照注册顺序依次执行。后注册的回调后执行。

### 3. 单次回调什么时候被注销？
单次回调在首次执行后自动注销，不需要手动注销。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
