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
            StreamWriter writer = new StreamWriter(path);

            serializer.Serialize(
                writer.BaseStream,
                item,
                new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty })
            );

            writer.Close();
        }

        public static T DeserializeXML<T>(string path)
            where T : new()
        {
            if (!File.Exists(path))
                return default(T);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(path);
            try
            {
                T deserialized = (T)serializer.Deserialize(reader.BaseStream);
                reader.Close();
                return deserialized;
            }
            catch (System.Exception)
            {
                reader.Close();
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
                NLogger.Error(e.Message);
            }
        }

        public static T DeserializeObject<T>(string inPath)
            where T : class
        {
            if (!File.Exists(inPath))
            {
                NLogger.Warn("File {0} not exists.", inPath);
                return default;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(inPath);
                return MessagePackSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                NLogger.Error(e.Message);
                return default;
            }
        }
    }
}
