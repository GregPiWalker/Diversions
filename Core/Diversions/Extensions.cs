using System;
using System.Collections.Generic;

namespace Diversions
{
    public static class Extensions
    {
        public static TState ToEnum<TState>(this string strValue)
        {
            return (TState)Enum.Parse(typeof(TState), strValue);
        }

        public static bool IsEnum<TState>(this string strValue)
        {
            try
            {
                Enum.Parse(typeof(TState), strValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<KeyValuePair<Type, object>> AddKey(this List<KeyValuePair<Type, object>> list, Type key)
        {
            list.Add(new KeyValuePair<Type, object>(key, null));
            return list;
        }

        public static List<KeyValuePair<Type, object>> AddValue(this List<KeyValuePair<Type, object>> list, object val)
        {
            list.Add(new KeyValuePair<Type, object>(val.GetType(), val));
            return list;
        }
    }
}
