using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;
using System.Runtime.CompilerServices;

namespace TheArchitect.TheArchitectCode.Relics;

// Internal data belongs to the mutable relic, never its canonical model or another player.
public abstract class ArchitectItemRelic : TheArchitectRelic
{
    protected sealed class ItemData
    {
        public bool TriggeredThisTurn;
        public bool TriggeredThisCombat;
        public bool PlayedThisTurn;
        public bool PlayedEnchantedThisTurn;
        public readonly Dictionary<CardModel, int> Plays = [];
        public readonly HashSet<CardModel> EnchantedThisTurn = [];

        public void ResetTurn()
        {
            TriggeredThisTurn = PlayedThisTurn = PlayedEnchantedThisTurn = false;
            EnchantedThisTurn.Clear();
        }
    }

    private static readonly ConditionalWeakTable<ArchitectItemRelic, ItemData> States = new();
    protected ItemData Data => States.GetOrCreateValue(this);

    public override Task BeforeCombatStart()
    {
        Data.ResetTurn();
        Data.TriggeredThisCombat = false;
        Data.Plays.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner) Data.ResetTurn();
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (play.Card.Owner != Owner) return Task.CompletedTask;
        Data.PlayedThisTurn = true;
        if (ArchitectEnchantmentHelper.HasAny(play.Card))
        {
            Data.PlayedEnchantedThisTurn = true;
            Data.EnchantedThisTurn.Add(play.Card);
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner != Owner) return Task.CompletedTask;
        int count = Data.Plays.GetValueOrDefault(play.Card) + 1;
        Data.Plays[play.Card] = count;
        return OnOwnerCardPlayed(context, play, count);
    }

    protected virtual Task OnOwnerCardPlayed(PlayerChoiceContext context, CardPlay play, int count) => Task.CompletedTask;
}
