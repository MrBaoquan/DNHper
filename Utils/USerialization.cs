using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MessagePack;

#pragma warning disable CS8600
#pragma warning disable CS8603

namespace DNHper
{
    /// <summary>
    /// 提供 XML 和 MessagePack 序列化/反序列化功能
    /// </summary>
    public static class USerialization
    {
        #region XML 序列化

        /// <summary>
        /// 获取默认的 XML 写入设置
        /// </summary>
        private static XmlWriterSettings GetDefaultWriterSettings()
        {
            return new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ", // 两个空格缩进
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = true, // 默认省略 XML 声明
                NewLineOnAttributes = false,
                CloseOutput = true
            };
        }

        /// <summary>
        /// 获取默认的 XML 读取设置
        /// </summary>
        private static XmlReaderSettings GetDefaultReaderSettings()
        {
            return new XmlReaderSettings
            {
                CloseInput = true,
                IgnoreWhitespace = false,
                IgnoreComments = true
            };
        }

        /// <summary>
        /// 将对象序列化为 XML 文件
        /// </summary>
        /// <param name="item">要序列化的对象</param>
        /// <param name="path">输出文件路径</param>
        /// <param name="settings">XML 写入设置，为 null 时使用默认设置</param>
        public static void SerializeXML(object item, string path, XmlWriterSettings settings = null)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            XmlSerializer serializer = new XmlSerializer(item.GetType());
            settings = settings ?? GetDefaultWriterSettings();

            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var writer = XmlWriter.Create(path, settings))
                {
                    serializer.Serialize(writer, item, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                    // serializer.Serialize(writer, item);
                }
            }
            catch (Exception e)
            {
                NLogger.Error($"SerializeXML failed for path {path}: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 从 XML 文件反序列化对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="path">XML 文件路径</param>
        /// <param name="settings">XML 读取设置，为 null 时使用默认设置</param>
        /// <returns>反序列化的对象，文件不存在时返回默认值</returns>
        public static T DeserializeXML<T>(string path, XmlReaderSettings settings = null)
            where T : new()
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                return default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            settings = settings ?? GetDefaultReaderSettings();

            try
            {
                using (var reader = XmlReader.Create(path, settings))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                NLogger.Error($"DeserializeXML failed for path {path}: {e.Message}");
                throw;
            }
        }

        #endregion

        #region XML 序列化便捷方法

        /// <summary>
        /// 序列化为紧凑的 XML（无缩进，无声明）
        /// </summary>
        public static void SerializeXMLCompact(object item, string path)
        {
            var settings = new XmlWriterSettings
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None,
                OmitXmlDeclaration = true,
                CloseOutput = true
            };
            SerializeXML(item, path, settings);
        }

        /// <summary>
        /// 序列化为格式化的 XML（可读性优先）
        /// </summary>
        public static void SerializeXMLFormatted(object item, string path, string indentChars = "    ")
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = indentChars,
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false, // 保留声明以便识别编码
                Encoding = System.Text.Encoding.UTF8,
                CloseOutput = true
            };
            SerializeXML(item, path, settings);
        }

        /// <summary>
        /// 序列化为带换行属性的 XML（便于查看多属性元素）
        /// </summary>
        public static void SerializeXMLWithAttributeLines(object item, string path)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                NewLineOnAttributes = true, // 属性换行
                OmitXmlDeclaration = true,
                CloseOutput = true
            };
            SerializeXML(item, path, settings);
        }

        #endregion

        #region MessagePack 序列化

        /// <summary>
        /// 使用 MessagePack 序列化对象到文件
        /// </summary>
        /// <param name="target">要序列化的对象</param>
        /// <param name="inPath">输出文件路径</param>
        public static void SerializeObject(object target, string inPath)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(inPath))
                throw new ArgumentException("Path cannot be null or empty", nameof(inPath));

            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(inPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                byte[] bytes = MessagePackSerializer.Serialize(target);
                File.WriteAllBytes(inPath, bytes);
            }
            catch (Exception e)
            {
                NLogger.Error($"SerializeObject failed for path {inPath}: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 使用 MessagePack 从文件反序列化对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="inPath">输入文件路径</param>
        /// <returns>反序列化的对象，失败时返回默认值</returns>
        public static T DeserializeObject<T>(string inPath)
            where T : class
        {
            if (string.IsNullOrEmpty(inPath))
                throw new ArgumentException("Path cannot be null or empty", nameof(inPath));

            if (!File.Exists(inPath))
            {
                NLogger.Warn($"File {inPath} not exists.");
                return default;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(inPath);
                return MessagePackSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                NLogger.Error($"DeserializeObject failed for path {inPath}: {e.Message}");
                return default;
            }
        }

        #endregion
    }
}
