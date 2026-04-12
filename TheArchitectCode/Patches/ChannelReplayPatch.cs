using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.AfterCardPlayed))]
public static class ChannelReplayPatch
{
    public static void Postfix(CardModel __instance, PlayerChoiceContext context, CardPlay cardPlay, ref Task __result)
    {
        __result = ReplayAsync(__result, __instance, context, cardPlay);
    }

    private static async Task ReplayAsync(Task original, CardModel card, PlayerChoiceContext context, CardPlay cardPlay)
    {
        await original;

        if (cardPlay.Card != card || cardPlay.IsAutoPlay)
        {
            return;
        }

        int repeats = ArchitectCombatState.ConsumePendingReplays(card);
        for (int i = 0; i < repeats; i++)
        {
            card.SetToFreeThisTurn();
            await CardCmd.AutoPlay(context, card, cardPlay.Target);
        }
    }
}
