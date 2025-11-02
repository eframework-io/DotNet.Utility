# XPrefs

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

实现了多源化配置的读写，支持变量求值和命令行参数覆盖等功能，是一个灵活高效的首选项系统。

## 功能特性

- 多源化配置：支持内置配置（只读）、本地配置（可写）和远端配置（只读），支持多个配置源按优先级顺序读取
- 多数据类型：支持基础类型（整数、浮点数、布尔值、字符串）、数组类型及配置实例（IBase）
- 变量求值：支持通过命令行参数动态覆盖配置项，使用 ${Preferences.Key} 语法引用其他配置项

## 使用手册

### 1. 基础操作

#### 1.1 检查配置项
```csharp
// 检查配置项是否存在
var exists = XPrefs.HasKey("configKey");
```

#### 1.2 读写基本类型
```csharp
// 写入配置
XPrefs.Local.Set("intKey", 42);
XPrefs.Local.Set("floatKey", 3.14f);
XPrefs.Local.Set("boolKey", true);
XPrefs.Local.Set("stringKey", "value");

// 读取配置
var intValue = XPrefs.GetInt("intKey", 0);
var floatValue = XPrefs.GetFloat("floatKey", 0f);
var boolValue = XPrefs.GetBool("boolKey", false);
var stringValue = XPrefs.GetString("stringKey", "");
```

#### 1.3 读写数组类型
```csharp
// 写入数组
XPrefs.Local.Set("intArray", new[] { 1, 2, 3 });
XPrefs.Local.Set("stringArray", new[] { "a", "b", "c" });

// 读取数组
var intArray = XPrefs.GetInts("intArray");
var stringArray = XPrefs.GetStrings("stringArray");
```

### 2. 配置源管理

#### 2.1 内置配置（只读）
```csharp
// 读取内置配置
var value = XPrefs.Asset.GetString("key");
```

#### 2.2 本地配置（可写）
```csharp
// 写入本地配置
XPrefs.Local.Set("key", "value");
XPrefs.Local.Save();

// 读取本地配置
var value = XPrefs.Local.GetString("key");
```

#### 2.3 远端配置（只读）
```csharp
// RemoteHandler 是远端配置处理器
public class RemoteHandler : XPrefs.IRemote.IHandler
{
    // Uri 是远端的地址。
    public string Uri => "http://example.com/config";

    // OnStarted 是流程启动的回调。
    public void OnStarted(XPrefs.IRemote context) { }
    
    // OnRequest 是预请求的回调。
    public void OnRequest(XPrefs.IRemote context, HttpRequestMessage request) { }

    // OnRetry 是错误重试的回调。
    public bool OnRetry(XPrefs.IRemote context, int count, out float pending)
    {
        pending = 1.0f;
        return count < 3;
    }

    // OnSucceeded 是请求成功的回调。
    public void OnSucceeded(XPrefs.IRemote context) { }

    // OnFailed 是请求失败的回调。
    public void OnFailed(XPrefs.IRemote context) { }
}

// 读取远端配置
await XPrefs.Remote.Read(new RemoteHandler());
```

### 3. 变量求值

#### 3.1 基本用法
```csharp
// 设置配置项
XPrefs.Local.Set("name", "John");
XPrefs.Local.Set("greeting", "Hello ${Preferences.name}");

// 解析变量引用
var result = XPrefs.Local.Eval("${Preferences.greeting}"); // 输出: Hello John
```

#### 3.2 多级路径
```csharp
// 设置嵌套配置
XPrefs.Local.Set("user.name", "John");
XPrefs.Local.Set("user.age", 30);

// 使用多级路径引用
var result = XPrefs.Local.Eval("${Preferences.user.name} is ${Preferences.user.age}");
```

### 4. 命令行参数

#### 4.1 覆盖配置路径
```bash
--Preferences@Asset=path/to/asset.json    # 覆盖内置配置路径
--Preferences@Local=path/to/local.json    # 覆盖本地配置路径
```

#### 4.2 覆盖配置值
```bash
--Preferences@Asset.key=value             # 覆盖内置配置项
--Preferences@Local.key=value             # 覆盖本地配置项
--Preferences.key=value                   # 覆盖所有配置源
```

## 常见问题

### 1. 配置无法保存？
- 检查配置对象是否可写（writable = true）。
- 确认文件路径有效且具有写入权限。
- 验证是否调用了 Save() 方法。

### 2. 变量替换失败？
- 确认变量引用格式正确（${Preferences.key}）。
- 检查引用的配置项是否存在。
- 注意避免循环引用和嵌套引用。

### 3. 远端配置失败？
- 检查网络连接是否正常。
- 确认远端服务器地址正确。
- 验证超时和重试参数设置。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
