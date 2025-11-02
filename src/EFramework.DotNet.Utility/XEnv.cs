// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XEnv 提供了应用程序的环境管理，支持多平台识别、路径管理、命令行参数解析和环境变量求值等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 参数解析：支持多种参数形式和缓存管理
    /// - 环境配置：支持应用类型、运行模式、版本等环境配置
    /// - 变量求值：支持 ${Environment.Key} 格式的环境变量引用和求值
    /// - 路径管理：提供本地路径和资产路径的统一管理
    /// 
    /// 使用手册
    /// 
    /// 1. 平台环境
    ///     // 获取当前运行平台
    ///     var platform = XEnv.Platform;
    ///     if (platform != XEnv.PlatformType.Unknown)
    ///     {
    ///         // 根据平台类型执行相应逻辑
    ///         switch (platform)
    ///         {
    ///             case XEnv.PlatformType.Windows:
    ///                 // Windows 平台特定处理
    ///                 break;
    ///             case XEnv.PlatformType.Linux:
    ///                 // Linux 平台特定处理
    ///                 break;
    ///             case XEnv.PlatformType.macOS:
    ///                 // macOS 平台特定处理
    ///                 break;
    ///         }
    ///     }
    /// 
    /// 2. 应用配置
    /// 
    /// 2.1 应用类型
    ///     // 获取当前应用类型
    ///     var appType = XEnv.App;
    ///     switch (appType)
    ///     {
    ///         case XEnv.AppType.Client:
    ///             // 客户端特定逻辑
    ///             break;
    ///         case XEnv.AppType.Server:
    ///             // 服务端特定逻辑
    ///             break;
    ///     }
    /// 
    /// 2.2 运行模式
    ///     // 获取当前运行模式
    ///     var mode = XEnv.Mode;
    ///     switch (mode)
    ///     {
    ///         case XEnv.AppType.Dev:
    ///             // 开发模式特定逻辑
    ///             break;
    ///         case XEnv.AppType.Test:
    ///             // 测试模式特定逻辑
    ///             break;
    ///         case XEnv.AppType.Staging:
    ///             // 预发模式特定逻辑
    ///             break;
    ///         case XEnv.AppType.Prod:
    ///             // 生产模式特定逻辑
    ///             break;
    ///     }
    /// 
    /// 2.3 获取基础配置
    ///     // 获取解决方案名称
    ///     var solution = XEnv.Solution;
    ///     // 获取项目名称
    ///     var project = XEnv.Project;
    ///     // 获取产品名称
    ///     var product = XEnv.Product;
    ///     // 获取发布渠道
    ///     var channel = XEnv.Channel;
    ///     // 获取版本号
    ///     var version = XEnv.Version;
    ///     // 获取作者名称
    ///     var author = XEnv.Author;
    /// 
    /// 3. 路径管理
    /// 
    /// 3.1 项目路径
    ///     // 获取项目根目录路径
    ///     var projectPath = XEnv.ProjectPath;
    /// 
    /// 3.2 数据目录
    ///     // 获取数据存储目录路径
    ///     var localPath = XEnv.LocalPath;
    /// 
    /// 3.3 资源目录
    ///     // 获取只读资源目录路径
    ///     var assetPath = XEnv.AssetPath;
    /// 
    /// 4. 命令行参数
    ///     // 解析命令行参数
    ///     XEnv.ParseArgs(true, "--config=dev.json", "-debug");
    /// 
    ///     // 获取参数值
    ///     var config = XEnv.GetArg("config"); // 返回 "dev.json"
    ///     var debug = XEnv.GetArg("debug");   // 返回 ""
    /// 
    ///     // 获取所有参数
    ///     var args = XEnv.GetArgs();
    ///     foreach (var pair in args)
    ///     {
    ///         Console.WriteLine($"{pair.Key}={pair.Value}");
    ///     }
    /// 
    /// 5. 环境变量
    /// 
    /// 5.1 解析环境变量
    ///     // 包含环境变量引用的字符串
    ///     var text = "Hello ${Environment.UserName}!";
    ///     // 解析环境变量
    ///     var result = text.Eval(XEnv.Instance);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XEnv
    {
        /// <summary>
        /// PlatformType 是平台类型枚举，定义了支持的运行平台类型。
        /// </summary>
        public enum PlatformType
        {
            /// <summary>Unknown 表示未知或不支持的平台。</summary>
            Unknown,

            /// <summary>Windows 表示 Windows 10/11/Server 等操作系统平台。</summary>
            Windows,

            /// <summary>Linux 表示 Ubuntu/CentOS 等操作系统平台。</summary>
            Linux,

            /// <summary>macOS 表示 macOS 操作系统平台。</summary>
            macOS,

            /// <summary>Android 表示 Android 及其衍生的操作系统平台。</summary>
            Android,

            /// <summary>iOS 表示 iPhone 和 iPad 操作系统平台。</summary>
            iOS,

            /// <summary>Browser 表示浏览器平台，主要指 WebGL。</summary>
            Browser
        }

        /// <summary>PlatformUnknown 是未知平台的字符串标识。</summary>
        public static readonly string PlatformUnknown = PlatformType.Unknown.ToString();

        /// <summary>PlatformWindows 是 Windows 10/11/Server 等操作系统平台的字符串标识。</summary>
        public static readonly string PlatformWindows = PlatformType.Windows.ToString();

        /// <summary>PlatformLinux 是 Ubuntu/CentOS 等操作系统平台的字符串标识。</summary>
        public static readonly string PlatformLinux = PlatformType.Linux.ToString();

        /// <summary>PlatformmacOS 是 macOS 操作系统平台的字符串标识。</summary>
        public static readonly string PlatformmacOS = PlatformType.macOS.ToString();

        /// <summary>PlatformAndroid 是 Android 及其衍生的操作系统平台的字符串标识。</summary>
        public static readonly string PlatformAndroid = PlatformType.Android.ToString();

        /// <summary>PlatformiOS 是 iPhone 和 iPad 操作系统平台的字符串标识。</summary>
        public static readonly string PlatformiOS = PlatformType.iOS.ToString();

        /// <summary>PlatformBrowser 是浏览器平台，主要指 WebGL 的字符串标识。</summary>
        public static readonly string PlatformBrowser = PlatformType.Browser.ToString();

        /// <summary>
        /// XEnv 是编辑器的静态构造函数。
        static XEnv() { Initialize(); }

        /// <summary>
        /// Initialize 是初始化环境配置的方法。
        /// </summary>
        internal static void Initialize()
        {
            try
            {
                var envFile = XFile.PathJoin(ProjectPath, ".env");
                if (XFile.HasFile(envFile))
                {
                    var lines = File.ReadAllLines(envFile);
                    foreach (var rawLine in lines)
                    {
                        var line = rawLine.Trim();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                        var idx = line.IndexOf('=');
                        if (idx < 0) continue;

                        var key = line[..idx].Trim();
                        var value = line[(idx + 1)..].Trim();
                        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;

                        Environment.SetEnvironmentVariable(key, value);
                        XLog.Notice("XEnv.Initialize: set environment variable: {0} = {1} from .env file.", key, value);
                    }
                }
            }
            catch (Exception e) { XLog.Panic(e); }

            if (Platform == PlatformType.Unknown) { } // 预先获取平台类型。
        }

        internal static PlatformType platform = PlatformType.Unknown;

        internal static bool bPlatform = false;

        /// <summary>
        /// Platform 获取当前运行平台类型。
        /// </summary>
        public static PlatformType Platform
        {
            get
            {
                if (!bPlatform)
                {
                    bPlatform = true;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platform = PlatformType.Windows;
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platform = PlatformType.Linux;
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platform = PlatformType.macOS;
                    else platform = PlatformType.Unknown;
                }
                return platform;
            }
        }

        /// <summary>
        /// AppType 是应用类型的枚举，定义应用程序的类型。
        /// </summary>
        public enum AppType
        {
            /// <summary>Unknown 表示未知或未定义的应用类型。</summary>
            Unknown,

            /// <summary>Server 表示服务器类型的应用，用于后端业务。</summary>
            Server,

            /// <summary>Client 表示客户端类型的应用，用于前端业务。</summary>
            Client
        }

        /// <summary>AppUnknown 是未知或未定义的应用类型的字符串标识。</summary>
        public static readonly string AppUnknown = AppType.Unknown.ToString();

        /// <summary>AppServer 是服务器类型的应用的字符串标识。</summary>
        public static readonly string AppServer = AppType.Server.ToString();

        /// <summary>AppClient 是客户端类型的应用的字符串标识。</summary>
        public static readonly string AppClient = AppType.Client.ToString();

        /// <summary>
        /// ModeType 是运行模式的枚举，定义应用程序的运行环境。
        /// </summary>
        public enum ModeType
        {
            /// <summary>Unknown 表示未知或未定义的运行模式。</summary>
            Unknown,

            /// <summary>Dev 表示开发模式，用于本地开发和调试。</summary>
            Dev,

            /// <summary>Test 表示测试模式，用于功能测试和验证。</summary>
            Test,

            /// <summary>Staging 表示预发模式，用于上线前测试。</summary>
            Staging,

            /// <summary>Prod 表示生产模式，用于正式环境。</summary>
            Prod
        }

        /// <summary>ModeUnknown 是未知或未定义的运行模式的字符串标识。</summary>
        public static readonly string ModeUnknown = ModeType.Unknown.ToString();

        /// <summary>ModeDev 是开发模式的字符串标识。</summary>
        public static readonly string ModeDev = ModeType.Dev.ToString();

        /// <summary>ModeTest 是测试模式的字符串标识。</summary>
        public static readonly string ModeTest = ModeType.Test.ToString();

        /// <summary>ModeStaging 是预发模式的字符串标识。</summary>
        public static readonly string ModeStaging = ModeType.Staging.ToString();

        /// <summary>ModeProd 是生产模式的字符串标识。</summary>
        public static readonly string ModeProd = ModeType.Prod.ToString();

        internal static string projectPath;
        /// <summary>
        /// ProjectPath 获取项目根目录路径，这个函数是线程安全的，可以多线程访问。
        /// </summary>
        public static string ProjectPath
        {
            get
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    projectPath = XFile.NormalizePath(Directory.GetCurrentDirectory());
                }
                return projectPath;
            }
        }

        internal static string localPath;
        /// <summary>
        /// LocalPath 获取数据存储目录路径，这个函数是线程不安全的，只能在主线程访问。
        /// </summary>
        public static string LocalPath
        {
            get
            {
                if (string.IsNullOrEmpty(localPath))
                {
                    var cmdPath = GetArg("LocalPath");
                    if (!string.IsNullOrEmpty(cmdPath))
                    {
                        localPath = cmdPath;
                    }

                    if (string.IsNullOrEmpty(localPath))
                    {
                        localPath = XFile.PathJoin(ProjectPath, "Local");
                    }

                    localPath = XFile.NormalizePath(localPath);
                    if (!XFile.HasDirectory(localPath)) XFile.CreateDirectory(localPath);
                }
                return localPath;
            }
        }

        internal static string assetPath;
        /// <summary>
        /// AssetPath 获取只读资源目录路径，这个函数是线程不安全的，只能在主线程访问。
        /// </summary>
        public static string AssetPath
        {
            get
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    var cmdPath = GetArg("AssetPath");
                    if (!string.IsNullOrEmpty(cmdPath))
                    {
                        assetPath = cmdPath;
                    }

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        assetPath = XFile.PathJoin(ProjectPath, "Assets");
                    }
                }
                return assetPath;
            }
        }
    }

    public partial class XEnv
    {
        /// <summary>
        /// Preferences 是环境配置的编辑器，提供环境相关配置项的管理功能。
        /// </summary>
        public class Preferences
        {
            /// <summary>
            /// Preferences 初始化环境配置的默认值。
            /// </summary>
            static Preferences()
            {
                var projectName = Path.GetFileName(ProjectPath);
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "remote get-url origin",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(psi);
                    var output = process.StandardOutput.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(output))
                    {
                        projectName = Path.GetFileNameWithoutExtension(output);
                    }
                }
                catch { }

                var nameParts = projectName.Split('.');
                SolutionDefault = nameParts.Length == 2 ? nameParts[0] : projectName;
                ProjectDefault = nameParts.Length == 2 ? nameParts[1] : projectName;
                ProductDefault = "Unknown";
                VersionDefault = "0.0.0";
                SecretDefault = XString.Random()[..8];
            }

            /// <summary>App 是应用类型的配置键。</summary>
            public const string App = "XEnv/App";

            /// <summary>AppDefault 是应用类型的默认值：客户端。</summary>
            public static readonly string AppDefault = AppType.Client.ToString();

            /// <summary>Mode 是运行模式的配置键。</summary>
            public const string Mode = "XEnv/Mode";

            /// <summary>ModeDefault 是运行模式的默认值：开发模式。</summary>
            public static readonly string ModeDefault = ModeType.Dev.ToString();

            /// <summary>Solution 是解决方案名称的配置键。</summary>
            public const string Solution = "XEnv/Solution";

            /// <summary>SolutionDefault 是解决方案名称的默认值。</summary>
            public static string SolutionDefault { get; private set; }

            /// <summary>Project 是项目名称的配置键。</summary>
            public const string Project = "XEnv/Project";

            /// <summary>ProjectDefault 是项目名称的默认值。</summary>
            public static string ProjectDefault { get; private set; }

            /// <summary>Product 是产品名称的配置键。</summary>
            public const string Product = "XEnv/Product";

            /// <summary>ProductDefault 是产品名称的默认值。</summary>
            public static string ProductDefault { get; private set; }

            /// <summary>Channel 是发布渠道的配置键。</summary>
            public const string Channel = "XEnv/Channel";

            /// <summary>ChannelDefault 是发布渠道的默认值。</summary>
            public const string ChannelDefault = "Default";

            /// <summary>Version 是版本号的配置键。</summary>
            public const string Version = "XEnv/Version";

            /// <summary>VersionDefault 是版本号的默认值。</summary>
            public static string VersionDefault { get; private set; }

            /// <summary>Author 是作者名称的配置键。</summary>
            public const string Author = "XEnv/Author";

            /// <summary>AuthorDefault 是作者名称的默认值：使用系统用户名。</summary>
            public const string AuthorDefault = "${Environment.UserName}";

            /// <summary>Secret 是应用密钥的配置键。</summary>
            public const string Secret = "XEnv/Secret";

            /// <summary>SecretDefault 是应用密钥的默认值：随机生成的8位字符串。</summary>
            public static string SecretDefault { get; private set; }

            /// <summary>Remote 是远程配置地址的配置键。</summary>
            public const string Remote = "XEnv/Remote";

            /// <summary>RemoteDefault 是远程配置地址的默认值：使用环境变量组合的 URL。</summary>
            public const string RemoteDefault = "${Environment.StoragePublic}/Preferences/${Environment.Author}/${Environment.Channel}/${Environment.Platform}/${Environment.Version}.json";
        }

        internal static bool bSolution = false;
        internal static string solution = "";
        /// <summary>
        /// Solution 获取解决方案名称。
        /// </summary>
        public static string Solution
        {
            get
            {
                if (!bSolution)
                {
                    bSolution = true;
                    solution = XPrefs.Asset.GetString(Preferences.Solution, Preferences.SolutionDefault).Eval(Instance);
                }
                return solution;
            }
        }

        internal static bool bProject = false;
        internal static string project = "";
        /// <summary>
        /// Project 获取项目名称。
        /// </summary>
        public static string Project
        {
            get
            {
                if (!bProject)
                {
                    bProject = true;
                    project = XPrefs.Asset.GetString(Preferences.Project, Preferences.ProjectDefault).Eval(Instance);
                }
                return project;
            }
        }

        internal static bool bProduct = false;
        internal static string product = "";
        /// <summary>
        /// Product 获取产品名称。
        /// </summary>
        public static string Product
        {
            get
            {
                if (!bProduct)
                {
                    bProduct = true;
                    product = XPrefs.Asset.GetString(Preferences.Product, Preferences.ProductDefault).Eval(Instance);
                }
                return product;
            }
        }

        internal static bool bChannel = false;
        internal static string channel = "";
        /// <summary>
        /// Channel 获取发布渠道。
        /// </summary>
        public static string Channel
        {
            get
            {
                if (!bChannel)
                {
                    bChannel = true;
                    channel = XPrefs.Asset.GetString(Preferences.Channel, Preferences.ChannelDefault).Eval(Instance);
                }
                return channel;
            }
        }

        internal static bool bVersion = false;
        internal static string version = "";
        /// <summary>
        /// Version 获取版本号。
        /// </summary>
        public static string Version
        {
            get
            {
                if (!bVersion)
                {
                    bVersion = true;
                    version = XPrefs.Asset.GetString(Preferences.Version, Preferences.VersionDefault).Eval(Instance);
                }
                return version;
            }
        }

        internal static bool bAuthor = false;
        internal static string author = "";
        /// <summary>
        /// Author 获取作者名称。
        /// </summary>
        public static string Author
        {
            get
            {
                if (!bAuthor)
                {
                    bAuthor = true;
                    author = XPrefs.Asset.GetString(Preferences.Author, Preferences.AuthorDefault).Eval(Instance);
                }
                return author;
            }
        }

        internal static bool bApp = false;
        internal static AppType app = AppType.Unknown;
        /// <summary>
        /// App 获取应用类型。
        /// </summary>
        public static AppType App
        {
            get
            {
                if (!bApp)
                {
                    bApp = true;
                    _ = Enum.TryParse(XPrefs.Asset.GetString(Preferences.App, Preferences.AppDefault), out app);
                }
                return app;
            }
        }

        internal static bool bMode = false;
        internal static ModeType mode = ModeType.Unknown;
        /// <summary>
        /// Mode 获取运行模式。
        /// </summary>
        public static ModeType Mode
        {
            get
            {
                if (!bMode)
                {
                    bMode = true;
                    _ = Enum.TryParse(XPrefs.Asset.GetString(Preferences.Mode, Preferences.ModeDefault), out mode);
                }
                return mode;
            }
        }

        internal static bool bSecret = false;
        internal static string secret = "";
        /// <summary>
        /// Secret 获取应用密钥。
        /// </summary>
        public static string Secret
        {
            get
            {
                if (!bSecret)
                {
                    bSecret = true;
                    if (!XPrefs.HasKey(Preferences.Secret)) XLog.Warn("XEnv.Secret: secret is not set, use default value.");
                    secret = XPrefs.Asset.GetString(Preferences.Secret, Preferences.SecretDefault).Eval(Instance);
                }
                return secret;
            }
        }

        internal static bool bRemote = false;
        internal static string remote = "";
        /// <summary>
        /// Remote 获取远程配置地址。
        /// </summary>
        public static string Remote
        {
            get
            {
                if (bRemote == false)
                {
                    bRemote = true;
                    remote = XPrefs.Asset.GetString(Preferences.Remote, Preferences.RemoteDefault).Eval(Instance);
                }
                return remote;
            }
        }
    }

    public partial class XEnv
    {
        internal static List<KeyValuePair<string, string>> argsCache;

        internal static readonly object argsCacheLock = new();

        internal static bool argsCacheInitialized;

        /// <summary>
        /// GetArg 获取命令行参数值。
        /// </summary>
        /// <param name="key">参数名(不含--前缀)</param>
        public static string GetArg(string key)
        {
            var args = ParseArgs(false);
            foreach (var pair in args)
            {
                if (pair.Key == key)
                {
                    return pair.Value;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetArgs 获取所有命令行参数。
        /// </summary>
        /// <returns>参数列表，每个元素为一个键值对</returns>
        public static List<KeyValuePair<string, string>> GetArgs() { return ParseArgs(false); }

        /// <summary>
        /// ParseArgs 解析命令行参数并存储在缓存中。
        /// 支持以下格式：
        /// 1. -key 或 --key：无值参数
        /// 2. -key=value 或 --key=value：等号连接的键值对
        /// 3. -key value 或 --key value：空格分隔的键值对
        /// </summary>
        /// <param name="reset">是否重置缓存</param>
        /// <param name="extras">额外的命令行参数数组，将被追加到内置的命令行参数前</param>
        /// <returns>解析后的参数列表</returns>
        public static List<KeyValuePair<string, string>> ParseArgs(bool reset = true, params string[] extras)
        {
            lock (argsCacheLock)
            {
                // 如果需要重置缓存或缓存尚未初始化
                if (reset || argsCache == null || !argsCacheInitialized)
                {
                    argsCache = new List<KeyValuePair<string, string>>();

                    var args = new List<string>();
                    if (extras != null && extras.Length > 0) args.AddRange(extras);
                    args.AddRange(Environment.GetCommandLineArgs());

                    // 解析命令行参数
                    for (int i = 0; i < args.Count; i++)
                    {
                        var arg = args[i];
                        if (string.IsNullOrWhiteSpace(arg) || !arg.StartsWith('-')) continue;

                        // 移除前缀 (支持 - 和 --)
                        var key = arg.StartsWith("--") ? arg[2..] : arg[1..];

                        // 处理 -key=value 或 --key=value 格式
                        var equalIndex = key.IndexOf('=');
                        if (equalIndex != -1)
                        {
                            var value = key[(equalIndex + 1)..];
                            key = key[..equalIndex];
                            argsCache.Add(new KeyValuePair<string, string>(key, value));
                            continue;
                        }

                        // 处理无值参数 -key 或 --key
                        if (i + 1 >= args.Count || args[i + 1].StartsWith('-'))
                        {
                            argsCache.Add(new KeyValuePair<string, string>(key, string.Empty));
                            continue;
                        }

                        // 处理 -key value 或 --key value 格式
                        argsCache.Add(new KeyValuePair<string, string>(key, args[i + 1]));
                        i++; // 跳过下一个参数，因为它是值
                    }

                    // 标记缓存已初始化
                    argsCacheInitialized = true;
                }

                return argsCache;
            }
        }
    }

    public partial class XEnv : XString.IEvaluator
    {
        internal static XEnv instance;

        private static readonly object instanceLock = new();

        /// <summary>
        /// Instance 是环境变量解析器的全局单例，用于处理配置中的环境变量引用。
        /// </summary>
        /// </summary>
        public static XEnv Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        instance ??= new XEnv();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// evalPattern 是环境变量引用的正则表达式模式。
        /// </summary>
        internal static readonly Regex evalPattern = new(@"\$\{Environment\.([^}]+?)\}", RegexOptions.Compiled);

        /// <summary>
        /// Eval 解析包含环境变量引用的字符串。
        /// </summary>
        /// <param name="input">要解析的字符串</param>
        /// <returns>解析后的结果</returns>
        string XString.IEvaluator.Eval(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var visited = new HashSet<string>();

            string replaceFunc(Match match)
            {
                var key = match.Groups[1].Value;

                // 1. 检查嵌套变量
                if (key.Contains("${"))
                {
                    return $"{match.Value}(Nested)";
                }

                // 2. 检查循环引用
                if (!visited.Add(key))
                {
                    return $"${{Environment.{key}}}(Recursive)";
                }

                try
                {
                    // 3. 获取变量值
                    var value = key switch
                    {
                        "LocalPath" => LocalPath,
                        "ProjectPath" => ProjectPath,
                        "AssetPath" => AssetPath,
                        "UserName" => Environment.UserName,
                        "Platform" => Platform.ToString(),
                        "App" => App.ToString(),
                        "Mode" => Mode.ToString(),
                        "Solution" => Solution,
                        "Project" => Project,
                        "Product" => Product,
                        "Channel" => Channel,
                        "Version" => Version,
                        "Author" => Author,
                        "Secret" => Secret,
                        "NumCPU" => Environment.ProcessorCount.ToString(),
                        _ => GetArg(key)
                    };

                    if (string.IsNullOrEmpty(value))
                    {
                        value = Environment.GetEnvironmentVariable(key);
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        // 递归处理嵌套的变量
                        return evalPattern.Replace(value, replaceFunc);
                    }

                    // 4. 处理未知变量
                    return $"${{Environment.{key}}}(Unknown)";
                }
                finally
                {
                    // 清理访问标记
                    visited.Remove(key);
                }
            }

            return evalPattern.Replace(input, replaceFunc);
        }
    }
}