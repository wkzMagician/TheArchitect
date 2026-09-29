using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(MegaCrit.Sts2.Core.Entities.Creatures.Creature)])]
public static class RollbackDescriptionPatch
{
    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (!ArchitectCombatState.ShouldShuffle(__instance))
        {
            return;
        }

        string effectDescription = new LocString("cards", "THEARCHITECT-ROLLBACK_SHUFFLE.description").GetFormattedText();
        __result = string.IsNullOrWhiteSpace(__result) ? effectDescription : $"{__result} {effectDescription}";
    }
}
