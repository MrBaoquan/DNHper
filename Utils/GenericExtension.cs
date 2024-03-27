using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DNHper
{
    public static class GenericExtension
    {
        public static List<T> Clone<T>(this List<T> InList)
        {
            T[] _newArr = new T[InList.Count];
            InList.CopyTo(_newArr);
            return _newArr.ToList();
        }

        public static List<T> Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            List<T> shuffledList = new List<T>(list);
            int n = shuffledList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = shuffledList[k];
                shuffledList[k] = shuffledList[n];
                shuffledList[n] = value;
            }
            return shuffledList;
        }

        public static string ToLogString<T>(this List<T> InList)
        {
            if (InList.Count <= 0)
            {
                return "[]";
            }
            string _str = "[";
            InList.ForEach(_ =>
            {
                _str += (_.ToString() + ", ");
            });
            _str = _str.Substring(0, _str.Length - 2);
            return _str + "]";
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f) =>
            e.SelectMany(c => f(c).Flatten(f)).Concat(e);
    }
}
