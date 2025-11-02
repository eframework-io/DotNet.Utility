// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XFile 简化了对文件和目录的基本操作，支持路径归一化、解压缩文件、文件校验等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 文件操作：创建、读取、写入、删除和复制文件，支持文本和二进制内容
    /// - 目录操作：创建、复制、删除和遍历目录，支持递归操作和文件过滤
    /// - 路径处理：合并路径、归一化处理、相对路径转换，支持特殊前缀（file://、jar:file://）
    /// - 压缩解压：ZIP 格式文件的压缩和解压，支持异步操作和进度回调
    /// - 文件校验：计算文件的 MD5 值，支持文件完整性验证
    /// 
    /// 使用手册
    /// 1. 文件操作
    /// 
    /// 1.1 读取文件
    /// 
    ///     // 读取文本文件
    ///     var content = XFile.OpenText("config.txt");
    ///     
    ///     // 读取二进制文件
    ///     var data = XFile.OpenFile("data.bin");
    ///     
    ///     // 获取文件大小
    ///     var size = XFile.FileSize("file.dat");
    /// 
    /// 1.2 写入文件
    /// 
    ///     // 写入文本文件
    ///     XFile.SaveText("config.txt", "Hello World");
    ///     
    ///     // 写入二进制文件
    ///     var data = new byte[] { 1, 2, 3 };
    ///     XFile.SaveFile("data.bin", data);
    /// 
    /// 1.3 文件管理
    /// 
    ///     // 检查文件是否存在
    ///     if (XFile.HasFile("file.txt"))
    ///     {
    ///         // 删除文件
    ///         XFile.DeleteFile("file.txt");
    ///     }
    /// 
    ///     // 复制文件
    ///     XFile.CopyFile("source.txt", "target.txt", true);
    /// 
    /// 2. 目录操作
    /// 
    /// 2.1 目录管理
    /// 
    ///     // 创建目录
    ///     XFile.CreateDirectory("data");
    ///     
    ///     // 检查目录是否存在
    ///     if (XFile.HasDirectory("logs"))
    ///     {
    ///         // 删除目录（递归删除）
    ///         XFile.DeleteDirectory("logs", true);
    ///     }
    /// 
    /// 2.2 目录复制
    /// 
    ///     // 复制目录（包含所有文件）
    ///     XFile.CopyDirectory("source", "target");
    ///     
    ///     // 复制目录（排除特定文件）
    ///     XFile.CopyDirectory("source", "target", ".meta", ".tmp");
    /// 
    /// 3. 路径处理
    /// 
    /// 3.1 路径合并
    /// 
    ///     // 合并多段路径
    ///     var path = XFile.PathJoin("root", "sub", "file.txt");
    ///     // 结果: root/sub/file.txt
    /// 
    /// 3.2 路径归一化
    /// 
    ///     // 统一分隔符
    ///     var path = XFile.NormalizePath("root\\sub\\file.txt");
    ///     // 结果: root/sub/file.txt
    /// 
    ///     // 处理特殊路径
    ///     path = XFile.NormalizePath("root/./sub/../file.txt");
    ///     // 结果: root/file.txt
    /// 
    ///     // 处理特殊前缀
    ///     path = XFile.NormalizePath("file://root\\sub\\file.txt");
    ///     // 结果: file://root/sub/file.txt
    /// 
    /// 4. 压缩解压
    /// 
    /// 4.1 压缩文件
    /// 
    ///     // 压缩目录
    ///     XFile.Zip("sourceDir", "target.zip");
    ///     
    ///     // 压缩目录（排除特定文件）
    ///     var exclude = new List&lt;string&gt; { ".meta", ".tmp" };
    ///     XFile.Zip("sourceDir", "target.zip", exclude);
    /// 
    /// 4.2 解压文件
    /// 
    ///     // 解压文件（带进度和回调）
    ///     XFile.Unzip("source.zip", "targetDir",
    ///         onComplete: () => { Console.WriteLine("解压完成"); },
    ///         onError: (error) => { Console.WriteLine($"解压错误: {error}"); },
    ///         onProgress: (progress) => { Console.WriteLine($"解压进度: {progress:P}"); }
    ///     );
    /// 
    /// 5. 文件校验
    /// 
    /// 5.1 计算 MD5
    /// 
    ///     // 获取文件的 MD5 值
    ///     // 支持全文件或分段采样模式
    ///     // 分段模式会将文件大小及多个采样段拼接后进行哈希，可显著减少对大文件的读取时间，同时降低冲突概率
    ///     var md5 = XFile.FileMD5("file.dat", 8, 64 * 1024);
    ///     if (!string.IsNullOrEmpty(md5))
    ///     {
    ///         Console.WriteLine($"文件 MD5: {md5}");
    ///     }
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XFile
    {
        /// <summary>
        /// Separator 是路径分隔符（POSIX风格）。
        /// </summary>
        public static readonly string Separator = "/";

        /// <summary>
        /// FileSize 用于获取文件的大小。
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>文件大小（字节），文件不存在返回 -1</returns>
        /// <remarks>
        /// 支持 Android 平台的 JAR 文件大小获取。
        /// 对于普通文件，使用 FileInfo 获取大小。
        /// </remarks>
        public static long FileSize(string file)
        {
            if (File.Exists(file) == false) return -1;
            try { return new FileInfo(file).Length; }
            catch (Exception e) { XLog.Panic(e, file); return -1; }
        }

        /// <summary>
        /// HasFile 检查文件是否存在。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件存在返回 true，否则返回 false</returns>
        /// <remarks>
        /// 支持 Android 平台的 JAR 文件检查。
        /// 对于普通文件，使用 File.Exists 检查。
        /// </remarks>
        public static bool HasFile(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// OpenText 以 UTF8 编码打开文本文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件内容字符串</returns>
        /// <remarks>
        /// 支持 Android 平台的 JAR 文件读取。
        /// 对于普通文件，使用 OpenFile 读取后转换为字符串。
        /// </remarks>
        public static string OpenText(string path)
        {
            return Encoding.UTF8.GetString(OpenFile(path));
        }

        /// <summary>
        /// OpenFile 打开文件并读取二进制内容。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件内容字节数组</returns>
        /// <remarks>
        /// 使用 FileStream 读取文件内容。
        /// 文件不存在或读取失败返回空字节数组。
        /// </remarks>
        public static byte[] OpenFile(string path)
        {
            var bytes = Array.Empty<byte>();
            try
            {
                if (HasFile(path) == false) return bytes;
                using var file = File.OpenRead(path);
                if (file != null)
                {
                    bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    file.Close();
                    file.Dispose();
                    return bytes;
                }
            }
            catch (Exception e) { XLog.Panic(e, path); }
            return bytes;
        }

        /// <summary>
        /// SaveText 保存文本内容到文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">文本内容</param>
        /// <param name="mode">文件打开模式</param>
        /// <returns>保存成功返回 true，否则返回 false</returns>
        /// <remarks>
        /// 使用 UTF8 编码将文本转换为字节数组保存。
        /// 自动创建目标文件所在的目录。
        /// </remarks>
        public static bool SaveText(string path, string content, FileMode mode = FileMode.CreateNew) { return SaveFile(path, Encoding.UTF8.GetBytes(content), mode); }

        /// <summary>
        /// SaveFile 保存二进制内容到文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="buffer">二进制内容</param>
        /// <param name="mode">文件打开模式</param>
        /// <returns>保存成功返回 true，否则返回 false</returns>
        /// <remarks>
        /// 自动创建目标文件所在的目录。
        /// 如果模式为 CreateNew 且文件已存在，会先删除原文件。
        /// </remarks>
        public static bool SaveFile(string path, byte[] buffer, FileMode mode = FileMode.CreateNew)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || buffer == null) return false;
                string directory = path[..path.IndexOf(Path.GetFileName(path))];
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                if (mode == FileMode.CreateNew) if (File.Exists(path)) File.Delete(path);
                using var file = File.Open(path, mode);
                if (file != null)
                {
                    file.Write(buffer, 0, buffer.Length);
                    file.Close();
                    file.Dispose();
                    return true;
                }
            }
            catch (Exception e) { XLog.Panic(e, path); }
            return false;
        }

        /// <summary>
        /// DeleteFile 删除文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <remarks>
        /// 如果文件不存在，不会抛出异常。
        /// 删除失败会记录异常信息。
        /// </remarks>
        public static void DeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception e) { XLog.Panic(e, path); }
        }

        /// <summary>
        /// CopyFile 复制文件。
        /// </summary>
        /// <param name="src">源文件路径</param>
        /// <param name="dst">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <remarks>
        /// 使用 File.Copy 实现文件复制。
        /// 如果目标文件已存在且 overwrite 为 false，会抛出异常。
        /// </remarks>
        public static void CopyFile(string src, string dst, bool overwrite = true) { File.Copy(src, dst, overwrite); }

        /// <summary>
        /// HasDirectory 检查目录是否存在。
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>目录存在返回 true，否则返回 false</returns>
        public static bool HasDirectory(string path) { return Directory.Exists(path); }

        /// <summary>
        /// DeleteDirectory 删除目录。
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="recursive">是否递归删除子目录和文件</param>
        /// <returns>删除成功返回 true，否则返回 false</returns>
        /// <remarks>
        /// 在编辑器中会同时删除目录对应的 .meta 文件。
        /// 删除失败会记录异常信息。
        /// </remarks>
        public static bool DeleteDirectory(string path, bool recursive = true)
        {
            var result = false;
            try
            {
                if (Directory.Exists(path) == true)
                {
                    Directory.Delete(path, recursive);
                }
                result = true;
            }
            catch (Exception e) { XLog.Panic(e, path); }
            return result;
        }

        /// <summary>
        /// CreateDirectory 创建目录。
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <remarks>
        /// 如果目录已存在，不会抛出异常。
        /// 创建失败会记录异常信息。
        /// </remarks>
        public static void CreateDirectory(string path)
        {
            try { Directory.CreateDirectory(path); }
            catch (Exception e) { XLog.Panic(e, path); }
        }

        /// <summary>
        /// CopyDirectory 复制目录。
        /// </summary>
        /// <param name="src">源目录路径</param>
        /// <param name="dst">目标目录路径</param>
        /// <param name="exclude">要排除的文件扩展名列表</param>
        /// <remarks>
        /// 递归复制目录结构和文件。
        /// 可以通过 exclude 参数排除特定类型的文件。
        /// 复制失败会记录异常信息。
        /// </remarks>
        public static void CopyDirectory(string src, string dst, params string[] exclude)
        {
            try
            {
                var paths = Directory.GetFileSystemEntries(src);
                for (var i = 0; i < paths.Length; i++)
                {
                    var path = NormalizePath(paths[i]);
                    var copy = true;
                    for (var j = 0; j < exclude.Length; j++)
                    {
                        var ext = exclude[j];
                        if (path.EndsWith(ext))
                        {
                            copy = false;
                            break;
                        }
                    }
                    if (copy)
                    {
                        var delta = Path.GetRelativePath(src, path);
                        if (HasDirectory(path))
                        {
                            CopyDirectory(path, PathJoin(dst, delta), exclude);
                        }
                        else
                        {
                            var file = PathJoin(dst, delta);
                            var dir = Path.GetDirectoryName(file);
                            if (HasDirectory(dir) == false) CreateDirectory(dir);
                            File.Copy(path, file, true);
                        }
                    }
                }
            }
            catch (Exception e) { XLog.Panic(e); }
        }

        /// <summary>
        /// IsDirectory 判断路径是否为目录。
        /// </summary>
        /// <param name="path">要判断的路径</param>
        /// <returns>是目录返回 true，否则返回 false</returns>
        /// <remarks>
        /// 通过检查路径是否以斜杠结尾来判断。
        /// </remarks>
        public static bool IsDirectory(string path)
        {
            if (path[^1] == '/') return true;
            return false;
        }

        /// <summary>
        /// NormalizePath 归一化路径。
        /// </summary>
        /// <param name="path">要归一化的路径</param>
        /// <returns>归一化后的路径</returns>
        /// <remarks>
        /// 1. 统一使用正斜杠作为分隔符
        /// 2. 处理 . 和 .. 等相对路径符号
        /// 3. 保留 file:// 和 jar:file:// 等特殊前缀
        /// </remarks>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            var prefix = string.Empty;
            if (path.StartsWith("file://")) prefix = "file://";
            else if (path.StartsWith("jar:file://")) prefix = "jar:file://";
            if (!string.IsNullOrEmpty(prefix)) path = path[prefix.Length..];
            path = path.Replace("\\", Separator);
            var parts = path.Split(Separator);
            var nparts = new List<string>();
            foreach (string part in parts)
            {
                if (part == "." || part == "")
                {
                    if (nparts.Count == 0) nparts.Add(part);
                }
                else if (part == "..")
                {
                    if (nparts.Count > 0) nparts.RemoveAt(nparts.Count - 1);
                }
                else nparts.Add(part);
            }
            var npath = string.Join(Separator, nparts);
            return prefix + npath;
        }

        /// <summary>
        /// PathJoin 合并路径。
        /// </summary>
        /// <param name="path">基础路径</param>
        /// <param name="paths">要合并的路径数组</param>
        /// <returns>合并后的路径</returns>
        /// <remarks>
        /// 1. 自动处理路径分隔符
        /// 2. 合并后进行路径归一化
        /// 3. 空参数返回空字符串
        /// </remarks>
        public static string PathJoin(params string[] paths)
        {
            if (paths.Length == 0) return string.Empty;
            var ret = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                if (!paths[i].StartsWith(Separator) && !ret.EndsWith(Separator))
                {
                    ret += Separator;
                }
                ret += paths[i];
            }
            return NormalizePath(ret);
        }

        /// Zip 将目录压缩为 ZIP 文件。
        /// </summary>
        /// <param name="dir">要压缩的目录</param>
        /// <param name="zip">目标 ZIP 文件路径</param>
        /// <param name="exclude">要排除的文件列表</param>
        /// <returns>压缩成功返回 true，否则返回 false</returns>
        /// <remarks>
        /// 使用 SharpZipLib 实现 ZIP 压缩。
        /// 自动创建目标文件所在的目录。
        /// 可以通过 exclude 参数排除特定文件。
        /// </remarks>
        public static bool Zip(string dir, string zip, List<string> exclude = null)
        {
            bool ret;
            try
            {
                if (dir.EndsWith("/") || dir.EndsWith(@"\")) dir = dir[..^1];
                var zipFileDirectory = Path.GetDirectoryName(zip);
                if (!Directory.Exists(zipFileDirectory))
                {
                    Directory.CreateDirectory(zipFileDirectory);
                }
                Dictionary<string, string> dictionaryList = PrepareFileSystementities(dir);
                using (var zipStream = new ZipOutputStream(File.Create(zip)))
                {
                    foreach (var key in dictionaryList.Keys)
                    {
                        if (File.Exists(key) && (exclude == null || (exclude != null && !exclude.Contains(Path.GetExtension(key)))))
                        {
                            FileInfo fileItem = new FileInfo(key);
                            using FileStream readStream = fileItem.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            string entryName = dictionaryList[key].Substring(dir.Length + 1);
                            entryName = NormalizePath(entryName);
                            ZipEntry zipEntry = new ZipEntry(entryName);
                            zipEntry.DateTime = fileItem.LastWriteTime;
                            zipEntry.Size = readStream.Length;
                            zipStream.PutNextEntry(zipEntry);
                            int readLength = 0;
                            byte[] buffer = new byte[1024];
                            do
                            {
                                readLength = readStream.Read(buffer, 0, 1024);
                                zipStream.Write(buffer, 0, readLength);
                            } while (readLength == 1024);
                            readStream.Close();
                        }
                    }
                    zipStream.Flush();
                    zipStream.Finish();
                    zipStream.Close();
                }
                ret = true;
            }
            catch (Exception e) { throw e; }
            return ret;
        }

        private static Dictionary<string, string> PrepareFileSystementities(string directory)
        {
            var fileEntityDictionary = new Dictionary<string, string>();
            var path = directory;
            if (path.EndsWith(@"\"))
            {
                path = path[..path.LastIndexOf(@"\")];
            }
            var parentDirectoryPath = Path.GetDirectoryName(path) + @"\";
            if (parentDirectoryPath.EndsWith(@":\\"))
            {
                parentDirectoryPath = parentDirectoryPath.Replace(@"\\", @"\");
            }
            var subDictionary = GetAllFileSystemEntities(path, parentDirectoryPath);
            foreach (string key in subDictionary.Keys)
            {
                if (!fileEntityDictionary.ContainsKey(key))
                {
                    fileEntityDictionary.Add(key, subDictionary[key]);
                }
            }
            return fileEntityDictionary;
        }

        private static Dictionary<string, string> GetAllFileSystemEntities(string source, string topDirectory)
        {
            var entitiesDictionary = new Dictionary<string, string>
            {
                { source, source.Replace(topDirectory, "") }
            };
            if (Directory.Exists(source))
            {
                var directories = Directory.GetDirectories(source, "*.*", SearchOption.AllDirectories);
                foreach (string directory in directories)
                {
                    entitiesDictionary.Add(directory, directory.Replace(topDirectory, ""));
                }
                var files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    entitiesDictionary.Add(file, file.Replace(topDirectory, ""));
                }
            }
            return entitiesDictionary;
        }

        /// <summary>
        /// Unzip 解压 ZIP 文件。
        /// </summary>
        /// <param name="src">ZIP 文件路径</param>
        /// <param name="to">解压目标目录</param>
        /// <param name="onComplete">解压完成回调</param>
        /// <param name="onError">解压错误回调</param>
        /// <param name="onProgress">解压进度回调</param>
        /// <remarks>
        /// Android 平台使用原生解压实现。
        /// 其他平台使用 SharpZipLib 实现。
        /// 自动创建目标目录。
        /// </remarks>
        public static void Unzip(string src, string to, Action onComplete = null, Action<string> onError = null, Action<float> onProgress = null)
        {
            if (!HasDirectory(to)) CreateDirectory(to);
            _ = XLoom.RunAsync(() =>
            {
                try
                {
                    var fis = File.OpenRead(src);
                    var zis = new ZipInputStream(fis);
                    ZipEntry zip;
                    var buffer = new byte[2048];
                    while ((zip = zis.GetNextEntry()) != null)
                    {
                        if (!string.IsNullOrEmpty(zip.Name))
                        {
                            string filePath = to;
                            filePath += "/" + zip.Name;
                            if (IsDirectory(filePath))
                            {
                                if (!HasDirectory(filePath)) CreateDirectory(filePath);
                            }
                            else
                            {
                                if (HasFile(filePath)) DeleteFile(filePath);
                                if (!HasDirectory(Path.GetDirectoryName(filePath)) == false) CreateDirectory(Path.GetDirectoryName(filePath));
                                var fs = File.Create(filePath);
                                int size = buffer.Length;
                                while (true)
                                {
                                    size = zis.Read(buffer, 0, buffer.Length);
                                    if (size > 0) fs.Write(buffer, 0, size);
                                    else break;
                                }

                                fs.Close();
                            }
                        }

                        onProgress?.Invoke(zis.Position * 1f / fis.Length);
                    }

                    fis.Close();
                    zis.Close();
                    onProgress?.Invoke(1);
                    onComplete?.Invoke();
                }
                catch (Exception e)
                {
                    XLog.Panic(e);
                    onError?.Invoke(e.Message);
                }
            });
        }

        /// <summary>
        /// FileMD5 计算文件的 MD5 指纹值，支持全文件或分段采样模式。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="segmentCount">
        /// 分段数量：
        /// 小于等于 0 表示对整个文件计算 MD5；
        /// 大于 0 表示按等间距采样 segmentCount 段，每段长度由 segmentSize 指定。
        /// </param>
        /// <param name="segmentSize">每段采样的最大字节数（仅分段模式有效）</param>
        /// <returns>
        /// 32 位小写 MD5 字符串，若文件不存在或出错返回空字符串。
        /// </returns>
        /// <remarks>
        /// 使用哈希算法计算文件指纹，分段模式会将文件大小及多个采样段拼接后进行哈希，可显著减少对大文件的读取时间，同时降低冲突概率。
        /// </remarks>
        public static string FileMD5(string path, int segmentCount = 8, int segmentSize = 64 * 1024)
        {
            if (!File.Exists(path)) return string.Empty;

            try
            {
                var fileInfo = new FileInfo(path);
                long fileSize = fileInfo.Length;

                using var md5 = MD5.Create();

                if (segmentCount <= 0) // 全文件哈希
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var fullHash = md5.ComputeHash(fs);
                    return BitConverter.ToString(fullHash).Replace("-", "").ToLowerInvariant();
                }
                else // 分段哈希
                {
                    using var ms = new MemoryStream();

                    // 写入文件大小（8字节，Little Endian）
                    ms.Write(BitConverter.GetBytes(fileSize));

                    if (fileSize == 0)
                    {
                        ms.Position = 0;
                        var hashEmpty = md5.ComputeHash(ms);
                        return BitConverter.ToString(hashEmpty).Replace("-", "").ToLowerInvariant();
                    }

                    using var fsSample = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var buffer = new byte[segmentSize];

                    for (int i = 0; i < segmentCount; i++)
                    {
                        long offset = fileSize * i / segmentCount;
                        long remaining = fileSize - offset;
                        if (remaining <= 0) break;

                        var readSize = (int)Math.Min(segmentSize, remaining > int.MaxValue ? int.MaxValue : remaining);
                        if (readSize <= 0) break;

                        fsSample.Seek(offset, SeekOrigin.Begin);

                        var totalRead = 0;
                        while (totalRead < readSize)
                        {
                            var read = fsSample.Read(buffer, totalRead, readSize - totalRead);
                            if (read == 0) break;
                            totalRead += read;
                        }

                        ms.Write(buffer, 0, totalRead);
                    }

                    ms.Position = 0;
                    var finalHash = md5.ComputeHash(ms);
                    return BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception e)
            {
                XLog.Panic(e, path);
                return string.Empty;
            }
        }
    }
}
