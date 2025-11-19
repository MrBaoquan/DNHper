using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DNHper
{
    public static class UConverter
    {
        public static T ByteArray2Struct<T>(this byte[] InData)
            where T : struct
        {
            T _instance;
            GCHandle handle = GCHandle.Alloc(InData, GCHandleType.Pinned);
            try
            {
                _instance = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return _instance;
        }

        public static byte[] ToBytes(this string _value)
        {
            return System.Text.Encoding.ASCII.GetBytes(_value);
        }

        public static byte[] ToHexBytes(this string _value, char Split = ' ')
        {
            return _value.Split(Split).Select(_hex => Convert.ToByte(_hex, 16)).ToArray();
        }

        public static string ToHexString(this byte[] InData, char Split = ' ')
        {
            var _hexString = BitConverter.ToString(InData);

            _hexString = _hexString.Replace("-", Split.ToString());
            return _hexString;
        }

        public static void BGR2RGB(ref byte[] buffer)
        {
            byte swap;
            for (int i = 0; i < buffer.Length; i = i + 3)
            {
                swap = buffer[i];
                buffer[i] = buffer[i + 2];
                buffer[i + 2] = swap;
            }
        }

        public static string ToChineseNumber(this int _number)
        {
            string[] _chineseNumber = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八" };
            return _chineseNumber[_number];
        }
    }
}
