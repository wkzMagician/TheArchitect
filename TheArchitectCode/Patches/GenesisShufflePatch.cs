using System.Reflection;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch]
public static class GenesisShufflePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod() => AccessTools.Method(
        typeof(CardPileCmd),
        nameof(CardPileCmd.Add),
        [typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])!;

    public static void Postfix(
        CardModel card,
        [HarmonyArgument(1)] PileType pileType,
        ref Task<CardPileAddResult> __result)
    {
        if (pileType == PileType.Draw)
        {
            __result = GrantGenesisBlock(__result, card);
        }
    }

    private static async Task<CardPileAddResult> GrantGenesisBlock(Task<CardPileAddResult> original, CardModel card)
    {
        CardPileAddResult result = await original;
        if (card.Owner?.Creature is { } creature && card.CombatState != null &&
            creature.GetPower<GenesisPower>() is { } genesis)
        {
            await CreatureCmd.GainBlock(creature, genesis.Amount, ValueProp.Move, null);
        }

        return result;
    }
}
