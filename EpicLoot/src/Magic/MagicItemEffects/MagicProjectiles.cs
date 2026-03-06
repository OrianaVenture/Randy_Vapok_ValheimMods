using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot;

public static class MagicProjectiles
{
    private static ItemDrop arrow;
    private static ItemDrop bolt;
    
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public static void CreateProjectiles()
    {
        CreateBowProjectile();
        CreateCrossbowProjectile();
        Jotunn.Managers.PrefabManager.OnVanillaPrefabsAvailable -= CreateProjectiles;
    }

    private static float GetEitrCost(string effectType)
    {
        Dictionary<string, float> configs = MagicItemEffectDefinitions.GetEffectConfig(effectType);
        return configs.TryGetValue("EitrCost", out float value) ? value : 5f;
    }

    private static HitData.DamageTypes GetRandomElementalDamage(float effectValue)
    {
        return Random.value switch
        {
            > 0.80f => new HitData.DamageTypes { m_frost     = effectValue },
            > 0.60f => new HitData.DamageTypes { m_fire      = effectValue },
            > 0.40f => new HitData.DamageTypes { m_lightning = effectValue },
            > 0.20f => new HitData.DamageTypes { m_poison    = effectValue },
            _       => new HitData.DamageTypes { m_spirit    = effectValue },
        };
    }

    private static void CreateBowProjectile()
    {
        GameObject arrowPrefab = Jotunn.Managers.PrefabManager.Instance.CreateClonedPrefab(
            "EL_MagicArrow", 
            "ArrowFrost");
        
        arrow = arrowPrefab.GetComponent<ItemDrop>();
        GameObject projectile = Jotunn.Managers.PrefabManager.Instance.CreateClonedPrefab(
            "bow_projectile_magic_el", 
            arrow.m_itemData.m_shared.m_attack.m_attackProjectile);
        
        MeshRenderer model = projectile.transform.Find("model").GetComponent<MeshRenderer>();
        for (int i = 0; i < model.sharedMaterials.Length; ++i)
        {
            Material material = model.sharedMaterials[i];
            Material mat = new Material(material);
            mat.SetColor(EmissionColor, material.GetColor(EmissionColor) * 5f);
            model.sharedMaterials[i] = mat;
            model.materials[i] = mat;
        }

        MeshRenderer tip = projectile.transform.Find("Sphere").GetComponent<MeshRenderer>();
        tip.sharedMaterials = model.sharedMaterials;
        tip.materials = model.sharedMaterials;

        arrow.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
        arrow.m_itemData.m_shared.m_damages = new HitData.DamageTypes();
        arrow.m_itemData.m_shared.m_name = "Magic Arrow";
        arrow.m_itemData.m_shared.m_description = "Formed from otherworldy powers";
        
        Jotunn.Managers.PrefabManager.OnPrefabsRegistered += () =>
        {
            Jotunn.Managers.PrefabManager.Instance.RegisterToZNetScene(arrowPrefab);
            Jotunn.Managers.PrefabManager.Instance.RegisterToZNetScene(projectile);
        };
    }

    private static void CreateCrossbowProjectile()
    {
        GameObject boltPrefab = Jotunn.Managers.PrefabManager.Instance.CreateClonedPrefab(
            "EL_MagicBolt", 
            "BoltCarapace");
        
        bolt = boltPrefab.GetComponent<ItemDrop>();
        GameObject projectile = Jotunn.Managers.PrefabManager.Instance.CreateClonedPrefab(
            "arbalest_projectile_magic_el", 
            bolt.m_itemData.m_shared.m_attack.m_attackProjectile);
        
        MeshRenderer model = projectile.transform.Find("default").GetComponent<MeshRenderer>();
        for (int i = 0; i < model.sharedMaterials.Length; ++i)
        {
            Material material = model.sharedMaterials[i];
            Material mat = new Material(material);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(EmissionColor, new Color(0f, 0.8f, 1f) * 5f);
            model.sharedMaterials[i] = mat;
            model.materials[i] = mat;
        }

        bolt.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
        bolt.m_itemData.m_shared.m_damages = new HitData.DamageTypes();
        bolt.m_itemData.m_shared.m_name = "Magic Bolt";
        bolt.m_itemData.m_shared.m_description = "Formed from otherworldy powers";
        
        Jotunn.Managers.PrefabManager.OnPrefabsRegistered += () =>
        {
            Jotunn.Managers.PrefabManager.Instance.RegisterToZNetScene(boltPrefab);
            Jotunn.Managers.PrefabManager.Instance.RegisterToZNetScene(projectile);
        };
    }

    private static bool HasMagicArrow(Player player, ItemDrop.ItemData weapon)
    {
        if (!player.HasActiveMagicEffect(MagicEffectType.MagicArrow)) return false;
        if (!weapon.m_shared.m_ammoType.Equals(arrow.m_itemData.m_shared.m_ammoType)) return false;
        if (!player.HaveEitr(GetEitrCost(MagicEffectType.MagicArrow))) return false;
        return true;
    }

    private static bool HasMagicBolt(Player player, ItemDrop.ItemData weapon)
    {
        if (!player.HasActiveMagicEffect(MagicEffectType.MagicBolt)) return false;
        if (!weapon.m_shared.m_ammoType.Equals(bolt.m_itemData.m_shared.m_ammoType)) return false;
        if (!player.HaveEitr(GetEitrCost(MagicEffectType.MagicBolt))) return false;
        return true;
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.FindAmmo))]
    private static class MagicProjectiles_FindAmmo_Patch
    {
        private static void Postfix(
            Humanoid character, 
            ItemDrop.ItemData weapon, 
            ref ItemDrop.ItemData __result)
        {
            if (character is not Player player) return;

            if (HasMagicArrow(player, weapon))
            {
                __result = arrow.m_itemData.Clone();
            }

            if (HasMagicBolt(player, weapon))
            {
                __result = bolt.m_itemData.Clone();
            }
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.EquipAmmoItem))]
    private static class MagicProjectiles_EquipAmmoItem_Patch
    {
        private static bool Prefix(
            Humanoid character, 
            ItemDrop.ItemData weapon, 
            ref bool __result)
        {
            if (character is not Player player) return true;

            if (HasMagicArrow(player, weapon))
            {
                __result = true;
                return false;
            }

            if (HasMagicBolt(player, weapon))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    private static class MagicProjectiles_HaveAmmo_Patch
    {
        private static bool Prefix(
            Humanoid character, 
            ItemDrop.ItemData weapon, 
            ref bool __result)
        {
            if (character is not Player player) return true;
            
            if (HasMagicArrow(player, weapon))
            {
                __result = true;
                return false;
            }

            if (HasMagicBolt(player, weapon))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo))]
    private static class MagicProjectiles_UseAmmo_Patch
    {
        private static bool Prefix(
            Attack __instance, 
            out ItemDrop.ItemData ammoItem, 
            ref bool __result)
        {
            ammoItem = __instance.m_character.GetAmmoItem();
            if (__instance.m_character is not Player player) return true;
            
            if (HasMagicArrow(player, __instance.m_weapon))
            {
                float effectValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.MagicArrow);
                ammoItem = arrow.m_itemData.Clone();
                ammoItem.m_shared.m_damages = GetRandomElementalDamage(effectValue);
                __instance.m_ammoItem = ammoItem;
                player.UseEitr(GetEitrCost(MagicEffectType.MagicArrow));
                __result = true;
                return false;
            }

            if (HasMagicBolt(player, __instance.m_weapon))
            {
                float effectValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.MagicBolt);
                ammoItem = bolt.m_itemData.Clone();
                ammoItem.m_shared.m_damages = GetRandomElementalDamage(effectValue);
                __instance.m_ammoItem = ammoItem;
                player.UseEitr(GetEitrCost(MagicEffectType.MagicBolt));
                __result = true;
                return false;
            }

            return true;
        }
    }


}