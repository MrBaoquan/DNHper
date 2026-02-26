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
            if (InList == null || InList.Count == 0)
            {
                return "[]";
            }

            return "[" + string.Join(", ", InList) + "]";
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f) =>
            e.SelectMany(c => f(c).Flatten(f)).Concat(e);

        //
        // Summary:
        //     Calls an action on each item before yielding them.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   action:
        //     The action to call for each item.
        public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
                yield return item;
            }
        }

        //
        // Summary:
        //     Perform an action on each item.
        //
        // Parameters:
        //   source:
        //     The source.
        //
        //   action:
        //     The action to perform.
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }

            return source;
        }

        //
        // Summary:
        //     Perform an action on each item.
        //
        // Parameters:
        //   source:
        //     The source.
        //
        //   action:
        //     The action to perform.
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int num = 0;
            foreach (T item in source)
            {
                action(item, num++);
            }

            return source;
        }

        //
        // Summary:
        //     Convert each item in the collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   converter:
        //     Func to convert the items.
        public static IEnumerable<T> Convert<T>(this IEnumerable source, Func<object, T> converter)
        {
            foreach (object item in source)
            {
                yield return converter(item);
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of a collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   prepend:
        //     Func to create the item to prepend.
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, Func<T> prepend)
        {
            yield return prepend();
            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of a collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   prepend:
        //     The item to prepend.
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, T prepend)
        {
            yield return prepend;
            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add a collection to the beginning of another collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   prepend:
        //     The collection to prepend.
        public static IEnumerable<T> PrependWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> prepend
        )
        {
            foreach (T item in prepend)
            {
                yield return item;
            }

            foreach (T item2 in source)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     Func to create the item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> prepend
        )
        {
            if (condition)
            {
                yield return prepend();
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T prepend
        )
        {
            if (condition)
            {
                yield return prepend;
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add a collection to the beginning of another collection, if a condition is met.
        //
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The collection to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> prepend
        )
        {
            if (condition)
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item2 in source)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     Func to create the item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> prepend
        )
        {
            if (condition())
            {
                yield return prepend();
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T prepend
        )
        {
            if (condition())
            {
                yield return prepend;
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add a collection to the beginning of another collection, if a condition is met.
        //
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The collection to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> prepend
        )
        {
            if (condition())
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item2 in source)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     Func to create the item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            Func<T> prepend
        )
        {
            if (condition(source))
            {
                yield return prepend();
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add an item to the beginning of another collection, if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The item to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            T prepend
        )
        {
            if (condition(source))
            {
                yield return prepend;
            }

            foreach (T item in source)
            {
                yield return item;
            }
        }

        //
        // Summary:
        //     Add a collection to the beginning of another collection, if a condition is met.
        //
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   prepend:
        //     The collection to prepend.
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            IEnumerable<T> prepend
        )
        {
            if (condition(source))
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item2 in source)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the end of a collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   append:
        //     Func to create the item to append.
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, Func<T> append)
        {
            foreach (T item in source)
            {
                yield return item;
            }

            yield return append();
        }

        //
        // Summary:
        //     Add an item to the end of a collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   append:
        //     The item to append.
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, T append)
        {
            foreach (T item in source)
            {
                yield return item;
            }

            yield return append;
        }

        //
        // Summary:
        //     Add a collection to the end of another collection.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   append:
        //     The collection to append.
        public static IEnumerable<T> AppendWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            foreach (T item2 in append)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the end of a collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     Func to create the item to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append();
            }
        }

        //
        // Summary:
        //     Add an item to the end of a collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     The item to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append;
            }
        }

        //
        // Summary:
        //     Add a collection to the end of another collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     The collection to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (!condition)
            {
                yield break;
            }

            foreach (T item2 in append)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Add an item to the end of a collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     Func to create the item to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append();
            }
        }

        //
        // Summary:
        //     Add an item to the end of a collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     The item to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append;
            }
        }

        //
        // Summary:
        //     Add a collection to the end of another collection if a condition is met.
        //
        // Parameters:
        //   source:
        //     The collection.
        //
        //   condition:
        //     The condition.
        //
        //   append:
        //     The collection to append.
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> append
        )
        {
            foreach (T item in source)
            {
                yield return item;
            }

            if (!condition())
            {
                yield break;
            }

            foreach (T item2 in append)
            {
                yield return item2;
            }
        }

        //
        // Summary:
        //     Returns and casts only the items of type T.
        //
        // Parameters:
        //   source:
        //     The collection.
        public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
        {
            foreach (object item in source)
            {
                if (item is T)
                {
                    yield return (T)item;
                }
            }
        }

        //
        // Summary:
        //     Adds a collection to a hashset.
        //
        // Parameters:
        //   hashSet:
        //     The hashset.
        //
        //   range:
        //     The collection.
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                hashSet.Add(item);
            }
        }

        //
        // Summary:
        //     Returns true if the list is either null or empty. Otherwise false.
        //
        // Parameters:
        //   list:
        //     The list.
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            if (list != null)
            {
                return list.Count == 0;
            }

            return true;
        }

        //
        // Summary:
        //     Sets all items in the list to the given value.
        //
        // Parameters:
        //   list:
        //     The list.
        //
        //   item:
        //     The value.
        public static void Populate<T>(this IList<T> list, T item)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                list[i] = item;
            }
        }

        //
        // Summary:
        //     Adds the elements of the specified collection to the end of the IList<T>.
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            if (list is List<T>)
            {
                ((List<T>)list).AddRange(collection);
                return;
            }

            foreach (T item in collection)
            {
                list.Add(item);
            }
        }

        //
        // Summary:
        //     Sorts an IList
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort(comparison);
                return;
            }

            List<T> list2 = new List<T>(list);
            list2.Sort(comparison);
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list2[i];
            }
        }

        //
        // Summary:
        //     Sorts an IList
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort();
                return;
            }

            List<T> list2 = new List<T>(list);
            list2.Sort();
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list2[i];
            }
        }

        /// <summary>
        /// 截取字节数组
        /// </summary>
        /// <param name="srcBytes">要截取的字节数组</param>
        /// <param name="startIndex">开始截取位置的索引</param>
        /// <param name="length">要截取的字节长度</param>
        /// <returns>截取后的字节数组</returns>
        public static byte[] SubByte(this byte[] srcBytes, int startIndex, int length)
        {
            System.IO.MemoryStream bufferStream = new System.IO.MemoryStream();
            byte[] returnByte = new byte[] { };
            if (srcBytes == null)
            {
                return returnByte;
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (startIndex < srcBytes.Length)
            {
                if (length < 1 || length > srcBytes.Length - startIndex)
                {
                    length = srcBytes.Length - startIndex;
                }
                bufferStream.Write(srcBytes, startIndex, length);
                returnByte = bufferStream.ToArray();
                bufferStream.SetLength(0);
                bufferStream.Position = 0;
            }
            bufferStream.Close();
            bufferStream.Dispose();
            return returnByte;
        }

        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
    }
}
