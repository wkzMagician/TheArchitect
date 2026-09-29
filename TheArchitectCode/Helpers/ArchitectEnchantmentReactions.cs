using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Powers.Architect;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.TheArchitectCode.Helpers;

// All Architect enchantment operations report their completed changes here.
internal static class ArchitectEnchantmentReactions
{
    public static void AfterAdded(CardModel card)
    {
        if (card.Owner?.Creature == null || card.CombatState == null) return;

        AfterCardEnchantedOrRefreshed(card);
        if (card.Pile?.Type == PileType.Deck) return;

        foreach (CalibrationRuler relic in card.Owner.Relics.OfType<CalibrationRuler>())
            ArchitectEffectQueue.Track(card.Owner, relic.OnFirstEnchantment(card));

        if (card.Owner.Creature.GetPower<SanctuaryPower>() is { } sanctuary)
            ArchitectEffectQueue.Track(card.Owner,
                CreatureCmd.GainBlock(card.Owner.Creature, sanctuary.Amount, ValueProp.Move, null));
    }

    public static void AfterRemoved(CardModel card)
    {
        if (card.Owner?.Creature == null || card.CombatState == null || card.Pile?.Type == PileType.Deck)
            return;

        foreach (DismantlingPliers relic in card.Owner.Relics.OfType<DismantlingPliers>())
            ArchitectEffectQueue.Track(card.Owner, relic.OnActiveRemoval(card));

        if (card.Owner.Creature.GetPower<ExplosiveCorePower>() is { } explosiveCore)
            ArchitectEffectQueue.Track(card.Owner, explosiveCore.OnEnchantmentsRemoved(card, 1));

        if (card.Owner.Creature.GetPower<DestroyerPower>() is { } destroyer)
            ArchitectEffectQueue.Track(card.Owner, PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(), card.Owner.Creature, destroyer.Amount, card.Owner.Creature, null));
    }

    public static void AfterRefreshed(CardModel card, bool triggerAutomaton = true)
    {
        AfterCardEnchantedOrRefreshed(card, triggerAutomaton);
    }

    private static void AfterCardEnchantedOrRefreshed(CardModel card, bool triggerAutomaton = true)
    {
        if (card.Owner?.Creature == null || card.CombatState == null) return;

        if (card is SoulTotem)
            ArchitectEffectQueue.Track(card.Owner, PlayerCmd.GainEnergy(1, card.Owner));

        if (triggerAutomaton && card is Automaton)
            ArchitectEffectQueue.Track(card.Owner,
                CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null));
    }
}
