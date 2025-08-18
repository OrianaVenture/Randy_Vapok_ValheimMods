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


### Example

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