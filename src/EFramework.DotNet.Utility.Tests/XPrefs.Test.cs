// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXPrefs 是 XPrefs 的单元测试。
/// </summary>
public class TestXPrefs
{
    [Test]
    public void Basic()
    {
        #region 基本操作测试
        {
            var preferences = new XPrefs.IBase();
            // 验证不存在的键返回 false
            Assert.That(preferences.Has("nonexistent"), Is.False, "不存在的键应返回 false");

            // 验证设置和检查键值
            preferences.Set("key", "value");
            Assert.That(preferences.Has("key"), Is.True, "设置后的键应该存在");

            // 验证移除键值
            preferences.Unset("key");
            Assert.That(preferences.Has("key"), Is.False, "移除后的键应该不存在");
        }
        #endregion

        #region 基本类型测试
        {
            var preferences = new XPrefs.IBase();

            var basicTests = new (string name, string key, object value, object expected)[]
            {
                ("String", "strKey", "value", "value"),
                ("Int", "intKey", 42, 42),
                ("Bool", "boolKey", true, true),
                ("Float", "floatKey", 3.14f, 3.14f)
            };

            foreach (var (name, key, value, expected) in basicTests)
            {
                preferences.Set(key, value);
                object result = name switch
                {
                    "String" => preferences.GetString(key),
                    "Int" => preferences.GetInt(key),
                    "Bool" => preferences.GetBool(key),
                    "Float" => preferences.GetFloat(key),
                    _ => null
                };
                Assert.That(result, Is.EqualTo(expected), $"{name} 类型的值应正确存储和读取");
            }
        }
        #endregion

        #region IBase对象测试
        {
            var preferences = new XPrefs.IBase();
            var child = new XPrefs.IBase();
            child.Set("stringKey", "childValue");
            child.Set("intKey", 42);
            child.Set("arrayKey", new[] { 1, 2, 3 });

            // 验证嵌套对象的存储
            Assert.That(preferences.Set("childPrefs", child), Is.True, "应成功存储嵌套的配置对象");
            var retrieved = preferences.Get<XPrefs.IBase>("childPrefs");
            Assert.That(retrieved, Is.Not.Null, "应能获取到嵌套的配置对象");
            Assert.That(retrieved.GetString("stringKey"), Is.EqualTo("childValue"), "嵌套对象中的字符串值应正确保存");
            Assert.That(retrieved.GetInt("intKey"), Is.EqualTo(42), "嵌套对象中的整数值应正确保存");
            Assert.That(retrieved.GetInts("arrayKey"), Is.EqualTo(new[] { 1, 2, 3 }), "嵌套对象中的数组应正确保存");

            // 深层嵌套测试
            var grandChild = new XPrefs.IBase();
            grandChild.Set("deepKey", "deepValue");
            child.Set("grandChild", grandChild);

            var deepRetrieved = preferences.Get<XPrefs.IBase>("childPrefs").Get<XPrefs.IBase>("grandChild");
            Assert.That(deepRetrieved, Is.Not.Null, "应能获取到深层嵌套的配置对象");
            Assert.That(deepRetrieved.GetString("deepKey"), Is.EqualTo("deepValue"), "深层嵌套对象中的值应正确保存");
        }
        #endregion

        #region 默认值测试
        {
            var preferences = new XPrefs.IBase();
            // 验证各种类型的默认值返回
            Assert.That(preferences.Get("missing", "default"), Is.EqualTo("default"), "缺失的字符串键应返回默认值");
            Assert.That(preferences.Get("missing", 100), Is.EqualTo(100), "缺失的整数键应返回默认值");
            Assert.That(preferences.Get("missing", true), Is.True, "缺失的布尔键应返回默认值");
            Assert.That(preferences.Get("missing", 1.23f), Is.EqualTo(1.23f), "缺失的浮点数键应返回默认值");

            // 验证数组类型的默认值返回
            Assert.That(preferences.Get("missing", new[] { "default" }), Is.EqualTo(new[] { "default" }), "缺失的字符串数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { 1, 2 }), Is.EqualTo(new[] { 1, 2 }), "缺失的整数数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { 1.1f }), Is.EqualTo(new[] { 1.1f }), "缺失的浮点数数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { true }), Is.EqualTo(new[] { true }), "缺失的布尔数组键应返回默认数组");
        }
        #endregion

        #region 数组类型测试
        {
            var preferences = new XPrefs.IBase();

            var arrayTests = new (string name, string key, object value, object expected)[]
            {
                    ("String Array", "strArray", new[] { "a", "b", "c" }, new[] { "a", "b", "c" }),
                    ("Int Array", "intArray", new[] { 1, 2, 3 }, new[] { 1, 2, 3 }),
                    ("Float Array", "floatArray", new[] { 1.1f, 2.2f, 3.3f }, new[] { 1.1f, 2.2f, 3.3f }),
                    ("Bool Array", "boolArray", new[] { true, false, true }, new[] { true, false, true })
            };

            foreach (var (name, key, value, expected) in arrayTests)
            {
                preferences.Set(key, value);
                object result = name switch
                {
                    "String Array" => preferences.GetStrings(key),
                    "Int Array" => preferences.GetInts(key),
                    "Float Array" => preferences.GetFloats(key),
                    "Bool Array" => preferences.GetBools(key),
                    _ => null
                };
                Assert.That((System.Array)result, Is.EqualTo((System.Array)expected), $"{name} 类型的数组应正确存储和读取");
            }
        }
        #endregion

        #region 相等性测试
        {
            var preferences1 = new XPrefs.IBase();
            preferences1.Set("intKey", 42);
            preferences1.Set("floatKey", 3.14f);
            preferences1.Set("boolKey", true);
            preferences1.Set("stringsKey", new[] { "a", "b", "c" });
            preferences1.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            preferences1.Set("boolsKey", new[] { true, false, true });

            var child1 = new XPrefs.IBase();
            child1.Set("key", "childValue");
            preferences1.Set("child", child1);

            var preferences2 = new XPrefs.IBase();
            preferences2.Set("intKey", 42);
            preferences2.Set("floatKey", 3.14f);
            preferences2.Set("boolKey", true);
            preferences2.Set("stringsKey", new[] { "a", "b", "c" });
            preferences2.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            preferences2.Set("boolsKey", new[] { true, false, true });

            var child2 = new XPrefs.IBase();
            child2.Set("key", "childValue");
            preferences2.Set("child", child2);

            Assert.That(preferences1.Equals(preferences2), Is.True, "具有相同内容的配置对象应该相等");
        }
        #endregion
    }

    [Test]
    public void Sources()
    {
        try
        {
            #region 初始化测试数据
            {
                // 初始化Asset测试数据
                XPrefs.asset = null;
                XPrefs.Asset.writable = true; // 设置为可写
                XPrefs.Asset.Set("intKey", 42);
                XPrefs.Asset.Set("intsKey", new[] { 1, 2, 3 });
                XPrefs.Asset.Set("stringKey", "assetValue");
                XPrefs.Asset.Set("floatKey", 3.14f);
                XPrefs.Asset.Set("boolKey", true);
                XPrefs.Asset.Set("stringsKey", new[] { "a", "b", "c" });
                XPrefs.Asset.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
                XPrefs.Asset.Set("boolsKey", new[] { true, false, true });

                // 初始化Local测试数据
                XPrefs.Local.Set("localIntKey", 100);
                XPrefs.Local.Set("localIntsKey", new[] { 4, 5, 6 });
                XPrefs.Local.Set("localStringKey", "localValue");
                XPrefs.Local.Set("overrideKey", "localOverride");
            }
            #endregion

            #region HasKey测试
            {
                Assert.That(XPrefs.HasKey("intKey"), Is.True, "Asset 配置中应存在 intKey");
                Assert.That(XPrefs.HasKey("nonexistentKey"), Is.False, "不存在的键应返回 false");
                Assert.That(XPrefs.HasKey("localIntKey", XPrefs.Local), Is.True, "Local 配置中应存在 localIntKey");
                Assert.That(XPrefs.HasKey("intKey", XPrefs.Local, XPrefs.Asset), Is.True, "多配置源中应能找到 intKey");
                Assert.That(XPrefs.HasKey("nonexistentKey", XPrefs.Local, XPrefs.Asset), Is.False, "多配置源中不存在的键应返回 false");
            }
            #endregion

            #region GetInt测试
            {
                Assert.That(XPrefs.GetInt("intKey"), Is.EqualTo(42), "应正确获取 Asset 中的整数值");
                Assert.That(XPrefs.GetInt("localIntKey", 0, XPrefs.Local), Is.EqualTo(100), "应正确获取 Local 中的整数值");
                Assert.That(XPrefs.GetInt("nonexistentKey", 999, XPrefs.Local, XPrefs.Asset), Is.EqualTo(999), "获取不存在的键应返回默认值");
                Assert.That(XPrefs.GetInt("floatKey"), Is.EqualTo(3), "浮点数应正确转换为整数");
            }
            #endregion

            #region GetInts测试
            {
                Assert.That(XPrefs.GetInts("intsKey"), Is.EqualTo(new[] { 1, 2, 3 }), "应正确获取 Asset 中的整数数组");
                Assert.That(XPrefs.GetInts("localIntsKey", null, XPrefs.Local), Is.EqualTo(new[] { 4, 5, 6 }), "应正确获取 Local 中的整数数组");
                Assert.That(XPrefs.GetInts("nonexistentKey", new[] { 7, 8, 9 }, XPrefs.Local, XPrefs.Asset), Is.EqualTo(new[] { 7, 8, 9 }), "获取不存在的数组应返回默认值");
            }
            #endregion

            #region Get基本类型测试
            {
                Assert.That(XPrefs.GetString("stringKey"), Is.EqualTo("assetValue"), "应正确获取字符串值");
                Assert.That(XPrefs.GetFloat("floatKey"), Is.EqualTo(3.14f), "应正确获取浮点数值");
                Assert.That(XPrefs.GetBool("boolKey"), Is.True, "应正确获取布尔值");
                Assert.That(XPrefs.GetString("overrideKey", "", XPrefs.Local, XPrefs.Asset), Is.EqualTo("localOverride"), "Local 配置应覆盖 Asset 配置");
            }
            #endregion

            #region 类型特定测试
            {
                Assert.That(XPrefs.GetString("stringKey"), Is.EqualTo("assetValue"), "GetString 应正确获取字符串值");
                Assert.That(XPrefs.GetString("nonexistentKey", "default"), Is.EqualTo("default"), "GetString 应返回默认值");
                Assert.That(XPrefs.GetStrings("stringsKey"), Is.EqualTo(new[] { "a", "b", "c" }), "GetStrings 应正确获取字符串数组");
                Assert.That(XPrefs.GetFloat("floatKey"), Is.EqualTo(3.14f), "GetFloat 应正确获取浮点数值");

                var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
                var actualFloats = XPrefs.GetFloats("floatsKey");
                for (int i = 0; i < expectedFloats.Length; i++)
                {
                    Assert.That(actualFloats[i], Is.EqualTo(expectedFloats[i]), "GetFloats 应正确获取浮点数数组");
                }

                Assert.That(XPrefs.GetBool("boolKey"), Is.True, "GetBool 应正确获取布尔值");
                Assert.That(XPrefs.GetBools("boolsKey"), Is.EqualTo(new[] { true, false, true }), "GetBools 应正确获取布尔数组");
            }
            #endregion

            #region 边界情况测试
            {
                Assert.That(XPrefs.GetInt("intKey", 0, null), Is.EqualTo(42), "空配置源列表应默认使用 Asset");
                Assert.That(XPrefs.GetInt("intKey", 0), Is.EqualTo(42), "无配置源应默认使用 Asset");
            }
            #endregion

            #region 类型不匹配测试
            {
                XPrefs.Asset.Set("mismatchKey", "not an int");
                Assert.That(XPrefs.GetInt("mismatchKey"), Is.EqualTo(0), "类型不匹配时应返回类型默认值");
            }
            #endregion
        }
        catch (Exception e) { throw e; }
        finally
        {
            // 清理测试数据
            XPrefs.Asset.Unset("intKey");
            XPrefs.Asset.Unset("intsKey");
            XPrefs.Asset.Unset("stringKey");
            XPrefs.Asset.Unset("floatKey");
            XPrefs.Asset.Unset("boolKey");
            XPrefs.Asset.Unset("stringsKey");
            XPrefs.Asset.Unset("floatsKey");
            XPrefs.Asset.Unset("boolsKey");
            XPrefs.Asset.Unset("mismatchKey");
            XPrefs.Asset.writable = false; // 恢复为只读

            XPrefs.Local.Unset("localIntKey");
            XPrefs.Local.Unset("localIntsKey");
            XPrefs.Local.Unset("localStringKey");
            XPrefs.Local.Unset("overrideKey");
        }
    }

    [Test]
    public void Persist()
    {
        #region 准备测试环境
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs");
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            var testFile = XFile.PathJoin(tmpDir, "test_persist.json");
            var preferences = new XPrefs.IBase();

            // 准备测试数据
            var testData = @"{
                    ""stringKey"": ""stringValue"",
                    ""intKey"": 123,
                    ""boolKey"": true,
                    ""intSliceKey"": [1, 2, 3],
                    ""floatSliceKey"": [1.1, 2.2, 3.3],
                    ""stringSliceKey"": [""a"", ""b"", ""c""],
                    ""boolSliceKey"": [true, false, true]
                }";

            // 写入测试文件
            XFile.SaveText(testFile, testData);
            #endregion

            #region 测试读取配置
            {
                Assert.That(preferences.Read(testFile), Is.True, "Should read file successfully");

                // 验证各种类型的数据
                Assert.That(preferences.GetString("stringKey"), Is.EqualTo("stringValue"));
                Assert.That(preferences.GetInt("intKey"), Is.EqualTo(123));
                Assert.That(preferences.GetBool("boolKey"), Is.True);
                Assert.That(preferences.GetInts("intSliceKey"), Is.EqualTo(new[] { 1, 2, 3 }));

                var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
                var actualFloats = preferences.GetFloats("floatSliceKey");
                for (int i = 0; i < expectedFloats.Length; i++)
                {
                    Assert.That(actualFloats[i], Is.EqualTo(expectedFloats[i]));
                }

                Assert.That(preferences.GetStrings("stringSliceKey"), Is.EqualTo(new[] { "a", "b", "c" }));
                Assert.That(preferences.GetBools("boolSliceKey"), Is.EqualTo(new[] { true, false, true }));
            }
            #endregion

            #region 测试读取不存在的文件
            {
                var nonExistentFile = XFile.PathJoin(tmpDir, "nonexistent.json");
                Assert.That(preferences.Read(nonExistentFile), Is.False, "Should fail reading non-existent file");
            }
            #endregion

            #region 测试读取无效的JSON
            {
                var invalidFile = XFile.PathJoin(tmpDir, "invalid.json");
                XFile.SaveText(invalidFile, "invalid json");
                Assert.That(preferences.Read(invalidFile), Is.False, "Should fail reading invalid JSON");
            }
            #endregion

            #region 测试复杂JSON
            {
                var complexData = @"{
                    ""nullValue"": null,
                    ""emptyObject"": {},
                    ""emptyArray"": [],
                    ""nestedObject"": {
                        ""key"": ""value""
                    },
                    ""mixedArray"": [1, ""two"", true, null]
                }";

                var complexFile = XFile.PathJoin(tmpDir, "complex.json");
                XFile.SaveText(complexFile, complexData);

                var complexPrefs = new XPrefs.IBase();
                Assert.That(complexPrefs.Read(complexFile), Is.True);

                Assert.That(complexPrefs.Get<object>("nullValue"), Is.Null);
                Assert.That(complexPrefs.Get<XPrefs.IBase>("emptyObject"), Is.Not.Null);
                Assert.That(complexPrefs.Get<object[]>("emptyArray"), Is.Null);
                Assert.That(complexPrefs.Get<XPrefs.IBase>("nestedObject"), Is.Not.Null);
                Assert.That(complexPrefs.Get<object[]>("mixedArray"), Is.Null);
            }
            #endregion

            #region 测试大文件
            {
                var largePrefs = new XPrefs.IBase();
                for (int i = 0; i < 1000; i++)
                {
                    largePrefs.Set($"key{i}", $"value{i}");
                }

                var largeFile = XFile.PathJoin(tmpDir, "large.json");
                largePrefs.File = largeFile;
                Assert.That(largePrefs.Save(), Is.True, "Should save large file successfully");

                var loadedLargePrefs = new XPrefs.IBase();
                Assert.That(loadedLargePrefs.Read(largeFile), Is.True);
                Assert.That(loadedLargePrefs.GetString("key42"), Is.EqualTo("value42"));
            }
            #endregion
        }
        finally
        {
            // 清理测试目录
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
    }

    [Test]
    public void Eval()
    {
        #region 基本替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("name", "John");
            pf.Set("greeting", "Hello ${Preferences.name}");

            var result = pf.Eval("${Preferences.greeting}");
            Assert.That(result, Is.EqualTo("Hello John"));
        }
        #endregion

        #region 缺失变量测试
        {
            var pf = new XPrefs.IBase();
            var result = pf.Eval("${Preferences.missing}");
            Assert.That(result, Is.EqualTo("${Preferences.missing}(Unknown)"));
        }
        #endregion

        #region 递归变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("recursive1", "${Preferences.recursive2}");
            pf.Set("recursive2", "${Preferences.recursive1}");

            var result = pf.Eval("${Preferences.recursive1}");
            Assert.That(result, Is.EqualTo("${Preferences.recursive1}(Recursive)"));
        }
        #endregion

        #region 嵌套变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("outer", "value");

            var result = pf.Eval("${Preferences.outer${Preferences.inner}}");
            Assert.That(result, Is.EqualTo("${Preferences.outer${Preferences.inner}(Nested)}"));
        }
        #endregion

        #region 多重替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("first", "John");
            pf.Set("last", "Doe");

            var child = new XPrefs.IBase();
            child.Set("name", "Mike");
            pf.Set("child", child);

            var result = pf.Eval("${Preferences.first} and ${Preferences.last} has a child named ${Preferences.child.name} age ${Preferences.child.age}");
            Assert.That(result, Is.EqualTo("John and Doe has a child named Mike age ${Preferences.child.age}(Unknown)"));
        }
        #endregion

        #region 空值测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("empty", "");

            var result = pf.Eval("test${Preferences.empty}end");
            Assert.That(result, Is.EqualTo("test${Preferences.empty}(Unknown)end"));
        }
        #endregion
    }

    [Test]
    public void Override()
    {
        #region 准备测试环境
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            // 准备配置文件
            var configData = @"{
                    ""key1"": ""value1"",
                    ""key2"": 42
                }";

            var assetFile = XFile.PathJoin(tmpDir, "asset.json");
            var localFile = XFile.PathJoin(tmpDir, "local.json");
            var customLocalFile = XFile.PathJoin(tmpDir, "custom_local.json");

            XFile.SaveText(assetFile, configData);
            XFile.SaveText(localFile, configData);
            XFile.SaveText(customLocalFile, @"{
                    ""customKey"": ""customValue""
                }");

            try
            {
                #region 测试Local配置文件路径覆盖
                {
                    XEnv.ParseArgs(true, "--Preferences@Local=" + customLocalFile);
                    XPrefs.local = null;
                    var local = XPrefs.Local;
                    Assert.That(local.File, Is.EqualTo(customLocalFile));
                    Assert.That(local.GetString("customKey"), Is.EqualTo("customValue"));
                }
                #endregion

                #region 测试Local配置文件不存在时的行为
                {
                    XEnv.ParseArgs(true, "--Preferences@Local=nonexistent.json");
                    XPrefs.local = null;
                    var local = XPrefs.Local;
                    Assert.That(local.File, Is.EqualTo("nonexistent.json"));
                    Assert.That(local.Has("key1"), Is.False, "文件不存在时应该是空配置");
                }
                #endregion

                #region 测试Asset配置文件路径覆盖
                {
                    XEnv.ParseArgs(true, "--Preferences@Asset=" + customLocalFile);
                    XPrefs.asset = null;
                    var asset = XPrefs.Asset;
                    Assert.That(asset.File, Is.EqualTo(customLocalFile));
                    Assert.That(asset.GetString("customKey"), Is.EqualTo("customValue"));
                }
                #endregion

                #region 测试Asset配置文件不存在时的行为
                {
                    XEnv.ParseArgs(true, "--Preferences@Asset=nonexistent.json");
                    XPrefs.asset = null;
                    var asset = XPrefs.Asset;
                    Assert.That(asset.File, Is.EqualTo("nonexistent.json"));
                    Assert.That(asset.Has("key1"), Is.False, "文件不存在时应该是空配置"); // 文件不存在时应该是空配置
                }
                #endregion

                #region 测试Asset和Local参数混合
                {
                    XEnv.ParseArgs(true,
                        "--Preferences@Asset.key2=100",
                        "--Preferences@Asset.key3=asset value",
                        "--Preferences@Local.key2=200",
                        "--Preferences@Local.key3=local value",
                        "--Preferences@Local=" + localFile
                    );
                    XPrefs.local = null;
                    var local = XPrefs.Local;

                    var asset = new XPrefs.IAsset();
                    asset = new XPrefs.IAsset();
                    Assert.That(asset.Read(assetFile), Is.True);

                    // 验证Asset结果
                    Assert.That(asset.GetString("key1"), Is.EqualTo("value1"), "原值保持不变");
                    Assert.That(asset.GetInt("key2"), Is.EqualTo(100), "被Asset命令行参数覆盖");
                    Assert.That(asset.GetString("key3"), Is.EqualTo("asset value"), "Asset新增参数");

                    // 验证Local结果
                    Assert.That(local.GetString("key1"), Is.EqualTo("value1"), "原值保持不变");
                    Assert.That(local.GetInt("key2"), Is.EqualTo(200), "被Local命令行参数覆盖");
                    Assert.That(local.GetString("key3"), Is.EqualTo("local value"), "Local新增参数");
                }

                #endregion

                #region 测试多级路径覆盖
                {
                    XEnv.ParseArgs(true,
                        "--Preferences.Log.Std.Config.Level=Debug",
                        "--Preferences@Asset.UI.Window.Style.Theme=Dark",
                        "--Preferences@Local.Network.Server.Config.Port=8080",
                        "--Preferences@Local=" + localFile
                    );

                    XPrefs.local = null;
                    var local = XPrefs.Local;
                    var asset = new XPrefs.IAsset();
                    Assert.That(asset.Read(assetFile), Is.True);

                    // 验证Asset多级路径
                    var logConfig = asset.Get<XPrefs.IBase>("Log")
                                        .Get<XPrefs.IBase>("Std")
                                        .Get<XPrefs.IBase>("Config");
                    Assert.That(logConfig.GetString("Level"), Is.EqualTo("Debug"));

                    var uiConfig = asset.Get<XPrefs.IBase>("UI")
                                       .Get<XPrefs.IBase>("Window")
                                       .Get<XPrefs.IBase>("Style");
                    Assert.That(uiConfig.GetString("Theme"), Is.EqualTo("Dark"));

                    // 验证Local多级路径
                    var networkConfig = local.Get<XPrefs.IBase>("Network")
                                           .Get<XPrefs.IBase>("Server")
                                           .Get<XPrefs.IBase>("Config");
                    Assert.That(networkConfig.GetString("Port"), Is.EqualTo("8080"));
                }
                #endregion

                #region 测试多层覆盖优先级
                {
                    XEnv.ParseArgs(true,
                        "--Preferences.sharedKey=base value",
                        "--Preferences@Asset.sharedKey=asset value",
                        "--Preferences@Local.sharedKey=local value",
                        "--Preferences@Local=" + localFile
                    );

                    XPrefs.local = null;
                    var local = XPrefs.Local;
                    var asset = new XPrefs.IAsset();
                    Assert.That(asset.Read(assetFile), Is.True);

                    Assert.That(asset.GetString("sharedKey"), Is.EqualTo("asset value"), "Asset特定覆盖优先");
                    Assert.That(local.GetString("sharedKey"), Is.EqualTo("local value"), "Local特定覆盖优先");
                }
                #endregion

                #region 测试Local配置文件和参数覆盖的顺序
                {
                    var localData = @"{
                        ""orderKey"": ""file value""
                    }";
                    XFile.SaveText(localFile, localData);

                    XEnv.ParseArgs(true,
                        "--Preferences@Local.orderKey=override value",
                        "--Preferences@Local=" + localFile
                    );

                    XPrefs.local = null;
                    var local = XPrefs.Local;
                    Assert.That(local.GetString("orderKey"), Is.EqualTo("override value"), "命令行参数应该优先于文件内容");
                }
                #endregion
            }
            catch (Exception e) { throw e; }
            finally
            {
                // 重置测试参数
                XEnv.ParseArgs(true);
                XPrefs.asset = null;
                XPrefs.local = null;
            }
        }
        catch (Exception e) { throw e; }
        finally
        {
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
        #endregion
    }

    internal class MyPreferencesServer : IDisposable
    {
        internal HttpListener listener;
        internal string baseUrl;
        internal Func<HttpListenerRequest, (int statusCode, string content)> handler;

        internal string BaseUrl => baseUrl;

        internal MyPreferencesServer(Func<HttpListenerRequest, (int statusCode, string content)> handler)
        {
            this.handler = handler;

            // 使用 TcpListener 查找可用端口
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var endPoint = (IPEndPoint)tcpListener.LocalEndpoint;
            var port = endPoint.Port;
            tcpListener.Stop();

            // 使用找到的端口创建 HttpListener
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            baseUrl = $"http://localhost:{port}";
        }

        internal MyPreferencesServer Start()
        {
            _ = Task.Run(async () =>
            {
                while (listener.IsListening)
                {
                    try
                    {
                        var context = await listener.GetContextAsync();
                        var (statusCode, content) = handler(context.Request);

                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";
                        var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.Close();
                    }
                    catch { }
                }
            });
            return this;
        }

        public void Dispose()
        {
            listener?.Stop();
            listener?.Close();
        }
    }

    internal class MyPreferencesHandler : XPrefs.IRemote.IHandler
    {
        internal bool onStartedCalled = false;
        internal bool onRequestCalled = false;
        internal bool onSucceededCalled = false;
        internal bool onFailedCalled = false;
        internal int expectedRetryCount = 0;
        internal float expectedPending = 0;
        internal int retryCount = 0;

        public string Uri { get; set; }

        public void OnStarted(XPrefs.IRemote context) { onStartedCalled = true; }

        public void OnRequest(XPrefs.IRemote context, HttpRequestMessage request)
        {
            request.Headers.Add("Token", "test-token");
            onRequestCalled = true;
        }

        public bool OnRetry(XPrefs.IRemote context, int count, out float pending)
        {
            pending = expectedPending;
            retryCount = count;
            return count < expectedRetryCount;
        }

        public void OnSucceeded(XPrefs.IRemote context) { onSucceededCalled = true; }

        public void OnFailed(XPrefs.IRemote context) { onFailedCalled = true; }
    }

    [Test]
    public async Task Remote()
    {
        #region 网络错误
        {
            var handler = new MyPreferencesHandler
            {
                Uri = "http://invalid.com/file.json",
                expectedRetryCount = 3,
                expectedPending = 1.0f
            };
            XPrefs.Remote.Client.Timeout = TimeSpan.FromSeconds(1);
            await XPrefs.Remote.Read(handler);
            Assert.That(handler.onStartedCalled, Is.True, "OnStarted 应当被调用。");
            Assert.That(handler.onRequestCalled, Is.True, "OnRequest 应当被调用。");
            Assert.That(handler.onSucceededCalled, Is.False, "OnSucceeded 不应当被调用。");
            Assert.That(handler.onFailedCalled, Is.True, "OnFailed 应当被调用。");
            Assert.That(string.IsNullOrEmpty(XPrefs.Remote.Error), Is.False, "Error 不应当为空。");
            Assert.That(handler.retryCount, Is.EqualTo(3), "重试次数应当为 3。");
        }
        #endregion

        #region 解析错误
        {
            using var server = new MyPreferencesServer((request) => { return (200, "invalid content"); }).Start();
            var handler = new MyPreferencesHandler { Uri = server.BaseUrl };
            await XPrefs.Remote.Read(handler);
            Assert.That(handler.onStartedCalled, Is.True, "OnStarted 应当被调用。");
            Assert.That(handler.onRequestCalled, Is.True, "OnRequest 应当被调用。");
            Assert.That(handler.onSucceededCalled, Is.False, "OnSucceeded 不应当被调用。");
            Assert.That(handler.onFailedCalled, Is.True, "OnFailed 应当被调用。");
            Assert.That(XPrefs.Remote.Error, Does.Contain("Request preferences succeeded, but parsing failed"), "Error 应当包含解析错误。");
        }
        #endregion

        #region 正常流程
        {
            using var server = new MyPreferencesServer((request) => { return (200, "{\"key\":\"value\"}"); }).Start();
            var handler = new MyPreferencesHandler { Uri = server.BaseUrl };
            await XPrefs.Remote.Read(handler);
            Assert.That(handler.onStartedCalled, Is.True, "OnStarted 应当被调用。");
            Assert.That(handler.onRequestCalled, Is.True, "OnRequest 应当被调用。");
            Assert.That(handler.onSucceededCalled, Is.True, "OnSucceeded 应当被调用。");
            Assert.That(handler.onFailedCalled, Is.False, "OnFailed 不应当被调用。");
            Assert.That(string.IsNullOrEmpty(XPrefs.Remote.Error), Is.True, "Error 应当为空。");
            Assert.That(XPrefs.Remote.GetString("key"), Is.EqualTo("value"), "值应当为 value。");
        }
        #endregion
    }
}
