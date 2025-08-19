# Epic Loot API

API designed to help developers extend [Epic Loot](https://valheim.thunderstore.io/package/RandyKnapp/EpicLoot/) with their own **magic effects**, **legendary items**, **sets**, and **abilities**.  
This wrapper provides convenience classes and reflection-based accessors to register new content into Epic Loot.

---

## 📦 Installation

You can use the API in one of two ways:

### 1. Bundle as DLL (Recommended)

Include `EpicLootAPI.dll` into your project and bundle it into your plugin using [ILRepack](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task).

**Example `ILRepack.targets`:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <Target Name="ILRepacker" AfterTargets="Build">
        <ItemGroup>
            <InputAssemblies Include="$(TargetPath)" />
            <InputAssemblies Include="$(OutputPath)\EpicLootAPI.dll" />
        </ItemGroup>
        <ILRepack Parallel="true" DebugInfo="true" Internalize="true"
                  InputAssemblies="@(InputAssemblies)"
                  OutputFile="$(TargetPath)"
                  TargetKind="SameAsPrimaryAssembly"
                  LibraryPath="$(OutputPath)" />
    </Target>
</Project>
```
### 2. Source Files

Copy API.cs and EffectTypes.cs into your plugin project.
⚠️ Do not modify the provided methods unless you know what you are doing.

### Using API

After you finish defining all your content, make sure to `Register` them to EpicLoot

If you need to update your custom classes, use the API `Update` functions

### Example Magic Effect

```c#
public void Awake()
{
    var Definition = new EpicLoot.MagicItemEffectDefinition("Blink", "Blink", "Teleport to impact point");
    Definition.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Bows, Skills.SkillType.Spears);
    Definition.Requirements.AllowedRarities.Add(EpicLoot.ItemRarity.Epic, EpicLoot.ItemRarity.Legendary, EpicLoot.ItemRarity.Mythic);
    Definition.SelectionWeight = 1;
}

[HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
private static class Projectile_Setup_Patch
{
    private static void Postfix(Projectile __instance, Vector3 hitPoint)
    {
        if (__instance.m_owner is not Player player) return;
        if (!EpicLoot.HasActiveMagicEffectOnWeapon(null, __instance.m_weapon, "Blink", out float _)) return;

        player.TeleportInstant(hitPoint, player.transform.rotation);
    }
}

private static void TeleportInstant(this Player player, Vector3 position, Quaternion rotation)
{
    player.transform.position = position;
    player.transform.rotation = rotation;
}

```

### Example Legendary Item
```c#
var legendary = new EpicLoot.LegendaryInfo(EpicLoot.LegendaryType.Mythic, "RustyCrossbow", "Rusty Crossbow", "Gods have favored you");
legendary.Requirements.AllowedSkillTypes.Add(Skills.SkillType.Crossbows);
legendary.GuaranteedMagicEffects.Add("AddFrostDamage", 5, 10, 10);
legendary.GuaranteedMagicEffects.Add("Indestructible");
legendary.GuaranteedEffectCount = 6;
```

### Example Legendary Set
```C#
EpicLoot.LegendarySetInfo DragonSet = new EpicLoot.LegendarySetInfo(EpicLoot.LegendaryType.Mythic, "DragonForm", "Dragon Form");
DragonSet.SetBonuses.Add(2, EffectType.ModifyStaminaRegen, 40, 40, 1);
DragonSet.SetBonuses.Add(3, EffectType.AddCarryWeight, 100, 100, 1);
DragonSet.SetBonuses.Add(4, "DragonForm", 1, 1, 1);
DragonSet.LegendaryIDs.Add("DragonChest", "DragonLegs", "DragonCape", "DragonHelmet");

EpicLoot.LegendaryInfo DragonChest = new EpicLoot.LegendaryInfo(EpicLoot.LegendaryType.Mythic,
    "DragonChest", "Dragon Chestpiece", "Cries from the queen ring throughout the fabric of this armor");
DragonChest.IsSetItem = true;
DragonChest.Requirements.AllowedItemTypes.Add("Chest");
DragonChest.GuaranteedEffectCount = 6;
DragonChest.GuaranteedMagicEffects.Add(EffectType.ModifyArmor);
DragonChest.GuaranteedMagicEffects.Add(EffectType.IncreaseStamina);

EpicLoot.LegendaryInfo DragonLegs = new EpicLoot.LegendaryInfo(EpicLoot.LegendaryType.Mythic, "DragonLegs",
    "Dragon Legwarmers", "Padded with the scaly furs of dragons.");
DragonLegs.IsSetItem = true;
DragonLegs.Requirements.AllowedItemTypes.Add("Legs");
DragonLegs.GuaranteedEffectCount = 6;
DragonLegs.GuaranteedMagicEffects.Add(EffectType.AddMovementSkills);
DragonLegs.GuaranteedMagicEffects.Add(EffectType.ModifyMovementSpeedLowHealth);

EpicLoot.LegendaryInfo DragonCape = new EpicLoot.LegendaryInfo(EpicLoot.LegendaryType.Mythic, "DragonCape", "Dragon Cape", "The mere smell of this fabric calls out to the dragons.");
DragonCape.IsSetItem = true;
DragonCape.Requirements.AllowedItemTypes.Add("Shoulder");
DragonCape.GuaranteedEffectCount = 6;

EpicLoot.LegendaryInfo DragonHelmet = new EpicLoot.LegendaryInfo(EpicLoot.LegendaryType.Mythic, "DragonHelmet", "Dragon Helmet", "Marks from the last war of the dragons still flicker on this helmet.");
DragonHelmet.IsSetItem = true;
DragonHelmet.Requirements.AllowedItemTypes.Add("Helmet");
DragonHelmet.GuaranteedEffectCount = 6;
```

### Example Ability 
```c#
SE_Stats SE_DragonForm = ScriptableObject.CreateInstance<SE_Stats>();
SE_DragonForm.name = "SE_DragonForm"
// make sure to register your Status Effect into ObjectDB
EpicLoot.AbilityDefinition DragonAbility = new EpicLoot.AbilityDefinition("DragonForm", "gdkingheart", 100f, "SE_DragonForm");
DragonAbility.IconAsset = "MyIconName";
EpicLoot.RegisterAsset(MySprite.name, MySprite);
```

### Example Recipe
```c#
EpicLoot.Recipe RustyRecipe = new EpicLoot.Recipe("Recipe_IronOre_to_Iron", "Iron", EpicLoot.CraftingTable.Workbench,  5);
RustyRecipe.resources.Add("IronOre", 5);
```

### Example Material Conversion
```c#
EpicLoot.MaterialConversion HealthUpgrade_Bonemass = new EpicLoot.MaterialConversion(EpicLoot.MaterialConversionType.Junk, "Recipe_HealthUpgrade_Bonemass_To_Mythic_Runestone", "RunestoneMythic");
HealthUpgrade_Bonemass.Resources.Add("HealthUpgrade_Bonemass", 1);
```

### Example Sacrifice
```c#
EpicLoot.Sacrifice SacrificeHearts = new EpicLoot.Sacrifice();
SacrificeHearts.ItemNames.Add("Bonemass heart", "Elder heart");
SacrificeHearts.AddRequiredItemType(ItemDrop.ItemData.ItemType.Consumable);
SacrificeHearts.Products.Add("ShardMythic", 2);
```