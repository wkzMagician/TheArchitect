using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
public static class ChannelReplayDescriptionPatch
{
    private static void Postfix(CardModel __instance, ref string __result)
    {
        int repeats = ArchitectCombatState.PendingReplays(__instance);
        if (repeats <= 0) return;
        // Hidden Gem uses REPLAY.extraText for its permanent replay count.
        // Keep Channel Power's one-shot count separate from permanent replays.
        LocString replay = new("static_hover_tips", "REPLAY.extraText");
        replay.Add("Times", repeats);
        LocString description = new("cards", "THEARCHITECT-CHANNEL_REPLAY.description");
        description.Add("Replay", replay.GetFormattedText());
        string effect = description.GetFormattedText();
        __result = string.IsNullOrWhiteSpace(__result) ? effect : $"{__result}\n{effect}";
    }
}
