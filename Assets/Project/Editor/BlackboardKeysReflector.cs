using System.Collections.Generic;
using System.Reflection;

namespace GOAP.Editor
{
    /// <summary>
    /// Reads string constants from BlackboardKeys at editor time.
    /// Used to populate key dropdowns in asset inspectors, removing dependency on manual typing.
    /// </summary>
    public static class BlackboardKeysReflector
    {
        // -----Private properties-----
        private static string[] _cachedKeys;

        // -----Public methods-----

        /// <summary>
        /// Caches and returns all string constants defined in BlackboardKeys.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllKeys()
        {
            if (_cachedKeys != null)
                return _cachedKeys;
            
            var keys = new List<string>();

            FieldInfo[] fields = typeof(BlackboardKeys).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    string value = field.GetValue(null) as string;
                    if (value != null)
                        keys.Add(value);
                }
            }

            _cachedKeys = keys.ToArray();
            return _cachedKeys;
        }

        /// <summary>
        /// Returns the index of the given key in the keys array or 0 if not found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetIndex(string key)
        {
            string[] keys = GetAllKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == key)
                    return i;
            }

            return 0;
        }

        public static void ClearCache() => _cachedKeys = null;
    }
}

