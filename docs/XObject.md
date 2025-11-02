# XObject

[![NuGet](https://img.shields.io/nuget/v/EFramework.DotNet.Utility.svg?label=NuGet)](https://www.nuget.org/packages/EFramework.DotNet.Utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/DotNet.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

提供了一个对象序列化工具集，实现了结构体与字节数组的转换、对象的 JSON 序列化等功能。

## 功能特性

- 支持结构体序列化：提供结构体与字节数组的双向转换，基于 Marshal 实现
- 支持 JSON 序列化：支持对象与 JSON 的双向转换，包括基础类型、数组、列表和字典
- 提供编解码接口：通过 IEncoder 和 IDecoder 接口支持自定义序列化逻辑
- 灵活序列化控制：使用特性标记可序列化的字段和属性，支持公有和私有成员

## 使用手册

### 1. 结构体序列化

#### 1.1 结构体转字节数组
```csharp
// 定义测试结构体
struct TestStruct
{
    public int IntTest;
    public bool BoolTest;
}

// 创建结构体实例
var testObj = new TestStruct { IntTest = 1, BoolTest = true };

// 序列化为字节数组
var bytes = XObject.ToByte(testObj);
```

#### 1.2 字节数组转结构体
```csharp
// 反序列化为结构体
var deserializedObj = XObject.FromByte<TestStruct>(bytes);

// 验证字段值
Console.WriteLine(deserializedObj.IntTest);    // 输出: 1
Console.WriteLine(deserializedObj.BoolTest);   // 输出: True
```

### 2. JSON 序列化

#### 2.1 对象转 JSON
```csharp
// 定义测试类
class TestClass
{
    public int Id;
    public string Name;
}

// 创建对象实例
var testObj = new TestClass { Id = 1, Name = "Test" };

// 转换为格式化的 JSON
var jsonPretty = XObject.ToJson(testObj, true);
// 输出: {
//     "Id": 1,
//     "Name": "Test"
// }

// 转换为压缩的 JSON
var jsonCompact = XObject.ToJson(testObj, false);
// 输出: {"Id":1,"Name":"Test"}
```

#### 2.2 JSON 转对象
```csharp
// JSON 字符串
var json = "{\"Id\":1,\"Name\":\"Test\"}";

// 从字符串解析
var resultFromString = XObject.FromJson<TestClass>(json);

// 从 JSONNode 解析
var resultFromNode = XObject.FromJson<TestClass>(JSON.Parse(json));
```

### 3. 自定义序列化

#### 3.1 使用编码器接口
```csharp
class CustomClass : XObject.Json.IEncoder
{
    public int Value { get; set; }

    public JSONNode Encode()
    {
        var node = new JSONObject();
        node.Add("value", Value);
        return node;
    }
}
```

#### 3.2 使用解码器接口
```csharp
class CustomClass : XObject.Json.IDecoder
{
    public int Value { get; set; }

    public void Decode(JSONNode json)
    {
        Value = json["value"].AsInt;
    }
}
```

#### 3.3 使用序列化特性
```csharp
class CustomClass
{
    // 排除特定字段
    [XObject.Json.Exclude]
    public int ExcludedField;

    // 包含私有字段
    [XObject.Json.Include]
    private string includedField;

    // 排除特定属性
    [XObject.Json.Exclude]
    public string ExcludedProperty { get; set; }
}
```

## 常见问题

### 1. 序列化结构体时出现异常？
- 确保结构体是值类型（struct）
- 检查结构体字段是否都是可序列化的基础类型
- 验证结构体的内存布局是否对齐

### 2. JSON 转换出现空值？
- 检查 JSON 字符串中的字段名是否与对象属性完全匹配
- 确认私有字段是否标记了 [XObject.Json.Include] 特性
- 验证字段是否被 [XObject.Json.Exclude] 特性排除

### 3. 自定义类型序列化失败？
- 实现 IEncoder/IDecoder 接口以自定义序列化逻辑
- 使用特性正确标记需要序列化的字段
- 确保所有嵌套类型都支持序列化

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE)
