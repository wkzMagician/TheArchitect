using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new[]
{
    typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)
})]
public static class CorruptionHandEntryPatch
{
    public static void Postfix(CardModel card, [HarmonyArgument(1)] PileType pileType, ref Task<CardPileAddResult> __result)
    {
        if (pileType != PileType.Hand)
        {
            return;
        }

        __result = EnchantAfterAdd(__result, card);
    }

    private static async Task<CardPileAddResult> EnchantAfterAdd(Task<CardPileAddResult> original, CardModel card)
    {
        CardPileAddResult result = await original;
        Powers.Architect.BlightAnointingPower.TryEnchant(card);
        return result;
    }
}
