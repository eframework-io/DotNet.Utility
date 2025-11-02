# EFramework Utility for DotNet

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了基于 .NET 的配置管理、任务调度、日志记录、事件系统、文件处理等基础功能模块。

## 功能特性

- [XApp](docs/XApp.md) 提供了应用程序的状态管理，用于控制应用程序的启动、运行和退出流程
- [XEnv](docs/XEnv.md) 提供了应用程序的环境管理，支持多平台识别、路径管理、命令行参数解析和环境变量求值等功能
- [XEvent](docs/XEvent.md) 提供了轻量级的事件管理器，支持多重监听、单次及泛型回调和批量通知等功能
- [XFile](docs/XFile.md) 简化了对文件和目录的基本操作，支持路径归一化、解压缩文件、文件校验等功能
- [XLog](docs/XLog.md) 提供了一个遵循 RFC5424 标准的日志系统，支持多适配器管理、日志轮转和结构化标签等特性
- [XLoom](docs/XLoom.md) 提供了一个轻量级的任务调度系统，用于管理异步任务、定时器和多线程并发
- [XObject](docs/XObject.md) 提供了一个对象序列化工具集，实现了结构体与字节数组的转换、对象的 JSON 序列化等功能
- [XPool](docs/XPool.md) 提供了一个对象缓存工具集，实现了基础对象和字节流的实例的缓存和复用
- [XPrefs](docs/XPrefs.md) 实现了多源化配置的读写，支持变量求值和命令行参数覆盖等功能，是一个灵活高效的首选项系统
- [XString](docs/XString.md) 实现了文本处理、数值转换、加密解密和变量求值等功能，是一个高效的字符串工具类
- [XTime](docs/XTime.md) 提供了一组时间常量定义及工具函数，支持时间戳转换和格式化等功能

## 常见问题

更多问题，请查阅[问题反馈](CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](CHANGELOG.md)
- [贡献指南](CONTRIBUTING.md)
- [许可协议](LICENSE)
