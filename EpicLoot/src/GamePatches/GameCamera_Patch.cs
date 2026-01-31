namespace EpicLoot;

using HarmonyLib;

// TODO: Patch was added in 2021 and overrides free-fly to 30 FPS; check if this can be removed.
[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateCamera))]
static class GameCamera_UpdateCamera_Patch
{
    const float _dt = 1f / 30f;

    static bool Prefix(GameCamera __instance)
    {
        if (__instance.m_freeFly)
        {
            __instance.UpdateFreeFly(_dt);
            __instance.UpdateCameraShake(_dt);

            return false;
        }

        return true;
    }
}
