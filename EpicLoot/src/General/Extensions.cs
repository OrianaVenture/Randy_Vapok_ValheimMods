using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLoot.src.General
{
    internal static class Extensions
    {
        /// <summary>
        /// Fisher-Yates style list sort for string lists.
        /// </summary>
        /// <returns></returns>
        public static List<T> shuffleList<T>(this List<T> inputList)
        {    //take any list of GameObjects and return it with Fischer-Yates shuffle
            int i = 0;
            int t = inputList.Count;
            int r = 0;
            T p = default(T);
            List<T> tempList = new List<T>();
            tempList.AddRange(inputList);

            while (i < t)
            {
                r = UnityEngine.Random.Range(i, tempList.Count);
                p = tempList[i];
                tempList[i] = tempList[r];
                tempList[r] = p;
                i++;
            }

            return tempList;
        }
    }
}
