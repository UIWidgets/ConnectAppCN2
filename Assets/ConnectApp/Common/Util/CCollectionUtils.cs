using System;
using System.Collections.Generic;

namespace ConnectApp.Common.Util {
    public static class CCollectionUtils {
        public static bool isNullOrEmpty<T>(this ICollection<T> it) {
            return it == null || it.Count == 0;
        }

        public static bool isNotNullAndEmpty<T>(this ICollection<T> it) {
            return it != null && it.Count > 0;
        }

        public static bool isNullOrEmpty<T>(this Queue<T> it) {
            return it == null || it.Count == 0;
        }

        public static bool isNotNullAndEmpty<T>(this Queue<T> it) {
            return it != null && it.Count > 0;
        }

        public static bool isNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it) {
            return it == null || it.Count == 0;
        }

        public static bool isNotNullAndEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it) {
            return it != null && it.Count > 0;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue) {
            return dictionary.TryGetValue(key: key, out var value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> defaultValueProvider) {
            return dictionary.TryGetValue(key: key, out var value) ? value : defaultValueProvider();
        }
    }
}