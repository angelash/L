using System;
using System.Collections.Generic;

namespace Mogo.Extension
{
    public static class ContainerExtensions
    {
        public static T First<T>(this IEnumerable<T> source, Func<T, bool> condition)
        {
            var en = source.GetEnumerator();
            var result = default(T);
            while (en.MoveNext())
            {
                if (condition(en.Current))
                {
                    result = en.Current;
                    break;
                }
            }
            return result;
        }

        public static KeyValuePair<int, TV> First<TV>(this IEnumerable<KeyValuePair<int, TV>> source)
        {
            if (source != null)
            {
                var en = source.GetEnumerator();
                en.MoveNext();
                return en.Current;
            }
            else
                return new KeyValuePair<int, TV>();
        }

        public static KeyValuePair<int, TV> First<TV>(this IEnumerable<KeyValuePair<int, TV>> source, Func<KeyValuePair<int, TV>, bool> condition)
        {
            if (source != null)
            {
                var en = source.GetEnumerator();
                KeyValuePair<int, TV> result = new KeyValuePair<int, TV>();
                while (en.MoveNext())
                {
                    if (condition(en.Current))
                    {
                        result = en.Current;
                        break;
                    }
                }
                return result;
            }
            else
                return new KeyValuePair<int, TV>();
        }

        public static List<KeyValuePair<int, TV>> Where<TV>(this IEnumerable<KeyValuePair<int, TV>> source, Func<KeyValuePair<int, TV>, bool> condition)
        {
            var result = new List<KeyValuePair<int, TV>>();
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    if (condition(en.Current))
                    {
                        result.Add(en.Current);
                    }
                }
            }
            return result;
        }

        public static List<TV> Where<TV>(this IEnumerable<TV> source, Func<TV, bool> condition)
        {
            var result = new List<TV>();
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    if (condition(en.Current))
                    {
                        result.Add(en.Current);
                    }
                }
            }
            return result;
        }

        public static bool Any<T>(this IEnumerable<KeyValuePair<int, T>> source, Func<KeyValuePair<int, T>, bool> condition)
        {
            var result = false;
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    if (condition(en.Current))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public static List<TV> ToList<TV>(this IEnumerable<TV> source)
        {
            var result = new List<TV>();
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result.Add(en.Current);
                }
            }
            return result;
        }

        public static int Count<T>(this IEnumerable<T> source)
        {
            int result = 0;
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result += 1;
                }
            }
            return result;
        }

        public static Dictionary<TRKey, TRValue> ToDictionary<TKey, TValue, TRKey, TRValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<KeyValuePair<TKey, TValue>, TRKey> conditionKey, Func<KeyValuePair<TKey, TValue>, TRValue> conditionValue)
        {
            var result = new Dictionary<TRKey, TRValue>();

            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result.Add(conditionKey(en.Current), conditionValue(en.Current));
                }
            }

            return result;
        }

        public static List<T> Distinct<T>(this IEnumerable<T> source)
        {
            var result = new List<T>();
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    if (!result.Contains(en.Current))
                    {
                        result.Add(en.Current);
                    }
                }
            }
            return result;
        }

        /*
        public static bool Contains<T>(this List<T> source, T value)
        {
            var result = false;
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    if (value == en.Current)
                    {
                        result = true;
                        break;
                    }  
                }
            }
            return result;
        }*/

        public static List<TResult> Select<TV, TResult>(this IEnumerable<TV> source, Func<TV, TResult> condition)
        {
            var result = new List<TResult>();
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result.Add(condition(en.Current));
                }
            }
            return result;
        }

        public static double Sum<TV>(this IEnumerable<KeyValuePair<int, TV>> source, Func<KeyValuePair<int, TV>, double> condition)
        {
            double result = 0;
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result += condition(en.Current);
                }
            }
            return result;
        }

        public static int Sum(this IEnumerable<int> source)
        {
            int result = 0;
            if (source != null)
            {
                var en = source.GetEnumerator();
                while (en.MoveNext())
                {
                    result += en.Current;
                }
            }
            return result;
        }

        public static KeyValuePair<int, TV> FirstOrDefault<TV>(this IEnumerable<KeyValuePair<int, TV>> source, Func<KeyValuePair<int, TV>, bool> condition)
        {
            if (source != null)
            {
                var en = source.GetEnumerator();
                KeyValuePair<int, TV> result = new KeyValuePair<int, TV>();
                while (en.MoveNext())
                {
                    if (condition(en.Current))
                    {
                        result = en.Current;
                        break;
                    }
                }
                return result;
            }
            else
                return new KeyValuePair<int, TV>();
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, Func<TValue> provider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : provider();
        }
    }
}