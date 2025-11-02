# XTime

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了一组时间常量定义及工具函数，支持时间戳转换和格式化等功能。

## 功能特性

- 提供秒、分钟、小时、天的常量定义
- 提供时间戳（秒/毫秒）的获取方法
- 提供时间格式化和转换功能
- 提供零点时间相关的计算功能

## 使用手册

### 1. 时间常量

时间常量提供了从秒到天的时间单位定义，所有常量均以秒为基本单位。

```csharp
// 基本单位示例
var oneSecond = XTime.Second1;        // 1秒
var oneMinute = XTime.Minute1;        // 60秒
var oneHour = XTime.Hour1;            // 3600秒
var oneDay = XTime.Day1;              // 86400秒
```

### 2. 时间戳操作

提供秒级和毫秒级时间戳的获取方法，基于 1970-01-01 计算。

```csharp
// 获取当前时间戳
var timestamp = XTime.GetTimestamp();  // 获取秒级时间戳
var millis = XTime.GetMillisecond();  // 获取毫秒级时间戳
```

### 3. 时间转换

支持时间戳与 DateTime 之间的相互转换。

```csharp
// 时间戳转换示例
var dateTime = XTime.ToTime(1234567890);    // 时间戳转DateTime
var now = XTime.NowTime();                  // 获取当前DateTime
```

### 4. 零点时间计算

提供基于本地时区的零点时间计算功能。

```csharp
// 零点计算示例
var secondsToZero = XTime.TimeToZero();     // 距离下个零点的秒数
var zeroTimestamp = XTime.ZeroTime();       // 获取当天零点时间戳
```

### 5. 时间格式化

支持多种格式的时间格式化，可自定义格式字符串。

```csharp
// 格式化示例
XTime.Format(timestamp);                    // "2024-03-21 14:30:00"
XTime.Format(millis);                       // "2024-03-21 14:30:00.123"
XTime.Format(DateTime.Now);                 // "2024-03-21 14:30:00.123"
XTime.Format(timestamp, "yyyy-MM-dd");      // "2024-03-21"
```

## 常见问题

### 1. 时区问题
XTime 的时间戳计算会自动考虑本地时区，初始时间（1970-01-01）会根据 `TimeZoneInfo.Local.BaseUtcOffset` 进行调整。

### 2. 格式化精度
- 秒级时间戳格式化默认精确到秒（HH:mm:ss）
- 毫秒级时间戳和 DateTime 格式化默认精确到毫秒（HH:mm:ss.fff）

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
