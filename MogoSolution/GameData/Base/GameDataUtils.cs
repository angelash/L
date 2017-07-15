using System;
using System.Collections.Generic;
using System.Linq;

namespace Mogo.Util
{
    public static class GameDataUtils
    {
        //OrderByKey的扩展方法在ios上实现不了，所以去掉了
        public static IEnumerable<int> OrderByValue(this IEnumerable<int> list)
        {
            var myList = list.ToList();
            myList.Sort((firstPair, nextPair) => { return firstPair == nextPair ? 0 : (firstPair < nextPair ? -1 : 1); });
            return myList;
        }

        public static List<KeyValuePair<TKey, TValue>> OrderByValue<TKey, TValue>(this Dictionary<TKey, TValue> dic,
            Func<TValue, int> comparer)
        {
            var myList = dic.ToList();

            myList.Sort((firstPair, nextPair) =>
            {
                var firstPairValue = comparer(firstPair.Value);
                var nextPairValue = comparer(nextPair.Value);
                return firstPairValue == nextPairValue ? 0 : (firstPairValue < nextPairValue ? -1 : 1);
            }
                );
            return myList;
        }

        public static List<T> ToTList<T>(this object list)
        {
            var l = list as List<object>;
            if (l != null)
                return l.ToTList<T>();
            return new List<T>();
        }

        public static List<T> ToTList<T>(this List<object> list)
        {
            var result = new List<T>();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                result.Add((T) item);
            }
            return result;
        }

        public static Dictionary<TKey, TValue> ToTDictionary<TKey, TValue>(this object dic)
        {
            var d = dic as Dictionary<object, object>;
            if (d != null)
                return d.ToTDictionary<TKey, TValue>();
            return new Dictionary<TKey, TValue>();
        }

        public static Dictionary<TKey, TValue> ToTDictionary<TKey, TValue>(this Dictionary<object, object> dic)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var o in dic)
            {
                result[(TKey) o.Key] = (TValue) o.Value;
            }
            return result;
        }
    }

    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            var result = x.CompareTo(y);

            if (result == 0)
                return 1;
            return result;
        }
    }

    public class DuplicateKeyDescendingComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            var result = y.CompareTo(x);

            if (result == 0)
                return 1;
            return result;
        }
    }
}