using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public abstract class IncreaseDrop
{
    public static void TryDropExtraItems(Character character, string effect, DropTable dropTable, Vector3 objPosition)
    {
        if (character is Player)
        {
            (character as Player).HasActiveMagicEffect(effect, out float effectValue);

            if (effectValue > 0)
            {
                DropExtraItems(dropTable.GetDropList(Mathf.RoundToInt(effectValue)), objPosition);
            }
        }
    }

    public static void DropExtraItems(List<GameObject> dropList, Vector3 objPosition)
    {
        EpicLoot.Log($"DropExtraItems!");
        Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.5f;
        Vector3 position = objPosition + Vector3.up + new Vector3(vector.x, 0, vector.y);
        Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);

        for (int i = 0; i < dropList.Count; i++)
        {
            GameObject drop = dropList[i];
            ItemDrop.OnCreateNew(UnityEngine.Object.Instantiate(drop, position, rotation));
        }
    }
}