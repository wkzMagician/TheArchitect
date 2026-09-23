using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

public static class ArchitectCombatHooksPatch
{
    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyCardPlayResultPileTypeAndPosition))]
    private static class RollbackDestination
    {
        private static void Postfix(MegaCrit.Sts2.Core.Models.CardModel card, ref (PileType, CardPilePosition) __result)
        {
            if (ArchitectCombatState.ShouldShuffle(card)) __result = (PileType.Draw, CardPilePosition.Random);
        }
    }
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
    private static class BeforePlay
    {
        private static void Prefix(CardPlay cardPlay) => ArchitectCombatState.CapturePlay(cardPlay.Card);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
    private static class AfterPlay
    {
        // Count all card types before powers/relics inspect the completed play.
        private static void Prefix(CardPlay cardPlay) => ArchitectCombatState.RecordPlayed(cardPlay.Card);
        private static void Postfix(ref Task __result, CardPlay cardPlay) => __result = Finish(__result, cardPlay.Card.Owner);
        private static async Task Finish(Task original, Player player)
        {
            await original;
            await ArchitectEffectQueue.Drain(player);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
    private static class TurnStart
    {
        private static void Prefix(Player player) => ArchitectCombatState.OnTurnStart(player);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeTurnEnd))]
    private static class TurnEnd
    {
        private static void Prefix(CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Player) return;
            foreach (Creature creature in participants)
                if (creature.Player is { } player) ArchitectCombatState.ExpirePotionReplays(player);
        }
    }
}
