using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;

namespace TheArchitect.TheArchitectCode.Helpers;

public static class ArchitectCombatState
{
    private sealed class State
    {
        public int CardsEnchantedThisCombat;
        public int EnchantedCardsPlayedThisCombat;
        public int EnchantedCardsPlayedThisTurn;
        public readonly Dictionary<Player, int> EnchantedByPlayerThisCombat = [];
        public readonly Dictionary<Player, int> PlayedByPlayerThisTurn = [];
        public readonly Dictionary<Player, int> EnchantedSeriesByPlayerThisTurn = [];
        public readonly Dictionary<Player, int> PlayedByPlayerThisCombat = [];
        public readonly HashSet<CardModel> PotionTargets = [];
    }

    private sealed class CardState
    {
        public int TimesPlayedThisCombat;
        public int PendingReplays;
        public int PotionReplays;
        public Player? PotionOwner;
        public bool ShuffleAfterPlay;
        public readonly Stack<bool> PlaySnapshots = [];
        public bool? LastPlayWasEnchanted;
    }

    private static readonly ConditionalWeakTable<ICombatState, State> States = new();
    private static readonly ConditionalWeakTable<CardModel, CardState> CardStates = new();

    public static void OnTurnStart(ICombatState combatState)
    {
        State state = States.GetValue(combatState, _ => new State());
        state.EnchantedCardsPlayedThisTurn = 0;
        state.PlayedByPlayerThisTurn.Clear();
        state.EnchantedSeriesByPlayerThisTurn.Clear();
    }

    public static void OnTurnStart(Player player)
    {
        if (player.Creature.CombatState is not { } combat) return;
        State state = States.GetOrCreateValue(combat);
        state.PlayedByPlayerThisTurn[player] = 0;
        state.EnchantedSeriesByPlayerThisTurn[player] = 0;
        state.EnchantedCardsPlayedThisTurn = state.PlayedByPlayerThisTurn.Values.Sum();
        ExpirePotionReplays(player);
    }

    public static void CapturePlay(CardModel card) => CardStates.GetOrCreateValue(card).PlaySnapshots.Push(ArchitectEnchantmentHelper.HasAny(card));

    public static void MarkForShuffle(CardModel card) => CardStates.GetOrCreateValue(card).ShuffleAfterPlay = true;
    public static bool ShouldShuffle(CardModel card) => CardStates.GetOrCreateValue(card).ShuffleAfterPlay;
    public static bool WasEnchantedOnPlay(CardModel card) => CardStates.GetOrCreateValue(card).LastPlayWasEnchanted ?? ArchitectEnchantmentHelper.HasAny(card);

    public static void RecordEnchanted(CardModel card)
    {
        ICombatState? combatState = card.CombatState;
        if (combatState == null)
        {
            return;
        }

        State state = States.GetOrCreateValue(combatState);
        state.CardsEnchantedThisCombat++;
        state.EnchantedByPlayerThisCombat[card.Owner] = state.EnchantedByPlayerThisCombat.GetValueOrDefault(card.Owner) + 1;
    }

    public static void RecordPlayed(CardModel card, bool isFirstInSeries = true)
    {
        CardState cardState = CardStates.GetOrCreateValue(card);
        cardState.TimesPlayedThisCombat++;
        bool wasEnchanted = cardState.PlaySnapshots.Count > 0 ? cardState.PlaySnapshots.Pop() : ArchitectEnchantmentHelper.HasAny(card);
        cardState.LastPlayWasEnchanted = wasEnchanted;

        if (!wasEnchanted)
        {
            return;
        }

        ICombatState? combatState = card.CombatState;
        if (combatState == null)
        {
            return;
        }

        State state = States.GetValue(combatState, _ => new State());
        state.EnchantedCardsPlayedThisCombat++;
        if (isFirstInSeries)
        {
            state.EnchantedSeriesByPlayerThisTurn[card.Owner] = state.EnchantedSeriesByPlayerThisTurn.GetValueOrDefault(card.Owner) + 1;
        }
        state.EnchantedCardsPlayedThisTurn++;
        state.PlayedByPlayerThisTurn[card.Owner] = state.PlayedByPlayerThisTurn.GetValueOrDefault(card.Owner) + 1;
        state.PlayedByPlayerThisCombat[card.Owner] = state.PlayedByPlayerThisCombat.GetValueOrDefault(card.Owner) + 1;
    }

    public static int EnchantedCardSeriesPlayedThisTurn(Player player) =>
        player.Creature.CombatState is { } combat
            ? States.GetOrCreateValue(combat).EnchantedSeriesByPlayerThisTurn.GetValueOrDefault(player)
            : 0;

    public static int CardsEnchantedThisCombat(CardModel card)
    {
        ICombatState? combatState = card.CombatState;
        return combatState == null ? 0 : States.GetOrCreateValue(combatState).EnchantedByPlayerThisCombat.GetValueOrDefault(card.Owner);
    }

    public static int EnchantedCardsPlayedThisCombat(CardModel card)
    {
        ICombatState? combatState = card.CombatState;
        return combatState == null ? 0 : States.GetOrCreateValue(combatState).PlayedByPlayerThisCombat.GetValueOrDefault(card.Owner);
    }

    public static int EnchantedCardsPlayedThisTurn(CardModel card)
    {
        ICombatState? combatState = card.CombatState;
        return combatState == null ? 0 : States.GetOrCreateValue(combatState).PlayedByPlayerThisTurn.GetValueOrDefault(card.Owner);
    }

    public static int EnchantedCardsPlayedThisTurn(Player player) => player.Creature.CombatState is { } combat
        ? States.GetOrCreateValue(combat).PlayedByPlayerThisTurn.GetValueOrDefault(player) : 0;

    public static int EnchantedCardsPlayedThisTurn(ICombatState combatState)
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

    public static int PendingReplays(CardModel card) => CardStates.TryGetValue(card, out CardState? state) ? state.PendingReplays : 0;

    public static int ConsumePendingReplays(CardModel card)
    {
        CardState state = CardStates.GetValue(card, _ => new CardState());
        int repeats = state.PendingReplays;
        state.PendingReplays = 0;
        return repeats;
    }

    public static void SetPotionReplays(CardModel card, int repeats)
    {
        if (repeats <= 0 || card.CombatState == null) return;
        CardState state = CardStates.GetOrCreateValue(card);
        if (state.PotionOwner != card.Owner) state.PotionReplays = 0;
        state.PotionOwner = card.Owner;
        state.PotionReplays += repeats;
        States.GetOrCreateValue(card.CombatState).PotionTargets.Add(card);
    }

    public static int ConsumePotionReplays(CardModel card)
    {
        CardState state = CardStates.GetOrCreateValue(card);
        int repeats = state.PotionOwner == card.Owner ? state.PotionReplays : 0;
        state.PotionReplays = 0;
        return repeats;
    }

    public static void ExpirePotionReplays(Player player)
    {
        if (player.Creature.CombatState is not { } combat) return;
        State state = States.GetOrCreateValue(combat);
        foreach (CardModel card in state.PotionTargets.ToArray())
        {
            CardState cardState = CardStates.GetOrCreateValue(card);
            if (cardState.PotionOwner != player) continue;
            cardState.PotionReplays = 0;
            state.PotionTargets.Remove(card);
        }
    }
}
