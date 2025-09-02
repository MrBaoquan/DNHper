using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using MessagePack;

namespace DNHper
{
    public static class USerialization
    {
        public static void SerializeXML(object item, string path)
        {
            XmlSerializer serializer = new XmlSerializer(item.GetType());
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, item, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                }
            }
            catch (Exception e)
            {
                NLogger.Error($"SerializeXML failed for path {path}: {e.Message}");
                throw;
            }
        }

        public static T DeserializeXML<T>(string path)
            where T : new()
        {
            if (!File.Exists(path))
                return default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            try
            {
                using (var reader = new StreamReader(path))
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

        public static void SerializeObject(object target, string inPath)
        {
            try
            {
                byte[] bytes = MessagePackSerializer.Serialize(target);
                File.WriteAllBytes(inPath, bytes);
            }
            catch (Exception e)
            {
                NLogger.Error($"SerializeObject failed for path {inPath}: {e.Message}");
            }
        }

        public static T DeserializeObject<T>(string inPath)
            where T : class
        {
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
    }
}
