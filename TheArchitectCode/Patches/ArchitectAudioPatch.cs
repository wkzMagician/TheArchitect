using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Audio;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.TheArchitectCode.Patches;

// The base game's sound properties are non-virtual and synthesize an FMOD event
// path from the character ID ("event:/sfx/characters/TheArchitect/..."), which
// does not exist in the shipped banks. Re-route only this character onto events
// that do exist: the Architect boss's own attack event plus Ironclad stand-ins.
[HarmonyPatch]
public static class ArchitectAudioPatch
{
    [HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.AttackSfx), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool AttackSfxPrefix(CharacterModel __instance, ref string __result)
        => UseFallback(__instance, ArchitectSfx.ArchitectAttack, ref __result);

    [HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CastSfx), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool CastSfxPrefix(CharacterModel __instance, ref string __result)
        => UseFallback(__instance, ArchitectSfx.IroncladCast, ref __result);

    [HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.PowerUpSfx), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PowerUpSfxPrefix(CharacterModel __instance, ref string __result)
        => UseFallback(__instance, ArchitectSfx.IroncladCast, ref __result);

    [HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.DeathSfx), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool DeathSfxPrefix(CharacterModel __instance, ref string __result)
        => UseFallback(__instance, ArchitectSfx.IroncladDie, ref __result);

    private static bool UseFallback(CharacterModel character, string fallback, ref string result)
    {
        if (character is not Character.TheArchitect)
            return true;

        result = fallback;
        return false;
    }
}
