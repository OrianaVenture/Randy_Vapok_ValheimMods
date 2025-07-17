using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.SpawnOnHit))]
    public class Apportation
    {
        public static void Postfix(Projectile __instance, GameObject go, Collider collider, Vector3 normal)
        {
            var item = __instance.m_spawnItem;
            var player = Player.m_localPlayer;
            if ((go.GetComponent<MonsterAI>() || go.GetComponent<BaseAI>()) && item != null && item.HasMagicEffect(MagicEffectType.Apportation))
            {
                Vector3 weaponPosition = __instance.transform.position;
                Vector3 targetPosition = weaponPosition + __instance.transform.TransformDirection(__instance.m_spawnOffset);
                if (Player.m_localPlayer != null && player == __instance.m_owner)
                {
                    Player.m_localPlayer.transform.position = targetPosition;
                }
            }
        }
    }
}
