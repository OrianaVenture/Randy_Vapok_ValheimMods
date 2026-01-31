namespace EpicLoot;

using global::EpicLoot.Adventure;
using HarmonyLib;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
static class ZNetPatches
{
    static void Postfix(ZNet __instance)
    {
        if (__instance.IsServer())
        {
            __instance.gameObject.AddComponent<BountyManagmentSystem>();
        }
        
        AdventureDataManager.Bounties.RegisterRPC(__instance.m_routedRpc);
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
static class ZNet_Start_Patch
{
    static void Postfix(ZNet __instance)
    {
        AdventureDataManager.OnZNetStart();
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.OnDestroy))]
static class ZNet_OnDestroy_Patch
{
    static void Postfix(ZNet __instance)
    {
        AdventureDataManager.OnZNetDestroyed();
    }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.SaveWorld))]
static class ZNet_SaveWorld_Patch
{
    static void Prefix(ZNet __instance)
    {
        AdventureDataManager.OnWorldSave();
    }
}
