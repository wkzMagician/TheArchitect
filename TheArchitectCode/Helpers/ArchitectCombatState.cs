using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Helpers;

public static class ArchitectCombatState
{
    private sealed class State
    {
        public int CardsEnchantedThisCombat;
        public int EnchantedCardsPlayedThisCombat;
        public int EnchantedCardsPlayedThisTurn;
    }

    private sealed class CardState
    {
        public int TimesPlayedThisCombat;
        public int PendingReplays;
    }

    private static readonly ConditionalWeakTable<CombatState, State> States = new();
    private static readonly ConditionalWeakTable<CardModel, CardState> CardStates = new();

    public static void OnTurnStart(CombatState combatState)
    {
        State state = States.GetValue(combatState, _ => new State());
        state.EnchantedCardsPlayedThisTurn = 0;
    }

    public static void RecordEnchanted(CardModel card)
    {
        States.GetValue(card.CombatState!, _ => new State()).CardsEnchantedThisCombat++;
    }

    public static void RecordPlayed(CardModel card)
    {
        CardStates.GetValue(card, _ => new CardState()).TimesPlayedThisCombat++;

        if (card.Enchantment == null)
        {
            return;
        }

        State state = States.GetValue(card.CombatState!, _ => new State());
        state.EnchantedCardsPlayedThisCombat++;
        state.EnchantedCardsPlayedThisTurn++;
    }

    public static int CardsEnchantedThisCombat(CardModel card)
    {
        return States.GetValue(card.CombatState!, _ => new State()).CardsEnchantedThisCombat;
    }

    public static int EnchantedCardsPlayedThisCombat(CardModel card)
    {
        return States.GetValue(card.CombatState!, _ => new State()).EnchantedCardsPlayedThisCombat;
    }

    public static int EnchantedCardsPlayedThisTurn(CardModel card)
    {
        return States.GetValue(card.CombatState!, _ => new State()).EnchantedCardsPlayedThisTurn;
    }

    public static int EnchantedCardsPlayedThisTurn(CombatState combatState)
    {
        return States.GetValue(combatState, _ => new State()).EnchantedCardsPlayedThisTurn;
    }

    public static int TimesPlayed(CardModel card)
    {
        return CardStates.GetValue(card, _ => new CardState()).TimesPlayedThisCombat;
    }

    public static void SetPendingReplays(CardModel card, int repeats)
    {
        if (repeats <= 0)
        {
            return;
        }

        CardStates.GetValue(card, _ => new CardState()).PendingReplays += repeats;
    }

    public static int ConsumePendingReplays(CardModel card)
    {
        CardState state = CardStates.GetValue(card, _ => new CardState());
        int repeats = state.PendingReplays;
        state.PendingReplays = 0;
        return repeats;
    }
}
