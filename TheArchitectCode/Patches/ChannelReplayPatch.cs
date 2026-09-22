using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

// Use the engine's replay count, so cost, X value, final pile, and exhaustion are
// handled once by OnPlayWrapper. A replay cannot recursively consume its own mark.
public static class ChannelReplayPatch
{
    private sealed class PlayContext { public bool IsAutoPlay; }
    private static readonly ConditionalWeakTable<CardModel, PlayContext> Plays = new();

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    private static class CapturePlayKind
    {
        private static void Prefix(CardModel __instance, bool isAutoPlay) => Plays.GetOrCreateValue(__instance).IsAutoPlay = isAutoPlay;
    }

    [HarmonyPatch(typeof(CardModel), "GeneratePlayCount")]
    private static class ReplayCount
    {
        private static void Prefix(CardModel __instance, out int __state)
        {
            __state = ArchitectCombatState.ConsumePendingReplays(__instance);
            if (!Plays.GetOrCreateValue(__instance).IsAutoPlay)
                __state += ArchitectCombatState.ConsumePotionReplays(__instance);
        }
        private static void Postfix(ref Task<int> __result, int __state) => __result = AddReplays(__result, __state);
        private static async Task<int> AddReplays(Task<int> original, int repeats) => await original + repeats;
    }
}
