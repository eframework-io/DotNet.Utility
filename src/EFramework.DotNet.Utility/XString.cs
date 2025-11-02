// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Collections.Generic;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XString 实现了文本处理、数值转换、加密解密和变量求值等功能，是一个高效的字符串工具类。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 文本处理：提供字符串格式化（Format）、缩略（Omit）、简化（Simplify）等处理功能
    /// - 数值转换：支持字节大小（B、KB、MB、GB）、版本号（一到三段式）等数值的字符串转换
    /// - 加密解密：支持 DES 加密算法的字符串和字节数组加密解密，支持自定义密钥
    /// - 变量求值：支持字符串中变量的替换和求值，可通过接口扩展自定义求值规则
    /// 
    /// 使用手册
    /// 
    /// 1. 文本处理
    /// 
    /// 1.1 格式化
    ///     // 单参数格式化
    ///     var result = XString.Format("Hello, {0}!", "World");  // 输出：Hello, World!
    ///     
    ///     // 多参数格式化
    ///     var result = XString.Format("Hello, {0}! You have {1} new messages.", "Alice", 5);
    ///     // 输出：Hello, Alice! You have 5 new messages.
    ///     
    ///     // 重复参数格式化
    ///     var result = XString.Format("Value: {0}, Again: {0}", 42);  // 输出：Value: 42, Again: 42
    ///     
    ///     // 数字格式化
    ///     var result = XString.Format("Number: {0:0.00}", 42);  // 输出：Number: 42.00
    ///     var result = XString.Format("Large: {0:N0}", 1234567);  // 输出：Large: 1,234,567
    /// 
    /// 2. 数值转换
    /// 
    /// 2.1 字节
    ///     var size = XString.ToSize(1024);  // 输出：1 KB
    ///     var size = XString.ToSize(1024 * 1024);  // 输出：1 MB
    ///     var size = XString.ToSize(1500);  // 输出：1.46 KB
    /// 
    /// 2.2 版本号
    ///     // 字符串转数字
    ///     var version = XString.ToVersion("1.2.3");  // 输出：100020003
    ///     var version = XString.ToVersion("1.1");  // 输出：10001
    ///     
    ///     // 数字转字符串
    ///     var ver = XString.FromVersion(100020003);  // 输出：1.2.3
    ///     var ver = XString.FromVersion(10001);  // 输出：1.1
    /// 
    /// 1.2 缩略
    ///     // 基本缩略
    ///     var result = "Hello World".Omit(5);  // 输出：Hello..
    ///     
    ///     // 自定义后缀
    ///     var result = "Hello World".Omit(5, "...");  // 输出：Hello...
    /// 
    /// 1.3 简化
    ///     // 基本简化
    ///     var result = "Hello World".Simplify(7);  // 输出：Hel...ld
    ///     
    ///     // 长文本简化
    ///     var result = "This is a long text".Simplify(10);  // 输出：Thi...ext
    /// 
    /// 3. 加密解密
    /// 
    /// 3.1 字符串
    ///     // 默认加密
    ///     var encrypted = "Hello".Encrypt();
    ///     var decrypted = encrypted.Decrypt();  // 输出：Hello
    ///     
    ///     // 带密钥加密
    ///     var encrypted = "Hello".Encrypt("12345678");  // 密钥必须是 8 字节
    ///     var decrypted = encrypted.Decrypt("12345678");  // 输出：Hello
    /// 
    /// 3.2 字节数组
    ///     var data = Encoding.UTF8.GetBytes("Hello");
    ///     var encrypted = data.Encrypt();
    ///     var decrypted = encrypted.Decrypt();
    /// 
    /// 4. 变量求值
    /// 
    /// 4.1 字典
    ///     var dict = new Dictionary&lt;string, string&gt; { {"name", "World"} };
    ///     var result = "${name}".Eval(dict);  // 输出：World
    ///     
    ///     // 多字典求值
    ///     var dict1 = new Dictionary&lt;string, string&gt; { {"name", "World"} };
    ///     var dict2 = new Dictionary&lt;string, string&gt; { {"greeting", "Hello"} };
    ///     var result = "${greeting} ${name}".Eval(dict1, dict2);  // 输出：Hello World
    /// 
    /// 4.2 求值器
    ///     public class ConfigEvaluator : XString.IEvaluator 
    ///     {
    ///         private Dictionary&lt;string, string&gt; configs;
    ///         
    ///         public string Eval(string input)
    ///         {
    ///             foreach (var config in configs)
    ///             {
    ///                 input = input.Replace($"${{{config.Key}}}", config.Value);
    ///             }
    ///             return input;
    ///         }
    ///     }
    ///     
    ///     // 使用自定义求值器
    ///     var evaluator = new ConfigEvaluator();
    ///     var result = "Hello ${name}".Eval(evaluator);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class XString
    {
        private static readonly StringBuilder cacheBuilder = new();

        /// <summary>
        /// Format 将字符串格式化，使用 StringBuilder 优化性能。
        /// </summary>
        /// <param name="format">格式化字符串，支持标准的格式化占位符 {0}、{1} 等</param>
        /// <param name="args">要格式化的参数数组</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(this string format, params object[] args)
        {
            try
            {
                lock (cacheBuilder)
                {
                    if (cacheBuilder.Length > 0) cacheBuilder.Remove(0, cacheBuilder.Length);
                    if (args != null && args.Length > 0) cacheBuilder.AppendFormat(format, args);
                    else cacheBuilder.Append(format);
                    return cacheBuilder.ToString();
                }
            }
            catch (Exception e)
            {
                XLog.Panic(e, format);
                return format;
            }
        }

        internal static string[] byteSizeLabels = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>
        /// ToSize 将字节数转换为可读的大小字符串。
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化后的大小字符串，如：1.5 KB、2.3 MB、3.1 GB，自动选择合适的单位（B、KB、MB、GB、TB），保留两位小数</returns>
        public static string ToSize(long bytes)
        {
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < byteSizeLabels.Length - 1)
            {
                order++;
                len /= 1024;
            }
            // 不对整数值作特殊处理
            // if (order == 0) return $"{(int)len} {byteSizeLabels[order]}";
            // else 
            return $"{len:0.##} {byteSizeLabels[order]}";
        }

        /// <summary>
        /// ToVersion 将版本号字符串转换为数字表示。
        /// </summary>
        /// <param name="version">版本号字符串，如：1.2.3</param>
        /// <returns>版本号的数字表示，如：100020003，失败返回 -1</returns>
        public static long ToVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return -1;
            else
            {
                var strs = version.Split('.');
                long iversion = 0;
                var large = (strs.Length - 1) * 4;
                for (int i = 0; i < strs.Length; i++)
                {
                    int.TryParse(strs[i], out int singleVersion);
                    if (i == 0) iversion = large == 0 ? singleVersion : singleVersion * (long)Math.Pow(10, large);
                    else if (i == strs.Length - 1) iversion += singleVersion;
                    else iversion += singleVersion * (int)Math.Pow(10, large - i * 4);
                }
                return iversion;
            }
        }

        /// <summary>
        /// FromVersion 将数字表示的版本号转换为字符串。
        /// </summary>
        /// <param name="version">版本号的数字表示</param>
        /// <returns>版本号字符串，如：1.2.3，失败返回空字符串</returns>
        public static string FromVersion(long version)
        {
            var sversion = string.Empty;
            if (version >= 0)
            {
                var str = version.ToString();
                for (int i = str.Length - 1; i >= 0;)
                {
                    int length = (i - 1) >= 0 ? 4 : 1;
                    int from = i - length + 1;
                    int.TryParse(str.Substring(from, length), out int singleVersion);
                    sversion = singleVersion + sversion;
                    if (i > 3) sversion = "." + sversion;
                    i -= 4;
                }
            }
            return sversion;
        }

        /// <summary>
        /// Omit 缩略字符串到指定长度。
        /// </summary>
        /// <param name="src">源字符串</param>
        /// <param name="length">目标长度</param>
        /// <param name="appendix">省略后缀，默认为 ".."</param>
        /// <returns>缩略后的字符串，如果源字符串长度小于目标长度，则返回原字符串</returns>
        public static string Omit(this string src, int length = 7, string appendix = "..")
        {
            if (string.IsNullOrEmpty(src)) return src;
            if (src.Length <= length) return src;
            else
            {
                string str = src[..length];
                return Format("{0}{1}", str, appendix);
            }
        }

        /// <summary>
        /// Simplify 简化文本，保留头尾，中间使用省略号。
        /// </summary>
        /// <param name="ori">源字符串</param>
        /// <param name="length">目标长度</param>
        /// <param name="ellipsis">省略符号，默认为 "..."</param>
        /// <returns>简化后的文本</returns>
        public static string Simplify(this string ori, int length, string ellipsis = "...")
        {
            // 处理空字符串或null
            if (string.IsNullOrEmpty(ori)) return string.Empty;

            // 如果原字符串长度小于等于目标长度，直接返回
            if (ori.Length <= length) return ori;

            // 计算省略号长度
            var ellipsisLength = ellipsis.Length;

            // 确保目标长度足够大，至少能容纳省略号加两个字符
            if (length <= ellipsisLength + 2) return ori[..length];

            // 计算保留的前后部分长度
            var remainingLength = length - ellipsisLength;
            var halfLength = remainingLength / 2;

            // 如果剩余长度为奇数，多给前半部分一个字符
            var frontLength = remainingLength % 2 == 0 ? halfLength : halfLength + 1;
            var backLength = halfLength;

            // 构建简化后的字符串
            var front = ori[..frontLength];
            var back = ori.Substring(ori.Length - backLength, backLength);

            return front + ellipsis + back;
        }

        /// <summary>
        /// MD5 获取字符串的 MD5 值。
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <returns>32 位小写 MD5 值</returns>
        public static string MD5(this string str)
        {
            var bytes = Encoding.Default.GetBytes(str);
            return MD5(bytes);
        }

        /// <summary>
        /// MD5 获取字节数组的 MD5 值。
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>32 位小写 MD5 值</returns>
        public static string MD5(this byte[] bytes)
        {
            var md5 = new MD5CryptoServiceProvider();
            var retVal = md5.ComputeHash(bytes);

            var sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

#if NATIVE_STRING_RGBIV && !UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        internal static extern IntPtr GetRGBIV();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        internal static extern int GetRGBIVLength();

        /// <summary>
        /// RGBIV 是 DES 加密向量。
        /// </summary>
        /// <remarks>
        /// 原生实现示例:
        /// <code>
        /// // 模块说明：该模块用于设置自定义的DES加密向量
        /// // 注意事项：编辑器模式下需编译成DLL，需启用IL2CPP
        /// // 源码文件：Assets/Plugins/Library/XString.cpp
        /// 
        /// #include <cstdint>
        /// 
        /// #ifdef _WIN32
        ///     #define EXPORT_API __declspec(dllexport)
        /// #else
        ///     #define EXPORT_API __attribute__((visibility("default")))
        /// #endif
        /// 
        /// namespace {
        ///     const uint8_t RGBIV[] = { 0x7B, 0x4A, 0xF3, 0x91, 0xE5, 0xD2, 0x8C, 0x6F };
        /// }
        /// 
        /// extern "C" {
        ///     EXPORT_API const uint8_t* GetRGBIV() {
        ///         return RGBIV;
        ///     }
        ///     
        ///     EXPORT_API int GetRGBIVLength() {
        ///         return sizeof(RGBIV);
        ///     }
        /// }
        /// </code>
        /// </remarks>
        private static byte[] RGBIV
        {
            get
            {
                var length = GetRGBIVLength();
                var result = new byte[length];
                var ptr = GetRGBIV();
                System.Runtime.InteropServices.Marshal.Copy(ptr, result, 0, length);
                return result;
            }
        }
#else
        /// <summary>
        /// RGBIV 是 DES 加密向量。
        /// </summary>
        private static readonly byte[] RGBIV = { 0x7B, 0x4A, 0xF3, 0x91, 0xE5, 0xD2, 0x8C, 0x6F };
#endif

        /// <summary>
        /// Encrypt 使用 DES 算法加密字符串。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="key">加密密钥，必须是 8 字节，为空则使用默认密钥</param>
        /// <returns>Base64 编码的加密字符串</returns>
        public static string Encrypt(this string str, string key = "")
        {
            try
            {
                var rgb = string.IsNullOrEmpty(key) ? RGBIV : Encoding.UTF8.GetBytes(key);
                var arr = Encoding.UTF8.GetBytes(str);
                var dcsp = new DESCryptoServiceProvider();
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, dcsp.CreateEncryptor(rgb, RGBIV), CryptoStreamMode.Write);
                cs.Write(arr, 0, arr.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                XLog.Panic(e);
                return str;
            }
        }

        /// <summary>
        /// Decrypt 使用 DES 算法解密字符串。
        /// </summary>
        /// <param name="str">要解密的 Base64 字符串</param>
        /// <param name="key">解密密钥，必须是 8 字节，为空则使用默认密钥</param>
        /// <returns>解密后的原文</returns>
        public static string Decrypt(this string str, string key = "")
        {
            try
            {
                var rgb = string.IsNullOrEmpty(key) ? RGBIV : Encoding.UTF8.GetBytes(key);
                var arr = Convert.FromBase64String(str);
                var dcsp = new DESCryptoServiceProvider();
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, dcsp.CreateDecryptor(rgb, RGBIV), CryptoStreamMode.Write);
                cs.Write(arr, 0, arr.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception e)
            {
                XLog.Panic(e);
                return str;
            }
        }

        /// <summary>
        /// Encrypt 使用 DES 算法加密字节数组。
        /// </summary>
        /// <param name="src">要加密的字节数组</param>
        /// <param name="key">加密密钥，必须是 8 字节，为空则使用默认密钥</param>
        /// <returns>加密后的字节数组</returns>
        public static byte[] Encrypt(this byte[] src, string key = "")
        {
            try
            {
                var rgb = string.IsNullOrEmpty(key) ? RGBIV : Encoding.UTF8.GetBytes(key);
                var dcsp = new DESCryptoServiceProvider();
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, dcsp.CreateEncryptor(rgb, RGBIV), CryptoStreamMode.Write);
                cs.Write(src, 0, src.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception e)
            {
                XLog.Panic(e);
                return null;
            }
        }

        /// <summary>
        /// Decrypt 使用 DES 算法解密字节数组。
        /// </summary>
        /// <param name="src">要解密的字节数组</param>
        /// <param name="key">解密密钥，必须是 8 字节，为空则使用默认密钥</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] Decrypt(this byte[] src, string key = "")
        {
            try
            {
                var rgb = string.IsNullOrEmpty(key) ? RGBIV : Encoding.UTF8.GetBytes(key);
                var dcsp = new DESCryptoServiceProvider();
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, dcsp.CreateDecryptor(rgb, RGBIV), CryptoStreamMode.Write);
                cs.Write(src, 0, src.Length);
                cs.FlushFinalBlock();
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception e)
            {
                XLog.Panic(e);
                return null;
            }
        }

        /// <summary>
        /// IEvaluator 是变量求值接口，用于实现自定义的变量替换逻辑。
        /// 实现此接口的类可以定义自己的变量替换规则，常用于配置系统、模板引擎等场景。
        /// </summary>
        public interface IEvaluator
        {
            /// <summary>
            /// Eval 对输入字符串进行变量求值。
            /// </summary>
            /// <param name="input">包含变量的字符串</param>
            /// <returns>完成变量替换后的字符串</returns>
            string Eval(string input);
        }

        /// <summary>
        /// Eval 使用多个求值器对字符串进行变量替换。
        /// 按顺序使用每个求值器处理字符串，前一个求值器的结果作为下一个求值器的输入。
        /// </summary>
        /// <param name="input">包含变量的字符串</param>
        /// <param name="evaluators">求值器列表</param>
        /// <returns>替换变量后的字符串</returns>
        public static string Eval(this string input, params IEvaluator[] evaluators)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            foreach (var source in evaluators)
            {
                if (source != null) input = source.Eval(input);
            }
            return input;
        }

        /// <summary>
        /// Eval 使用多个字典对字符串进行变量替换。
        /// </summary>
        /// <param name="input">包含变量的字符串</param>
        /// <param name="dictionaries">变量字典列表</param>
        /// <returns>替换变量后的字符串</returns>
        public static string Eval(this string input, params Dictionary<string, string>[] dictionaries)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            foreach (var source in dictionaries)
            {
                if (source != null)
                {
                    foreach (var item in source)
                    {
                        input = input.Replace(item.Key, item.Value);
                    }
                }
            }
            return input;
        }

        /// <summary>
        /// Random 生成随机字符串。
        /// N：32 位数字，无连字符。
        /// D：含连字符的 32 位数字。
        /// B：带大括号、连字符的 32 位数字。
        /// P：带括号、连字符的 32 位数字。
        /// </summary>
        /// <param name="format">GUID 格式化选项，默认为 N 格式（32 位，无连字符）</param>
        /// <returns>随机字符串</returns>
        public static string Random(string format = "N")
        {
            var guid = Guid.NewGuid().ToString(format);
            return guid;
        }
    }
}
