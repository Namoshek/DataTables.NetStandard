using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Clones the given dictionary deeply by copying all values.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="input">The input dictionary.</param>
        public static IDictionary<TKey, TValue> DeepClone<TKey, TValue>(this IDictionary<TKey, TValue> input)
        {
            return (IDictionary<TKey, TValue>)input.ToDictionary(e => e.Key, e => CloneDynamicObject(e.Value));
        }

        /// <summary>
        /// Clones a dynamic object by serializing it to json and deserializing it afterwards.
        /// </summary>
        /// <param name="dynamic">The dynamic object to clone.</param>
        private static dynamic CloneDynamicObject(dynamic dynamic)
        {
            return JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(dynamic));
        }
    }
}
