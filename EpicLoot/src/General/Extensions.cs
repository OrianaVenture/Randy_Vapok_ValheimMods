using System.Collections.Generic;
using System.Linq;
using static ItemDrop;

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
            List<T> tempList = new List<T>();
            tempList.AddRange(inputList);
            int count = inputList.Count;
            for (int i = 0; i < count; i++)
            {
                int tpos = UnityEngine.Random.Range(i, count);
                p = tempList[i];
                tempList[i] = tempList[tpos];
                tempList[tpos] = p;
            }
            //EpicLoot.Log($"Input list: {string.Join(",", inputList)}");
            //EpicLoot.Log($"Shuffled l: {string.Join(",", tempList)}");
            return tempList;
        }

        public static bool hasElelemtalDamage(this ItemDrop.ItemData item)
        {
            return item.m_shared.m_damages.m_fire +
                item.m_shared.m_damages.m_frost +
                item.m_shared.m_damages.m_lightning +
                item.m_shared.m_damages.m_poison +
                item.m_shared.m_damages.m_spirit > 0;
        }
    }
}