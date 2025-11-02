# XLog

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了一个遵循 RFC5424 标准的日志系统，支持多适配器管理、日志轮转和结构化标签等特性。

## 功能特性

- 内置标准输出和文件存储适配器
- 支持日志文件的自动轮转和清理
- 支持异步写入和线程安全操作
- 支持结构化的日志标签系统

## 使用手册

### 1. 日志输出

```csharp
// 不同级别的日志记录（按严重程度从高到低排序）
XLog.Emergency("系统崩溃");
XLog.Alert("立即处理");
XLog.Critical("严重错误");
XLog.Error("操作失败");
XLog.Warn("潜在问题");
XLog.Notice("重要信息");
XLog.Info("一般信息");
XLog.Debug("调试信息");

// 检查日志级别
var currentLevel = XLog.Level();
var logable = XLog.Able(XLog.LevelType.Debug);
```

### 2. 多适配器

#### 2.1 标准输出

标准输出适配器支持以下配置项：

```csharp
var config = new XPrefs.IBase();
config.Set("Level", "Info");     // 日志级别
config.Set("Color", true);       // 着色输出
preferences.Set("XLog/Std", config);

XLog.Initialize(preferences);
```

日志内容着色：
  - Emergency: 黑色背景
  - Alert: 青色
  - Critical: 品红色
  - Error: 红色
  - Warn: 黄色
  - Notice: 绿色
  - Info: 灰色
  - Debug: 蓝色

#### 2.2 文件存储

文件存储适配器支持以下配置项：

```csharp
var preferences = new XPrefs.IBase();
var config = new XPrefs.IBase();

// 基础配置
config.Set("Path", "${Environment.LocalPath}/Log/app.log");  // 日志文件
config.Set("Level", "Debug");                                // 日志级别

// 轮转配置
config.Set("Rotate", true);        // 是否启用日志轮转
config.Set("Daily", true);         // 是否按天轮转
config.Set("MaxDay", 7);           // 日志文件保留天数
config.Set("Hourly", false);       // 是否按小时轮转
config.Set("MaxHour", 168);        // 日志文件保留小时数

// 文件限制
config.Set("MaxFile", 100);        // 最大文件数量
config.Set("MaxLine", 1000000);    // 单文件最大行数
config.Set("MaxSize", 134217728);  // 单文件最大体积（128MB）

preferences.Set("XLog/File", config);
XLog.Initialize(preferences);
```

文件轮转策略：
  - 按天轮转：每天创建新文件，自动清理超过 MaxDay 天数的文件
  - 按小时轮转：每小时创建新文件，自动清理超过 MaxHour 小时数的文件
  - 按大小轮转：当文件超过 MaxSize 时创建新文件
  - 按行数轮转：当文件超过 MaxLine 时创建新文件
  - 文件数量限制：通过 MaxFile 控制最大文件数

文件命名规则，以 "./logs/app.log" 为例：
  - 按天轮转：
    - 当前文件：app.log
    - 历史文件：app.2024-03-21.001.log, app.2024-03-21.002.log, ...
  - 按小时轮转：
    - 当前文件：app.log
    - 历史文件：app.2024-03-21-15.001.log, app.2024-03-21-15.002.log, ...
  - 按大小/行数轮转：
    - 当前文件：app.log
    - 历史文件：app.001.log, app.002.log, ...

### 3. 日志标签

#### 3.1 基本用法
```csharp
// 创建和使用标签
var tag = XLog.GetTag()
    .Set("module", "network")
    .Set("action", "connect")
    .Set("userId", "12345");

// 使用标签记录日志
XLog.Info(tag, "用户连接成功");

// 使用完后回收标签
XLog.PutTag(tag);
```

#### 3.2 上下文
```csharp
// 设置当前线程的标签
var tag = XLog.GetTag()
    .Set("threadId", "main")
    .Set("session", "abc123");
XLog.Watch(tag);

// 使用当前线程的标签
XLog.Info("处理请求");  // 自动附加上下文标签

// 清理上下文标签
XLog.Defer();
```

#### 3.3 格式化
```csharp
var tag = XLog.GetTag()
    .Set("module", "auth")
    .Set("userId", "12345")
    .Set("ip", "192.168.1.1");

// 输出格式：[时间] [级别] [module=auth, userId=12345, ip=192.168.1.1] 消息内容
XLog.Info(tag, "用户登录成功");
```

## 常见问题

### 1. 日志文件没有轮转？
- Rotate 是否设置为 true
- Daily/Hourly 是否正确设置
- MaxDay/MaxHour 是否合理设置
- MaxLine/MaxSize 是否达到触发条件

### 2. 日志文件数量过多？
- 合理设置日志级别，避免过多调试日志
- 使用异步写入模式
- 适当调整文件轮转参数
- 及时清理过期日志文件

### 3. 日志内存占用过高？
- 及时回收日志标签（使用 PutTag）
- 清理不再使用的上下文标签（使用 Defer）
- 合理设置缓冲区大小
- 定期执行日志清理

### 4. 日志内容丢失问题？
- 确保正确调用 Flush 和 Close
- 检查文件系统权限
- 验证磁盘空间是否充足
- 检查文件路径是否正确

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
