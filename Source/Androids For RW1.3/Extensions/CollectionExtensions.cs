using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATReforged
{
    using System.Collections.Generic;

    static class CollectionExtensions
    {
        public static List<T> FastToList<T>(this ICollection<T> collection)
        {
            var newList = new List<T>(collection.Count);

            newList.AddRange(collection);

            return newList;
        }
    }
}