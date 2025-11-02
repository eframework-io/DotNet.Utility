// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXEnv 是 XEnv 的单元测试。
/// </summary>
public class TestXEnv
{
    [OneTimeTearDown]
    public void Cleanup() { XEnv.ParseArgs(reset: true); }

    [Test]
    public void Initialize()
    {
        var envFile = XFile.PathJoin(XEnv.ProjectPath, ".env");
        var tempEnvFile = XFile.PathJoin(XEnv.ProjectPath, ".env.temp");
        try
        {
            if (XFile.HasFile(envFile))
            {
                File.Move(envFile, tempEnvFile);
                XFile.DeleteFile(envFile);
            }
            File.AppendAllLines(envFile, new string[] { " key1 = value1 ", "key2=", "key3", "# key4=value4" });
            XEnv.Initialize();

            Assert.That(Environment.GetEnvironmentVariable("key1"), Is.EqualTo("value1"), "环境变量应正确解析");
            Assert.That(Environment.GetEnvironmentVariable("key2"), Is.Null, "未赋值的环境变量应当解析为空");
            Assert.That(Environment.GetEnvironmentVariable("key3"), Is.Null, "非法的变量不应当被解析");
            Assert.That(Environment.GetEnvironmentVariable("key4"), Is.Null, "注释的变量不应当被解析");
        }
        catch (Exception e) { throw e; }
        finally
        {
            if (XFile.HasFile(envFile)) XFile.DeleteFile(envFile);
            if (XFile.HasFile(tempEnvFile)) File.Move(tempEnvFile, envFile);
            Environment.SetEnvironmentVariable("key1", string.Empty);
        }
    }

    [Test]
    public void Metas()
    {
        Assert.That(XEnv.Platform, Is.Not.EqualTo(XEnv.PlatformType.Unknown), "平台类型不应为未知");
        Assert.That(XEnv.Solution, Is.Not.Empty, "解决方案名称不应为空");
        Assert.That(XEnv.Project, Is.Not.Empty, "项目名称不应为空");
        Assert.That(XEnv.Product, Is.Not.Empty, "产品名称不应为空");
        Assert.That(XEnv.Channel, Is.Not.Empty, "发布渠道不应为空");
        Assert.That(XEnv.Version, Is.Not.Empty, "版本号不应为空");
        Assert.That(XEnv.Author, Is.Not.Empty, "作者名称不应为空");
    }

    [Test]
    public void Preferences()
    {
        Assert.That(XEnv.Preferences.AppDefault, Is.Not.Empty, "XEnv/App 应用类型默认值不应为空");
        Assert.That(XEnv.Preferences.ModeDefault, Is.Not.Empty, "XEnv/Mode 运行模式默认值不应为空");
        Assert.That(XEnv.Preferences.SolutionDefault, Is.Not.Empty, "XEnv/Solution 解决方案名称默认值不应为空");
        Assert.That(XEnv.Preferences.ProjectDefault, Is.Not.Empty, "XEnv/Project 项目名称默认值不应为空");
        Assert.That(XEnv.Preferences.ProductDefault, Is.Not.Empty, "XEnv/Product 产品名称默认值不应为空");
        Assert.That(XEnv.Preferences.ChannelDefault, Is.Not.Empty, "XEnv/Channel 发布渠道默认值不应为空");
        Assert.That(XEnv.Preferences.VersionDefault, Is.Not.Empty, "XEnv/Version 版本号默认值不应为空");
        Assert.That(XEnv.Preferences.AuthorDefault, Is.Not.Empty, "XEnv/Author 作者名称默认值不应为空");
        Assert.That(XEnv.Preferences.SecretDefault, Is.Not.Empty, "XEnv/Secret 应用密钥默认值不应为空");
        Assert.That(XEnv.Preferences.RemoteDefault, Is.Not.Empty, "XEnv/Remote 远端配置默认值不应为空");
    }

    /// <summary>
    /// 测试路径管理功能。
    /// </summary>
    [Test]
    public void Paths()
    {
        var tempDir = XFile.PathJoin(XEnv.LocalPath, "TestXEnv-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tempDir)) XFile.DeleteDirectory(tempDir);

        try
        {
            // 验证 ProjectPath 是否存在
            Assert.That(XFile.HasDirectory(XEnv.ProjectPath), Is.True);

            // 验证 AssetPath 是否正确
            Assert.That(XEnv.AssetPath, Is.EqualTo(XFile.PathJoin(XEnv.ProjectPath, "Assets")));

            // 验证 LocalPath 是否创建
            Assert.That(XEnv.LocalPath, Is.EqualTo(XFile.PathJoin(XEnv.ProjectPath, "Local")));

            // 测试自定义LocalPath
            var customLocal = XFile.PathJoin(tempDir, "CustomLocal");

            // 重置参数缓存并设置自定义路径
            XEnv.ParseArgs(true, "-LocalPath", customLocal);
            XEnv.localPath = null;

            // 验证自定义值
            Assert.That(Path.GetFullPath(customLocal), Is.EqualTo(Path.GetFullPath(XEnv.LocalPath)));

            // 验证本地目录已创建
            Assert.That(XFile.HasDirectory(XEnv.LocalPath), Is.True);
        }
        finally
        {
            XFile.DeleteDirectory(tempDir);
            XEnv.ParseArgs(true);
            XEnv.localPath = null;
        }
    }

    /// <summary>
    /// 测试命令行参数解析功能。
    /// </summary>
    [Test]
    public void Args()
    {
        try
        {
            // 测试用例1：基本参数形式
            {
                XEnv.ParseArgs(true, "--test=value");
                Assert.That(XEnv.GetArg("test"), Is.EqualTo("value"), "基本参数形式应正确解析键值对");
            }

            // 测试用例2：多种参数形式
            {
                XEnv.ParseArgs(true,
                    "--key1=value1",          // 双横杠等号
                    "-key2=value2",           // 单横杠等号
                    "--flag1",                // 双横杠无值
                    "-flag2",                 // 单横杠无值
                    "--key3=value3",          // 双横杠等号
                    "-key4", "value4",        // 单横杠空格
                    "--key5", "value5",       // 双横杠空格
                    "-flag3",                 // 单横杠无值
                    "-key6=with=equals",      // 单横杠多等号
                    "--key7=with=equals"      // 双横杠多等号
                );

                Assert.That(XEnv.GetArg("key1"), Is.EqualTo("value1"), "双横杠等号形式的参数应正确解析");
                Assert.That(XEnv.GetArg("key2"), Is.EqualTo("value2"), "单横杠等号形式的参数应正确解析");
                Assert.That(XEnv.GetArg("flag1"), Is.EqualTo(""), "双横杠无值标志应解析为空字符串");
                Assert.That(XEnv.GetArg("flag2"), Is.EqualTo(""), "单横杠无值标志应解析为空字符串");
                Assert.That(XEnv.GetArg("key3"), Is.EqualTo("value3"), "双横杠等号形式的参数应正确解析");
                Assert.That(XEnv.GetArg("key4"), Is.EqualTo("value4"), "单横杠空格分隔的参数应正确解析");
                Assert.That(XEnv.GetArg("key5"), Is.EqualTo("value5"), "双横杠空格分隔的参数应正确解析");
                Assert.That(XEnv.GetArg("flag3"), Is.EqualTo(""), "单横杠无值标志应解析为空字符串");
                Assert.That(XEnv.GetArg("key6"), Is.EqualTo("with=equals"), "含等号的值应完整保留");
                Assert.That(XEnv.GetArg("key7"), Is.EqualTo("with=equals"), "含等号的值应完整保留");
            }

            // 测试用例3：特殊值处理
            {
                XEnv.ParseArgs(true,
                    "--empty=",
                    "--spaces=value with spaces",
                    "--chinese=中文参数",
                    "--symbols=!@#$%^&*()",
                    "--multi=value=with=equals"
                );

                Assert.That(XEnv.GetArg("empty"), Is.EqualTo(""), "空值参数应解析为空字符串");
                Assert.That(XEnv.GetArg("spaces"), Is.EqualTo("value with spaces"), "含空格的值应完整保留");
                Assert.That(XEnv.GetArg("chinese"), Is.EqualTo("中文参数"), "中文参数应正确解析");
                Assert.That(XEnv.GetArg("symbols"), Is.EqualTo("!@#$%^&*()"), "特殊字符应完整保留");
                Assert.That(XEnv.GetArg("multi"), Is.EqualTo("value=with=equals"), "多等号的值应完整保留");
            }

            // 测试用例4：缓存控制
            {
                XEnv.ParseArgs(false, "--newkey=newvalue");
                Assert.That(XEnv.GetArg("chinese"), Is.EqualTo("中文参数"), "不重置缓存时应保留原有参数值");
                Assert.That(XEnv.GetArg("symbols"), Is.EqualTo("!@#$%^&*()"), "不重置缓存时应保留原有参数值");
            }

            // 测试用例5：重置缓存
            {
                XEnv.ParseArgs(true, "--single=value");
                Assert.That(XEnv.GetArg("single"), Is.EqualTo("value"), "重置缓存后新参数应生效");
                Assert.That(XEnv.GetArg("chinese"), Is.EqualTo(""), "重置缓存后原有参数应被清除");
            }

            // 测试用例6：空参数列表
            XEnv.ParseArgs(true);

            // 测试用例7：无效参数形式
            {
                XEnv.ParseArgs(true,
                    "invalid",
                    "--valid=value",
                    "--"
                );
                Assert.That(XEnv.GetArg("valid"), Is.EqualTo("value"), "有效参数应正确解析，忽略无效参数");
            }

            // 测试用例8：参数列表直接访问
            {
                var args = XEnv.ParseArgs(true,
                    "--key1=value1",
                    "--key2=value2"
                );

                var foundKey1 = false;
                var foundKey2 = false;

                foreach (var pair in args)
                {
                    if (pair.Key == "key1")
                    {
                        Assert.That(pair.Value, Is.EqualTo("value1"), "参数列表中的键值对应正确保存");
                        foundKey1 = true;
                    }
                    else if (pair.Key == "key2")
                    {
                        Assert.That(pair.Value, Is.EqualTo("value2"), "参数列表中的键值对应正确保存");
                        foundKey2 = true;
                    }
                }

                Assert.That(foundKey1, Is.True, "参数列表应包含第一个测试键值对");
                Assert.That(foundKey2, Is.True, "参数列表应包含第二个测试键值对");
            }
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }

    /// <summary>
    /// 测试环境变量解析功能。
    /// </summary>
    [Test]
    public void Eval()
    {
        try
        {
            var testVar = $"TEST_VAR_{XTime.GetMillisecond()}";

            #region 测试命令行参数解析
            {
                XEnv.ParseArgs(true, "-test", "value");
                var result1 = "prefix ${Environment.test} suffix".Eval(XEnv.Instance);
                Assert.That(result1, Is.EqualTo("prefix value suffix"), "应正确解析命令行参数的环境变量引用");
            }
            #endregion

            #region 测试系统环境变量解析
            {
                Environment.SetEnvironmentVariable(testVar, "env_value");
                var result2 = ("prefix ${Environment." + testVar + "} suffix").Eval(XEnv.Instance);
                Assert.That(result2, Is.EqualTo("prefix env_value suffix"), "应正确解析系统环境变量引用");
            }
            #endregion

            #region 测试内置变量解析
            {
                Assert.That("${Environment.LocalPath}".Eval(XEnv.Instance), Is.EqualTo(XEnv.LocalPath), "${Environment.LocalPath} 解析后应当和 XEnv.LocalPath 相等。");
                Assert.That("${Environment.ProjectPath}".Eval(XEnv.Instance), Is.EqualTo(XEnv.ProjectPath), "${Environment.ProjectPath} 解析后应当和 XEnv.ProjectPath 相等。");
                Assert.That("${Environment.AssetPath}".Eval(XEnv.Instance), Is.EqualTo(XEnv.AssetPath), "${Environment.AssetPath} 解析后应当和 XEnv.AssetPath 相等。");
                Assert.That("${Environment.UserName}".Eval(XEnv.Instance), Is.EqualTo(Environment.UserName), "${Environment.UserName} 解析后应当和 Environment.UserName 相等。");
                Assert.That("${Environment.Platform}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Platform.ToString()), "${Environment.Platform} 解析后应当和 XEnv.Platform 相等。");
                Assert.That("${Environment.App}".Eval(XEnv.Instance), Is.EqualTo(XEnv.App.ToString()), "${Environment.App} 解析后应当和 XEnv.App 相等。");
                Assert.That("${Environment.Mode}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Mode.ToString()), "${Environment.Mode} 解析后应当和 XEnv.Mode 相等。");
                Assert.That("${Environment.Solution}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Solution), "${Environment.Solution} 解析后应当和 XEnv.Solution 相等。");
                Assert.That("${Environment.Project}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Project), "${Environment.Project} 解析后应当和 XEnv.Project 相等。");
                Assert.That("${Environment.Product}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Product), "${Environment.Product} 解析后应当和 XEnv.Product 相等。");
                Assert.That("${Environment.Channel}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Channel), "${Environment.Channel} 解析后应当和 XEnv.Channel 相等。");
                Assert.That("${Environment.Version}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Version), "${Environment.Version} 解析后应当和 XEnv.Version 相等。");
                Assert.That("${Environment.Author}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Author), "${Environment.Author} 解析后应当和 XEnv.Author 相等。");
                Assert.That("${Environment.Secret}".Eval(XEnv.Instance), Is.EqualTo(XEnv.Secret), "${Environment.Secret} 解析后应当和 XEnv.Secret 相等。");
                Assert.That("${Environment.NumCPU}".Eval(XEnv.Instance), Is.EqualTo(Environment.ProcessorCount.ToString()), "${Environment.NumCPU} 解析后应当和 Environment.ProcessorCount 相等。");
            }
            #endregion

            #region 测试参数优先级
            {
                XEnv.ParseArgs(true, $"-{testVar}", "arg_value");
                var result3 = ("${Environment." + testVar + "}").Eval(XEnv.Instance);
                Assert.That(result3, Is.EqualTo("arg_value"), "命令行参数应优先于系统环境变量");
            }
            #endregion

            #region 测试缺失变量处理
            {
                XEnv.ParseArgs(true);
                var result4 = "hello ${Environment.missing}".Eval(XEnv.Instance);
                Assert.That(result4.Contains("(Unknown)"), Is.True, "未定义的环境变量应标记为未知");
            }
            #endregion

            #region 测试嵌套变量处理
            {
                var result5 = "nested ${Environment.outer${Environment.inner}}".Eval(XEnv.Instance);
                Assert.That(result5.Contains("(Nested)"), Is.True, "嵌套的环境变量引用应标记为嵌套");
            }
            #endregion
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }
}
