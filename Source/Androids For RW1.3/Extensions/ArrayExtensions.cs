using System;
using System.Linq;

namespace ATReforged
{
    static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] array, T target)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(target))
                {
                    return true;
                }
            }

            return false;
        }

        public static T[] GetSortedArray<T>(this T[] array)
        {
            return array.OrderBy(x => x).ToArray();
        }
    }
}