// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EFramework.DotNet.Utility
{
    /// <summary>
    /// XObject 提供了一个对象序列化工具集，实现了结构体与字节数组的转换、对象的 JSON 序列化等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 支持结构体序列化：提供结构体与字节数组的双向转换，基于 Marshal 实现
    /// - 支持 JSON 序列化：支持对象与 JSON 的双向转换，包括基础类型、数组、列表和字典
    /// - 提供编解码接口：通过 IEncoder 和 IDecoder 接口支持自定义序列化逻辑
    /// - 灵活序列化控制：使用特性标记可序列化的字段和属性，支持公有和私有成员
    /// 
    /// 使用手册
    /// 1. 结构体序列化
    /// 
    /// 1.1 结构体转字节数组
    ///     // 定义测试结构体
    ///     struct TestStruct
    ///     {
    ///         public int IntTest;
    ///         public bool BoolTest;
    ///     }
    ///     
    ///     // 创建结构体实例
    ///     var testObj = new TestStruct { IntTest = 1, BoolTest = true };
    ///     
    ///     // 序列化为字节数组
    ///     var bytes = XObject.ToByte(testObj);
    /// 
    /// 1.2 字节数组转结构体
    ///     // 反序列化为结构体
    ///     var deserializedObj = XObject.FromByte&lt;TestStruct&gt;(bytes);
    ///     
    ///     // 验证字段值
    ///     Console.WriteLine(deserializedObj.IntTest);    // 输出: 1
    ///     Console.WriteLine(deserializedObj.BoolTest);   // 输出: True
    /// 
    /// 2. JSON 序列化
    /// 
    /// 2.1 对象转 JSON
    ///     // 定义测试类
    ///     class TestClass
    ///     {
    ///         public int Id;
    ///         public string Name;
    ///     }
    ///     
    ///     // 创建对象实例
    ///     var testObj = new TestClass { Id = 1, Name = "Test" };
    ///     
    ///     // 转换为格式化的 JSON
    ///     var jsonPretty = XObject.ToJson(testObj, true);
    ///     // 输出: {
    ///     //     "Id": 1,
    ///     //     "Name": "Test"
    ///     // }
    ///     
    ///     // 转换为压缩的 JSON
    ///     var jsonCompact = XObject.ToJson(testObj, false);
    ///     // 输出: {"Id":1,"Name":"Test"}
    /// 
    /// 2.2 JSON 转对象
    ///     // JSON 字符串
    ///     var json = "{\"Id\":1,\"Name\":\"Test\"}";
    ///     
    ///     // 从字符串解析
    ///     var resultFromString = XObject.FromJson&lt;TestClass&gt;(json);
    ///     
    ///     // 从 JSONNode 解析
    ///     var resultFromNode = XObject.FromJson&lt;TestClass&gt;(JSON.Parse(json));
    /// 
    /// 3. 自定义序列化
    /// 
    /// 3.1 使用编码器接口
    ///     class CustomClass : XObject.Json.IEncoder
    ///     {
    ///         public int Value { get; set; }
    ///     
    ///         public JSONNode Encode()
    ///         {
    ///             var node = new JSONObject();
    ///             node.Add("value", Value);
    ///             return node;
    ///         }
    ///     }
    /// 
    /// 3.2 使用解码器接口
    ///     class CustomClass : XObject.Json.IDecoder
    ///     {
    ///         public int Value { get; set; }
    ///     
    ///         public void Decode(JSONNode json)
    ///         {
    ///             Value = json["value"].AsInt;
    ///         }
    ///     }
    /// 
    /// 3.3 使用序列化特性
    ///     class CustomClass
    ///     {
    ///         // 排除特定字段
    ///         [XObject.Json.Exclude]
    ///         public int ExcludedField;
    ///     
    ///         // 包含私有字段
    ///         [XObject.Json.Include]
    ///         private string includedField;
    ///     
    ///         // 排除特定属性
    ///         [XObject.Json.Exclude]
    ///         public string ExcludedProperty { get; set; }
    ///     }
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XObject
    {
        public partial class Json
        {
            /// <summary>
            /// IEncoder 是 JSON 的编码器接口，实现此接口的类可以自定义 JSON 序列化逻辑。
            /// </summary>
            public interface IEncoder { JSONNode Encode(); }

            /// <summary>
            /// IDecoder 是 JSON 的解码器接口，实现此接口的类可以自定义 JSON 反序列化逻辑。
            /// </summary>
            public interface IDecoder { void Decode(JSONNode json); }

            /// <summary>
            /// ExcludeAttribute 是排除特性，用于标记不需要序列化的字段或属性。
            /// </summary>
            /// <remarks>
            /// 可以指定具体要排除的字段名，如果不指定则排除被标记的整个成员。
            /// </remarks>
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
            public class ExcludeAttribute : Attribute
            {
                /// <summary>
                /// Field 是要排除的字段名。
                /// </summary>
                public string Field;

                /// <summary>
                /// 构造一个排除特性 ExcludeAttribute。
                /// </summary>
                /// <param name="field">要排除的字段名，为空则排除整个成员</param>
                public ExcludeAttribute(string field = "") { Field = field; }
            }

            /// <summary>
            /// IncludeAttribute 是包含特性，用于标记需要序列化的私有字段或属性。
            /// </summary>
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
            public class IncludeAttribute : Attribute { }
        }

        /// <summary>
        /// ToByte 将结构体序列化为字节数组。
        /// </summary>
        /// <remarks>
        /// 使用 Marshal 类将结构体序列化为字节数组，支持所有基础类型字段。
        /// </remarks>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="obj">要序列化的结构体实例</param>
        /// <returns>序列化后的字节数组</returns>
        public static byte[] ToByte<T>(T obj) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            var bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>
        /// FromByte 将字节数组反序列化为结构体。
        /// </summary>
        /// <remarks>
        /// 使用 Marshal 类将字节数组反序列化为结构体，支持所有基础类型字段。
        /// </remarks>
        /// <typeparam name="T">目标结构体类型</typeparam>
        /// <param name="bytes">要反序列化的字节数组</param>
        /// <returns>反序列化后的结构体实例</returns>
        public static T FromByte<T>(byte[] bytes) where T : struct
        {
            object obj = null;
            var size = Marshal.SizeOf(typeof(T));
            var allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, allocIntPtr, size);
                obj = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)obj;
        }

        /// <summary>
        /// FromJson 将 Json 文本反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的反序列化。
        /// </remarks>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <returns>反序列化后的对象实例</returns>
        public static T FromJson<T>(string json) where T : class { return FromJson(json, typeof(T)) as T; }

        /// <summary>
        /// FromJson 将 Json 节点反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的反序列化。
        /// </remarks>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="node">JSON 节点</param>
        /// <returns>反序列化后的对象实例</returns>
        public static T FromJson<T>(JSONNode node) where T : class { return FromJson(node, typeof(T)) as T; }

        /// <summary>
        /// FromJson 将 Json 文本反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的反序列化。
        /// </remarks>
        /// <param name="json">JSON 字符串</param>
        /// <param name="type">目标类型</param>
        /// <returns>反序列化后的对象实例</returns>
        public static object FromJson(string json, Type type)
        {
            if (string.IsNullOrEmpty(json) || type == null) return null;
            var node = JSON.Parse(json);
            return FromJson(node, type);
        }

        /// <summary>
        /// FromJson 将 Json 节点反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的反序列化。
        /// 对于自定义类型，优先使用 IDecoder 接口进行解码，否则通过反射处理字段和属性。
        /// </remarks>
        /// <param name="node">JSON 节点</param>
        /// <param name="type">目标类型</param>
        /// <returns>反序列化后的对象实例</returns>
        public static object FromJson(JSONNode node, Type type)
        {
            if (node == null || type == null) return null;
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                if (type.IsPrimitive)
                {
                    if (type == typeof(int)) return node.AsInt;
                    else if (type == typeof(bool)) return node.AsBool;
                    else if (type == typeof(byte)) return node.AsByte;
                    else if (type == typeof(sbyte)) return node.AsSByte;
                    else if (type == typeof(short)) return node.AsShort;
                    else if (type == typeof(ushort)) return node.AsUShort;
                    else if (type == typeof(long)) return node.AsLong;
                    else if (type == typeof(ulong)) return node.AsULong;
                    else if (type == typeof(float)) return node.AsFloat;
                    else if (type == typeof(double)) return node.AsDouble;
                    else if (type == typeof(decimal)) return node.AsDecimal;
                    else if (type == typeof(char)) return node.AsChar;
                }
                else if (type.IsEnum) return Enum.ToObject(type, node.AsInt);
                else if (type == typeof(string)) return node.Value;

                XLog.Error("XObject.FromJson: unsupport primitive type: {0}", type);
                return null;
            }
            else if (type.IsArray)
            {
                var etype = type.GetElementType();
                var jarr = node.AsArray;
                var narr = Array.CreateInstance(etype, jarr.Count);
                for (int i = 0; i < jarr.Count; i++)
                {
                    narr.SetValue(FromJson(jarr[i], etype), i);
                }
                return narr;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var etype = type.GetGenericArguments()[0];
                var jarr = node.AsArray;
                var narr = Array.CreateInstance(etype, jarr.Count);
                for (int i = 0; i < jarr.Count; i++)
                {
                    narr.SetValue(FromJson(jarr[i], etype), i);
                }
                return Activator.CreateInstance(type, new object[] { narr });
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && type.GetGenericArguments()[0] == typeof(string))
            {
                var ktype = typeof(string);
                var vtype = type.GetGenericArguments()[1];
                var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(ktype, vtype));
                foreach (var kvp in node.Keys)
                {
                    dict[kvp] = FromJson(node[kvp], vtype);
                }
                return dict;
            }
            else
            {
                var obj = Activator.CreateInstance(type);
                if (typeof(Json.IDecoder).IsAssignableFrom(type))
                {
                    (obj as Json.IDecoder).Decode(node);
                }
                else
                {
                    foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (node.HasKey(field.Name))
                        {
                            var sig = true;
                            if (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() != null) sig = false;
                            else if (field.IsPrivate && field.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                            if (sig) field.SetValue(obj, FromJson(node[field.Name], field.FieldType));
                        }
                    }
                    foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (node.HasKey(prop.Name))
                        {
                            var sig = true;
                            if (prop.GetCustomAttribute<Json.ExcludeAttribute>() != null) sig = false;
                            else if (prop.SetMethod.IsPrivate && prop.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                            if (sig) prop.SetValue(obj, FromJson(node[prop.Name], prop.PropertyType));
                        }
                    }
                }
                return obj;
            }
        }

        /// <summary>
        /// FromJson 将 Json 文本反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 将 JSON 数据解析到现有对象实例中，支持自定义类型的反序列化。
        /// </remarks>
        /// <param name="json">JSON 字符串</param>
        /// <param name="obj">目标对象实例</param>
        public static void FromJson(string json, object obj)
        {
            if (string.IsNullOrEmpty(json) || obj == null) return;
            var node = JSON.Parse(json);
            FromJson(node, obj);
        }

        /// <summary>
        /// FromJson 将 Json 节点反序列化为对象。
        /// </summary>
        /// <remarks>
        /// 将 JSON 节点数据解析到现有对象实例中，支持自定义类型的反序列化。
        /// </remarks>
        /// <param name="node">JSON 节点</param>
        /// <param name="obj">目标对象实例</param>
        public static void FromJson(JSONNode node, object obj)
        {
            if (node == null || obj == null) return;
            var type = obj.GetType();
            if (typeof(Json.IDecoder).IsAssignableFrom(type))
            {
                (obj as Json.IDecoder).Decode(node);
            }
            else
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (node.HasKey(field.Name))
                    {
                        var sig = true;
                        if (field.GetCustomAttribute<NonSerializedAttribute>() != null) sig = false;
                        else if (field.IsPrivate && field.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                        if (sig) field.SetValue(obj, FromJson(node[field.Name], field.FieldType));
                    }
                }
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (node.HasKey(prop.Name))
                    {
                        var sig = true;
                        if (prop.GetCustomAttribute<Json.ExcludeAttribute>() != null) sig = false;
                        else if (prop.SetMethod.IsPrivate && prop.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                        if (sig) prop.SetValue(obj, FromJson(node[prop.Name], prop.PropertyType));
                    }
                }
            }
        }

        /// <summary>
        /// ToJson 将对象序列化为 Json 文本。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的序列化。
        /// </remarks>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="pretty">是否格式化输出</param>
        /// <param name="ignores">要忽略的字段列表</param>
        /// <returns>序列化后的 JSON 字符串</returns>
        public static string ToJson(object obj, bool pretty, List<string> ignores = null)
        {
            var node = ToJson(obj, ignores);
            if (node != null) return pretty ? node.ToString(4) : node.ToString();
            return string.Empty;
        }

        /// <summary>
        /// ToJson 将对象序列化为 Json 节点。
        /// </summary>
        /// <remarks>
        /// 支持基础类型、数组、列表、字典和自定义类型的序列化。
        /// 对于自定义类型，优先使用 IEncoder 接口进行编码，否则通过反射处理字段和属性。
        /// </remarks>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="ignores">要忽略的字段列表</param>
        /// <returns>序列化后的 JSON 节点</returns>
        public static JSONNode ToJson(object obj, List<string> ignores = null)
        {
            if (obj == null) return null;
            if (obj is JSONNode) return obj as JSONNode;
            var type = obj.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                if (type.IsPrimitive)
                {
                    if (type == typeof(int)) return (int)obj;
                    else if (type == typeof(bool)) return (bool)obj;
                    else if (type == typeof(byte)) return (byte)obj;
                    else if (type == typeof(sbyte)) return (sbyte)obj;
                    else if (type == typeof(short)) return (short)obj;
                    else if (type == typeof(ushort)) return (ushort)obj;
                    else if (type == typeof(long)) return (long)obj;
                    else if (type == typeof(ulong)) return (ulong)obj;
                    else if (type == typeof(float)) return (float)obj;
                    else if (type == typeof(double)) return (double)obj;
                    else if (type == typeof(decimal)) return (decimal)obj;
                    else if (type == typeof(char)) return (char)obj;
                }
                else if (type.IsEnum) return ((Enum)obj).GetHashCode();
                else if (type == typeof(string)) return obj as string;

                XLog.Error("XObject.ToJson: unsupport primitive type: {0}", type);
                return null;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && type.GetGenericArguments()[0] == typeof(string))
            {
                var jobj = new JSONObject();
                PropertyInfo kmethod = null;
                PropertyInfo vmethod = null;
                foreach (var kvp in (IEnumerable)obj)
                {
                    if (kmethod == null || vmethod == null)
                    {
                        var ktype = kvp.GetType();
                        kmethod = ktype.GetProperty("Key");
                        vmethod = ktype.GetProperty("Value");
                    }
                    var key = (string)kmethod.GetValue(kvp, null);
                    var val = vmethod.GetValue(kvp, null);
                    if (val != null) jobj.Add(key, ToJson(val, ignores));
                }
                return jobj;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var jarr = new JSONArray();
                foreach (var item in (IEnumerable)obj) jarr.Add(ToJson(item, ignores));
                return jarr;
            }
            else
            {
                if (typeof(Json.IEncoder).IsAssignableFrom(type))
                {
                    return (obj as Json.IEncoder).Encode();
                }
                else
                {
                    var tignores = type.GetCustomAttributes<Json.ExcludeAttribute>();
                    foreach (var tignore in tignores)
                    {
                        ignores ??= new List<string>();
                        if (!ignores.Contains(tignore.Field)) ignores.Add(tignore.Field);
                    }
                    var jobj = new JSONObject();
                    foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (ignores != null && ignores.Contains($"{field.DeclaringType.FullName.Replace("+", ".")}.{field.Name}")) continue;

                        var sig = true;
                        if (field.GetCustomAttribute<Json.ExcludeAttribute>() != null) sig = false;
                        else if (field.GetCustomAttribute<NonSerializedAttribute>() != null) sig = false;
                        else if (field.IsPrivate && field.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                        if (sig)
                        {
                            var val = field.GetValue(obj);
                            if (val != null) jobj.Add(field.Name, ToJson(val, ignores));
                        }
                    }
                    foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (ignores != null && ignores.Contains($"{prop.DeclaringType.FullName.Replace("+", ".")}.{prop.Name}")) continue;

                        var sig = true;
                        if (prop.GetCustomAttribute<Json.ExcludeAttribute>() != null) sig = false;
                        else if (prop.GetMethod.IsPrivate && prop.GetCustomAttribute<Json.IncludeAttribute>() == null) sig = false;
                        if (sig)
                        {
                            var val = prop.GetValue(obj);
                            if (val != null) jobj.Add(prop.Name, ToJson(val, ignores));
                        }
                    }
                    return jobj;
                }
            }
        }
    }
}
