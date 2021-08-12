using System;
using System.Security.Cryptography;
using System.Text;
namespace DNHper {
    public static class ParserExtension {
        public static float Parse2Float (this string _value) {
            if (_value == string.Empty) {
                return 0f;
            }
            return float.Parse (_value);
        }

        public static int Parse2Int (this string _value) {
            if (_value == string.Empty) {
                return 0;
            }
            return int.Parse (_value);
        }

        public static byte[] ToBytes (this string _value) {
            return System.Text.Encoding.ASCII.GetBytes (_value);
        }

        public static string ToMD5 (this string _value) {
            using (var _cryptoMD5 = MD5.Create ()) {
                var _bytes = Encoding.UTF8.GetBytes (_value);
                var _hash = _cryptoMD5.ComputeHash (_bytes);
                return BitConverter.ToString (_hash).Replace ("-", string.Empty).ToUpper ();
            }
        }
    }
}