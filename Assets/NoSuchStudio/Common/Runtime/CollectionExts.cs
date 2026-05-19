using System.Collections.Generic;
using System.Linq;

namespace NoSuchStudio.Common {
    public static class CollectionExts {

        public enum MergeDuplicateRule {
            Keep,
            Replace,
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue defVal = default(TValue)) {
            TValue retVal;
            bool found = dic.TryGetValue(key, out retVal);
            if (!found) {
                retVal = defVal;
            }
            return retVal;
        }

        public static Dictionary<string, object> ToStringObjectDic(this Dictionary<string, long> dic) {
            var res = dic.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            return res;
        }

        public static Dictionary<string, long> ToStringLongDic(this Dictionary<string, object> dic) {
            var res = dic.Where(kvp => kvp.Value is long).ToDictionary(kvp => kvp.Key, kvp => (long)kvp.Value);
            return res;
        }

        /// <summary>
        /// Returns t
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic1"></param>
        /// <param name="dic2"></param>
        /// <param name="duplicatesRule"></param>
        /// <returns>The number of added entries.</returns>
        public static int MergeWith<TKey, TValue>(this Dictionary<TKey, TValue> dic1, Dictionary<TKey, TValue> dic2, MergeDuplicateRule duplicatesRule = MergeDuplicateRule.Keep) {
            int mergedEntries = 0;
            foreach (KeyValuePair<TKey, TValue> kvp in dic2) {
                if (!dic1.ContainsKey(kvp.Key)
                || duplicatesRule == MergeDuplicateRule.Replace) {
                    dic1[kvp.Key] = kvp.Value;
                    mergedEntries++;
                }
            }
            return mergedEntries;
        }
    }
}