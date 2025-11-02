// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXString 是 XString 的单元测试。
/// </summary>
public class TestXString
{
    [TestCase("Hello, {0}!", new object[] { "World" }, "Hello, World!", Description = "测试基本的单参数替换")]
    [TestCase("Hello, {0}! You have {1} new messages.", new object[] { "Alice", 5 }, "Hello, Alice! You have 5 new messages.", Description = "测试多参数替换")]
    [TestCase("Value: {0}, Again: {0}", new object[] { 42 }, "Value: 42, Again: 42", Description = "测试同一参数多次使用")]
    [TestCase("Last: {2}, First: {0}, Middle: {1}", new object[] { "A", "B", "C" }, "Last: C, First: A, Middle: B", Description = "测试参数乱序使用")]
    // 边缘情况测试
    [TestCase("", new object[] { }, "", Description = "测试空字符串")]
    [TestCase("Value: {0}", new object[] { null }, "Value: ", Description = "测试 null 参数")]
    [TestCase("No placeholders", new object[] { }, "No placeholders", Description = "测试无占位符字符串")]
    [TestCase("Missing: {1}", new object[] { "OnlyOne" }, "Missing: {1}", Description = "测试缺少参数")]
    [TestCase("Special: {0}", new object[] { "!@#$%^&*()" }, "Special: !@#$%^&*()", Description = "测试特殊字符")]
    // 格式说明符测试
    [TestCase("Number: {0:0.00}", new object[] { 42 }, "Number: 42.00", Description = "测试数字格式化")]
    [TestCase("Large: {0:N0}", new object[] { 1234567 }, "Large: 1,234,567", Description = "测试大数字格式化")]
    public void Format(string format, object[] args, string expected)
    {
        // Act
        var result = XString.Format(format, args);

        // Assert
        // if (expected == "Missing: {1}") LogAssert.Expect(LogType.Exception, new Regex("FormatException"));
        Assert.That(result, Is.EqualTo(expected), "字符串格式化结果与预期不符。");
    }

    [TestCase(1024, "1 KB", Description = "测试 1KB 转换")]
    [TestCase(1024 * 1024, "1 MB", Description = "测试 1MB 转换")]
    [TestCase(1024 * 1024 * 1024, "1 GB", Description = "测试 1GB 转换")]
    [TestCase(1500, "1.46 KB", Description = "测试非整数 KB 转换")]
    [TestCase(1024 * 1500, "1.46 MB", Description = "测试非整数 MB 转换")]
    public void Size(long bytes, string expected)
    {
        // Act
        var result = XString.ToSize(bytes);

        // Assert
        Assert.That(result, Is.EqualTo(expected), "字节大小转换结果与预期不符。");
    }

    [Test]
    public void Version()
    {
        // 测试ToVersion方法
        Assert.That(XString.ToVersion(""), Is.EqualTo(-1), "空字符串版本号应转换为 -1。");
        Assert.That(XString.ToVersion("0"), Is.EqualTo(0), "版本号 0 转换结果错误。");
        Assert.That(XString.ToVersion("1"), Is.EqualTo(1), "版本号 1 转换结果错误。");
        Assert.That(XString.ToVersion("1.1"), Is.EqualTo(10001), "两段式版本号转换结果错误。");
        Assert.That(XString.ToVersion("1.1.1"), Is.EqualTo(100010001), "三段式版本号转换结果错误。");

        // 测试FromVersion方法
        Assert.That(XString.FromVersion(-1), Is.EqualTo(""), "数字 -1 应转换为空版本号。");
        Assert.That(XString.FromVersion(0), Is.EqualTo("0"), "数字 0 转换为版本号结果错误。");
        Assert.That(XString.FromVersion(1), Is.EqualTo("1"), "数字 1 转换为版本号结果错误。");
        Assert.That(XString.FromVersion(10001), Is.EqualTo("1.1"), "数字转两段式版本号结果错误。");
        Assert.That(XString.FromVersion(100010001), Is.EqualTo("1.1.1"), "数字转三段式版本号结果错误。");
    }

    [TestCase("This is a long text that needs to be omitted", 10, "This is a ..", Description = "测试长文本缩略")]
    [TestCase("Short", 10, "Short", Description = "测试短于目标长度的文本")]
    [TestCase("Exactly 10", 10, "Exactly 10", Description = "测试恰好等于目标长度的文本")]
    public void Omit(string input, int length, string expected)
    {
        // Act
        var result = input.Omit(length);

        // Assert
        Assert.That(result, Is.EqualTo(expected), "文本缩略结果与预期不符。");
    }

    [TestCase("This is a long text that needs simplification", 20, "This is a...fication", Description = "测试长文本简化")]
    [TestCase("Short string", 20, "Short string", Description = "测试短于目标长度的文本")]
    [TestCase("A very long string that needs simplification", 15, "A very...cation", Description = "测试较短目标长度")]
    [TestCase("Exactly21CharsLongStr", 21, "Exactly21CharsLongStr", Description = "测试恰好等于目标长度的文本")]
    [TestCase("TooShortForEllipsis", 5, "TooSh", Description = "测试小于省略号长度的目标长度")]
    [TestCase("", 10, "", Description = "测试空字符串")]
    public void Simplify(string input, int length, string expected)
    {
        // Act
        var result = input.Simplify(length);

        // Assert
        Assert.That(result, Is.EqualTo(expected), "文本简化结果与预期不符。");
    }

    [Test]
    public void Crypt()
    {
        // 测试用例数据
        var testCases = new[]
        {
            "Hello, World!",
            "12345",
            "特殊字符!@#$%^&*()",
            "空格 测试"
        };

        foreach (var testCase in testCases)
        {
            // 测试默认加密/解密
            var encrypted = testCase.Encrypt();
            Assert.That(encrypted, Is.Not.EqualTo(testCase), "加密后的文本不应与原文相同。");

            var decrypted = encrypted.Decrypt();
            Assert.That(decrypted, Is.EqualTo(testCase), "解密后的文本应与原文相同。");

            // 测试带密钥的加密/解密
            var key1 = "abcdefgh"; // 确保8字节长度
            var encryptedWithKey1 = testCase.Encrypt(key1);
            Assert.That(encryptedWithKey1, Is.Not.EqualTo(testCase), "使用密钥加密后的文本不应与原文相同。");

            var decryptedWithKey1 = encryptedWithKey1.Decrypt(key1);
            Assert.That(decryptedWithKey1, Is.EqualTo(testCase), "使用密钥解密后的文本应与原文相同。");

            // 测试不同密钥的加密结果不同
            var key2 = "12345678"; // 确保8字节长度且与key1完全不同
            var encryptedWithKey2 = testCase.Encrypt(key2);

            // 只有在testCase不为空时才比较加密结果
            if (!string.IsNullOrEmpty(testCase))
            {
                Assert.That(encryptedWithKey1, Is.Not.EqualTo(encryptedWithKey2), "不同密钥加密的结果应该不同。");
            }
        }

        // 测试超长密钥的加密结果，不超过8个字符
        string key3 = "2e0ae1cc2e0ae1cc"; // 16字节长度
        key3.Encrypt(key3);
    }

    [Test]
    public void Random()
    {
        // 测试默认参数（format="N", length=32）
        var defaultResult = XString.Random();
        Assert.That(defaultResult, Has.Length.EqualTo(32), "默认格式应生成32位字符串。");
        Assert.That(defaultResult, Does.Match("^[0-9a-f]{32}$"), "默认格式应生成32位十六进制字符串。");

        // 测试不同的GUID格式
        // "D"格式：含连字符的32位数字
        var resultD = XString.Random("D");
        Assert.That(resultD, Has.Length.EqualTo(36), "D格式应生成36位字符串。");
        Assert.That(resultD, Does.Match("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"), "D格式应生成带连字符的GUID字符串。");

        // "B"格式：带括号、连字符的32位数字
        var resultB = XString.Random("B");
        Assert.That(resultB, Has.Length.EqualTo(38), "B格式应生成38位字符串。");
        Assert.That(resultB, Does.Match(@"^\{[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\}$"), "B格式应生成带括号和连字符的GUID字符串。");

        // "P"格式：带括号的32位数字
        var resultP = XString.Random("P");
        Assert.That(resultP, Has.Length.EqualTo(38), "P格式应生成38位字符串。");
        Assert.That(resultP, Does.Match(@"^\([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\)$"), "P格式应生成带括号和连字符的GUID字符串。");

        // 测试两次生成的随机字符串不相同
        var result1 = XString.Random();
        var result2 = XString.Random();
        Assert.That(result1, Is.Not.EqualTo(result2), "连续生成的随机字符串应该不同。");
    }
}
