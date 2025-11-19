using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DNHper
{
    public class SingletonConfig<T> : Singleton<T>
        where T : class, new()
    {
        [XmlIgnore]
        public string FilePath { get; private set; } = string.Empty;

        public T SetConfig(string _filePath)
        {
            FilePath = _filePath;
#pragma warning disable CS8603 // 可能返回 null 引用。
            return this as T;
#pragma warning restore CS8603 // 可能返回 null 引用。
        }

        public string Load()
        {
            try
            {
                if (File.Exists(FilePath) == false)
                    USerialization.SerializeXML(this, FilePath);

                var loaded = USerialization.DeserializeXML<T>(FilePath);
                if (loaded != null)
                {
                    CopyToSelf(loaded);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void Save()
        {
            USerialization.SerializeXML(this, FilePath);
        }

        private void CopyToSelf(T other)
        {
            foreach (var property in other.GetType().GetProperties())
            {
                if (property.CanWrite)
                    property.SetValue(this, property.GetValue(other));
            }
        }
    }
}
