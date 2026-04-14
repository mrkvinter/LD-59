using System.Collections.Generic;
using System.Linq;

namespace Game.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IReadOnlyCollection<T> collection, T item)
        {
            var index = 0;
            foreach (var i in collection)
            {
                if (i.Equals(item))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
        
        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var randomIndex = UnityEngine.Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
        
        public static T RandomOrDefault<T>(this ICollection<T> list)
        {
            return list.Count == 0 ? default : list.ElementAt(UnityEngine.Random.Range(0, list.Count));
        }
    }
}