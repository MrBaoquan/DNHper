using System;
using System.Security.Cryptography;
using System.Text;
namespace DNHper {
    public static class ParserExtension {
        public static float Parse2Float (this string _value, float InDefault = 0.0f) {
            if (_value == string.Empty) {
                return 0f;
            }
            float _result = -1;
            if (float.TryParse (_value, out _result)) {
                return _result;
            }
            return InDefault;
        }

        public static int Parse2Int (this string _value) {
            if (_value == string.Empty) {
                return 0;
            }
            return int.Parse (_value);
        }

        public static bool Parse2Boolean (this string _value) {
            if (_value == string.Empty) return false;
            return Boolean.Parse (_value);
        }

        public static string ToMD5 (this string _value) {
            using (var _cryptoMD5 = MD5.Create ()) {
                var _bytes = Encoding.UTF8.GetBytes (_value);
                var _hash = _cryptoMD5.ComputeHash (_bytes);
                return BitConverter.ToString (_hash).Replace ("-", string.Empty).ToUpper ();
            }
        }

        public static string FormatBytes (this ulong bytes) {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024) {
                dblSByte = bytes / 1024.0;
            }

            return String.Format ("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }
}