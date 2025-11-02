// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EFramework.DotNet.Utility;
using NUnit.Framework;

/// <summary>
/// TestXFile 是 XFile 的单元测试。
/// </summary>
public class TestXFile
{
    private string testBasePath;
    private string testFilePath;
    private string testDirectoryPath;
    private string testZipPath;
    private string testUnzipPath;

    [OneTimeSetUp]
    public void Setup()
    {
        testBasePath = XFile.PathJoin(XEnv.LocalPath, "TestXFile-" + XTime.GetMillisecond());
        testDirectoryPath = XFile.PathJoin(testBasePath, "TestDirectory");
        XFile.CreateDirectory(testDirectoryPath);
        testFilePath = XFile.PathJoin(testBasePath, "TestFile.txt");
        testZipPath = XFile.PathJoin(testBasePath, "TestZip");
        testUnzipPath = XFile.PathJoin(testBasePath, "TestUnzip");
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        if (XFile.HasDirectory(testBasePath))
        {
            XFile.DeleteDirectory(testBasePath);
        }
    }

    [Test]
    public void FileSize()
    {
        // Arrange
        var content = "Test content";
        XFile.SaveText(testFilePath, content);

        // Act
        var size = XFile.FileSize(testFilePath);

        // Assert
        Assert.That(size, Is.EqualTo(content.Length), "文件大小应该与内容长度相等");
        Assert.That(XFile.FileSize("nonexistent.txt"), Is.EqualTo(-1), "不存在的文件应返回 -1");
    }

    [Test]
    public void HasFile()
    {
        // Arrange
        XFile.SaveText(testFilePath, "Test content");

        // Assert
        Assert.That(XFile.HasFile(testFilePath), Is.True, "应能检测到已创建的文件");
        Assert.That(XFile.HasFile(XFile.PathJoin(testDirectoryPath, "NonExistingFile.txt")), Is.False, "不存在的文件应返回 false");
    }

    [Test]
    public void OpenText()
    {
        // Arrange
        var content = "Hello, World!";
        XFile.SaveText(testFilePath, content);

        // Act
        var result = XFile.OpenText(testFilePath);

        // Assert
        Assert.That(result, Is.EqualTo(content), "读取的文本内容应与写入的内容完全一致");
    }

    [Test]
    public void OpenFile()
    {
        // Arrange
        var content = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        XFile.SaveFile(testFilePath, content);

        // Act
        var result = XFile.OpenFile(testFilePath);

        // Assert
        Assert.That(result.Length, Is.EqualTo(content.Length), "读取的文件长度应与原始内容长度相等");
        Assert.That(result, Is.EqualTo(content), "读取的文件内容应与原始内容完全一致");
    }

    [Test]
    public void SaveText()
    {
        // Arrange
        var content = "Hello, World!";

        // Act
        var result = XFile.SaveText(testFilePath, content);

        // Assert
        Assert.That(result, Is.True, "保存文本操作应成功完成");
        Assert.That(XFile.HasFile(testFilePath), Is.True, "文件应成功创建");
        Assert.That(XFile.OpenText(testFilePath), Is.EqualTo(content), "保存的内容应与原始内容一致");
    }

    [Test]
    public void DeleteFile()
    {
        // Arrange
        XFile.SaveText(testFilePath, "Test content");

        // Act
        XFile.DeleteFile(testFilePath);

        // Assert
        Assert.That(XFile.HasFile(testFilePath), Is.False, "文件应被成功删除");
    }

    [Test]
    public void CopyFile()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);
        var copyFilePath = XFile.PathJoin(testDirectoryPath, "CopyTestFile.txt");
        XFile.SaveText(testFilePath, "Test content");

        // Act
        XFile.CopyFile(testFilePath, copyFilePath);

        // Assert
        Assert.That(XFile.HasFile(copyFilePath), Is.True, "目标文件应成功创建");
        Assert.That(XFile.OpenText(copyFilePath), Is.EqualTo("Test content"), "复制的文件内容应与源文件一致");
    }

    [Test]
    public void HasDirectory()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);

        // Assert
        Assert.That(XFile.HasDirectory(testDirectoryPath), Is.True, "应能检测到已创建的目录");
        Assert.That(XFile.HasDirectory("NonExistingDirectory"), Is.False, "不存在的目录应返回 false");
    }

    [Test]
    public void DeleteDirectory()
    {
        // Arrange
        XFile.CreateDirectory(testDirectoryPath);
        var subFilePath = XFile.PathJoin(testDirectoryPath, "SubFile.txt");
        XFile.SaveText(subFilePath, "Sub file content");

        // Act
        var result = XFile.DeleteDirectory(testDirectoryPath);

        // Assert
        Assert.That(result, Is.True, "目录删除操作应成功完成");
        Assert.That(XFile.HasDirectory(testDirectoryPath), Is.False, "目录应被成功删除");
    }

    [Test]
    public void CreateDirectory()
    {
        // Act
        XFile.CreateDirectory(testDirectoryPath);

        // Assert
        Assert.That(Directory.Exists(testDirectoryPath), Is.True, "目录应被成功创建");
    }

    [Test]
    public void CopyDirectory()
    {
        // 确保测试目录存在
        XFile.CreateDirectory(testDirectoryPath);

        // 准备源目录结构
        var srcDir = XFile.PathJoin(testDirectoryPath, "src");
        var dstDir = XFile.PathJoin(testDirectoryPath, "dst");
        var subDir = XFile.PathJoin(srcDir, "subdir");
        var excludeFile = XFile.PathJoin(srcDir, "exclude.txt");
        var normalFile = XFile.PathJoin(srcDir, "normal.txt");
        var subFile = XFile.PathJoin(subDir, "subfile.txt");

        // 创建目录结构
        XFile.CreateDirectory(srcDir);
        XFile.CreateDirectory(subDir);

        // 创建测试文件
        XFile.SaveText(excludeFile, "exclude content");
        XFile.SaveText(normalFile, "normal content");
        XFile.SaveText(subFile, "sub content");

        // 执行目录拷贝，排除.txt文件
        XFile.CopyDirectory(srcDir, dstDir, ".txt");

        // 验证结果
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir, "normal.txt")), Is.False, "被排除的文件不应被复制");
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir, "subdir/subfile.txt")), Is.False, "子目录中被排除的文件不应被复制");

        // 清理目标目录
        XFile.DeleteDirectory(dstDir);

        // 测试不带排除的拷贝
        var dstDir2 = XFile.PathJoin(testDirectoryPath, "dst2");
        XFile.CopyDirectory(srcDir, dstDir2);
        Assert.That(XFile.HasDirectory(dstDir2), Is.True, "目标目录应被成功创建");

        // 验证完整拷贝结果
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir2, "normal.txt")), Is.True, "普通文件应被成功复制");
        Assert.That(XFile.HasFile(XFile.PathJoin(dstDir2, "subdir/subfile.txt")), Is.True, "子目录文件应被成功复制");
        Assert.That(XFile.OpenText(XFile.PathJoin(dstDir2, "normal.txt")), Is.EqualTo("normal content"), "复制的文件内容应保持一致");
    }

    [Test]
    public void IsDirectory()
    {
        // 测试以斜杠结尾的路径
        Assert.That(XFile.IsDirectory("path/to/dir/"), Is.True, "以斜杠结尾的路径应被识别为目录");

        // 测试不以斜杠结尾的路径
        Assert.That(XFile.IsDirectory("path/to/file"), Is.False, "不以斜杠结尾的路径应被识别为文件");

        // 测试根目录
        Assert.That(XFile.IsDirectory("/"), Is.True, "根目录路径应被识别为目录");
    }

    [Test]
    public void NormalizePath()
    {
        // 测试基本路径转换
        Assert.That(XFile.NormalizePath("C:\\Users\\..\\TestFile.txt"), Is.EqualTo("C:/TestFile.txt"), "反斜杠应被转换为正斜杠，且相对路径应被解析");

        // 测试特殊前缀
        Assert.That(XFile.NormalizePath("file://path\\to\\file"), Is.EqualTo("file://path/to/file"), "特殊前缀应被保留，路径分隔符应被统一");
        Assert.That(XFile.NormalizePath("jar:file://path\\to\\file"), Is.EqualTo("jar:file://path/to/file"), "JAR文件路径应被正确处理");

        // 测试点和双点
        Assert.That(XFile.NormalizePath("path/./file"), Is.EqualTo("path/file"), "当前目录符号应被正确处理");
        Assert.That(XFile.NormalizePath("path/file/.."), Is.EqualTo("path"), "父目录符号应被正确处理");
    }

    [Test]
    public void PathJoin()
    {
        // 测试基本路径合并
        Assert.That(XFile.PathJoin("path", "to", "file"), Is.EqualTo("path/to/file"), "多个路径段应被正确合并");

        // 测试带斜杠的路径合并
        Assert.That(XFile.PathJoin("path/", "/to/", "/file"), Is.EqualTo("path/to/file"), "重复的路径分隔符应被正确处理");

        // 测试空路径
        Assert.That(XFile.PathJoin(), Is.EqualTo(""), "空参数应返回空字符串");

        // 测试单个路径
        Assert.That(XFile.PathJoin("path"), Is.EqualTo("path"), "单个路径应保持不变");

        // 测试带点和双点的路径
        Assert.That(XFile.PathJoin("path/./file"), Is.EqualTo("path/file"), "当前目录符号应被正确处理");
        Assert.That(XFile.PathJoin("path/../file"), Is.EqualTo("file"), "父目录符号应被正确处理");
    }

    [Test]
    public void Zip()
    {
        // 创建测试目录和文件
        XFile.CreateDirectory(testZipPath);
        var file1Path = XFile.PathJoin(testZipPath, "File1.txt");
        var file2Path = XFile.PathJoin(testZipPath, "File2.txt");
        var zipFilePath = XFile.PathJoin(testBasePath, "TestZip.zip");

        // 写入测试文件
        File.WriteAllText(file1Path, "Content 1");
        File.WriteAllText(file2Path, "Content 2");

        // 执行压缩
        var zipResult = XFile.Zip(testZipPath, zipFilePath);

        // 验证结果
        Assert.That(zipResult, Is.True, "压缩操作应成功完成");
        Assert.That(XFile.HasFile(zipFilePath), Is.True, "压缩文件应被成功创建");
        Assert.That(new FileInfo(zipFilePath).Length, Is.GreaterThan(0), "压缩文件不应为空");
    }

    [Test]
    public void Unzip()
    {
        // 创建源文件和目录
        XFile.CreateDirectory(testZipPath);
        var file1Path = XFile.PathJoin(testZipPath, "File1.txt");
        var file2Path = XFile.PathJoin(testZipPath, "File2.txt");
        var zipFilePath = XFile.PathJoin(testBasePath, "TestUnzip.zip");

        // 写入测试文件
        File.WriteAllText(file1Path, "Content 1");
        File.WriteAllText(file2Path, "Content 2");

        // 创建zip文件
        Assert.That(XFile.Zip(testZipPath, zipFilePath), Is.True, "压缩文件应成功创建");

        // 创建解压目录
        XFile.CreateDirectory(testUnzipPath);

        // 创建TaskCompletionSource来等待异步操作完成
        var tcs = new TaskCompletionSource<bool>();

        // 执行解压
        XFile.Unzip(zipFilePath, testUnzipPath, () =>
        {
            try
            {
                // 验证解压结果
                Assert.That(XFile.HasFile(XFile.PathJoin(testUnzipPath, "File1.txt")), Is.True, "第一个文件应被成功解压");
                Assert.That(XFile.HasFile(XFile.PathJoin(testUnzipPath, "File2.txt")), Is.True, "第二个文件应被成功解压");
                Assert.That(XFile.OpenText(XFile.PathJoin(testUnzipPath, "File1.txt")), Is.EqualTo("Content 1"), "第一个文件的内容应保持一致");
                Assert.That(XFile.OpenText(XFile.PathJoin(testUnzipPath, "File2.txt")), Is.EqualTo("Content 2"), "第二个文件的内容应保持一致");
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        // 等待异步操作完成，设置20秒超时
        var timeoutTask = Task.Delay(20000);
        Task.WaitAny(tcs.Task, timeoutTask);

        if (!tcs.Task.IsCompleted)
        {
            Assert.Fail("解压操作超时（20秒）");
        }
    }

    [Test]
    public void FileMD5()
    {
        // 准备测试文件
        var testFile = XFile.PathJoin(testBasePath, "md5test.txt");
        var content = "Hello, MD5!";
        using (var sw = new StreamWriter(testFile))
        {
            for (int i = 0; i < 1000; i++) sw.WriteLine(content);
        }

        // 计算MD5
        var md5 = XFile.FileMD5(testFile, 4, 32);

        // 验证MD5不为空且长度正确（32个字符）
        Assert.That(md5, Is.Not.Empty, "MD5值不应为空");
        Assert.That(md5.Length, Is.EqualTo(32), "MD5值应为32个字符");

        // 验证相同内容产生相同的MD5
        var testFile2 = XFile.PathJoin(testBasePath, "md5test2.txt");
        using (var sw2 = new StreamWriter(testFile2))
        {
            for (int i = 0; i < 1000; i++) sw2.WriteLine(content);
        }
        var md52 = XFile.FileMD5(testFile2, 4, 32);
        Assert.That(md52, Is.EqualTo(md5), "相同内容应产生相同的MD5值");

        // 验证不同内容产生不同的MD5
        XFile.SaveText(testFile2, "Different content");
        var md5_3 = XFile.FileMD5(testFile2);
        Assert.That(md5_3, Is.Not.EqualTo(md5), "不同内容应产生不同的MD5值");

        // 测试不存在的文件
        var nonExistentMD5 = XFile.FileMD5("nonexistent.txt");
        Assert.That(nonExistentMD5, Is.Empty, "不存在的文件应返回空MD5值");

        // 空文件的MD5值
        var emptyFile = XFile.PathJoin(testBasePath, "empty.txt");
        XFile.SaveText(emptyFile, "");
        var emptyMd5 = XFile.FileMD5(emptyFile);
        Assert.That(emptyMd5, Is.Not.Empty, "空文件的MD5值不应为空");
        Assert.That(emptyMd5.Length, Is.EqualTo(32), "空文件的MD5值应为32个字符");

        // 文件路径为null或空字符串
        Assert.That(XFile.FileMD5(null), Is.Empty, "文件路径为null应返回空MD5值");
        Assert.That(XFile.FileMD5(string.Empty), Is.Empty, "文件路径为空字符串应返回空MD5值");

        // 分段数量为 0 时应计算全文件MD5
        using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var fsMd5 = MD5.Create();
        var fullHash = BitConverter.ToString(fsMd5.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
        Assert.That(XFile.FileMD5(testFile, 0), Is.EqualTo(fullHash), "分段数量为 0 时应计算全文件MD5");
    }
}
