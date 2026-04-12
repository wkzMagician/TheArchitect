using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add))]
public static class CorruptionHandEntryPatch
{
    public static void Postfix(CardModel card, PileType pileType, ref Task __result)
    {
        if (pileType != PileType.Hand)
        {
            return;
        }

        __result = EnchantAfterAdd(__result, card);
    }

    private static async Task EnchantAfterAdd(Task original, CardModel card)
    {
        await original;
        Powers.Architect.BlightAnointingPower.TryEnchant(card);
    }
}
