// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Runtime.InteropServices;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXObject 是 XObject 的单元测试。
/// </summary>
public class TestXObject
{
    private struct TestStruct
    {
        public int IntTest;
        public bool BoolTest;
    }

    private class TestClass
    {
        public int Id;
        public string Name;
    }

    [Test]
    public void ToByte()
    {
        // Arrange
        var testObj = new TestStruct { IntTest = 1, BoolTest = true };

        // Act
        var bytes = XObject.ToByte(testObj);

        // Assert
        Assert.That(bytes, Is.Not.Null, "结构体序列化后的字节数组不应为空");
        Assert.That(bytes.Length, Is.EqualTo(Marshal.SizeOf(typeof(TestStruct))), "序列化后的字节数组长度应与结构体大小相同");
    }

    [Test]
    public void FromByte()
    {
        // Arrange
        var testObj = new TestStruct { IntTest = 1, BoolTest = false };

        // Act
        var bytes = XObject.ToByte(testObj);
        var deserializedObj = XObject.FromByte<TestStruct>(bytes);

        // Assert
        Assert.That(deserializedObj.IntTest, Is.EqualTo(testObj.IntTest), "反序列化后的整数字段值应与原始值相同");
        Assert.That(deserializedObj.BoolTest, Is.EqualTo(testObj.BoolTest), "反序列化后的布尔字段值应与原始值相同");
    }

    [Test]
    public void FromJson()
    {
        // Arrange
        string json = "{\"Id\":1,\"Name\":\"Test\"}";

        // Act
        var resultFromString = XObject.FromJson<TestClass>(json);
        var resultFromNode = XObject.FromJson<TestClass>(JSON.Parse(json));

        // Assert
        Assert.That(resultFromString, Is.Not.Null, "从字符串解析的对象不应为空");
        Assert.That(resultFromString.Id, Is.EqualTo(1), "从字符串解析的对象 Id 应为 1");
        Assert.That(resultFromString.Name, Is.EqualTo("Test"), "从字符串解析的对象 Name 应为 'Test'");

        Assert.That(resultFromNode, Is.Not.Null, "从 JSONNode 解析的对象不应为空");
        Assert.That(resultFromNode.Id, Is.EqualTo(1), "从 JSONNode 解析的对象 Id 应为 1");
        Assert.That(resultFromNode.Name, Is.EqualTo("Test"), "从 JSONNode 解析的对象 Name 应为 'Test'");
    }

    [Test]
    public void ToJson()
    {
        // Arrange
        var testObj = new TestClass { Id = 1, Name = "Test" };

        // Act
        var jsonPretty = XObject.ToJson(testObj, true);
        var jsonCompact = XObject.ToJson(testObj, false);

        // Assert
        Assert.That(jsonPretty, Is.Not.Null, "格式化的 JSON 字符串不应为空");
        Assert.That(jsonPretty.Contains("\"Id\": 1"), Is.True, "格式化的 JSON 应包含格式化后的 Id 字段");
        Assert.That(jsonPretty.Contains("\"Name\": \"Test\""), Is.True, "格式化的 JSON 应包含格式化后的 Name 字段");

        Assert.That(jsonCompact, Is.Not.Null, "压缩的 JSON 字符串不应为空");
        Assert.That(jsonCompact.Contains("\"Id\":1"), Is.True, "压缩的 JSON 应包含未格式化的 Id 字段");
        Assert.That(jsonCompact.Contains("\"Name\":\"Test\""), Is.True, "压缩的 JSON 应包含未格式化的 Name 字段");
    }
}
