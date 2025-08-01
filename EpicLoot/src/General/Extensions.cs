using System.Collections.Generic;
using System.Linq;

namespace EpicLoot.General
{
    internal static class Extensions
    {
        /// <summary>
        /// Take any list of Objects and return it with Fischer-Yates shuffle
        /// </summary>
        /// <returns></returns>
        public static List<T> shuffleList<T>(this List<T> inputList)
        {
            T p = default;
            int t = inputList.Count;
            int r = 0;
            int count = inputList.Count;
            for (int i = 0; i < count; i++)
            {
                int tpos = UnityEngine.Random.Range(i, count);
            while (i < t)
            {
                r = UnityEngine.Random.Range(i, tempList.Count);
                p = tempList[i];
                tempList[i] = tempList[tpos];
                tempList[tpos] = p;
            }
            //EpicLoot.Log($"Input list: {string.Join(",", inputList)}");
            //EpicLoot.Log($"Shuffled l: {string.Join(",", tempList)}");
            return tempList;
        }
    }
}