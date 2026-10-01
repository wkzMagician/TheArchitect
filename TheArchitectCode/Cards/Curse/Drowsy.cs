using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Extensions;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

[Pool(typeof(TheArchitectTokenPool))]
public sealed class Drowsy : CustomCardModel
{
    public override MegaCrit.Sts2.Core.Models.CardPoolModel Pool => MegaCrit.Sts2.Core.Models.ModelDb.CardPool<TheArchitectTokenPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    public override string PortraitPath => "drowsy.png".CardImagePath();

    public override string CustomPortraitPath => "drowsy.png".BigCardImagePath();

    public Drowsy() : base(1, CardType.Curse, CardRarity.Token, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        _playedThisCombat = true;
        return DeckVersion != null && Owner.Deck.Cards.Contains(DeckVersion)
            ? CardPileCmd.RemoveFromDeck(DeckVersion)
            : Task.CompletedTask;
    }

    private bool _playedThisCombat;
    private bool _persistedAfterCombatEnd;
    public void PreventPersistence() => _playedThisCombat = true;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_persistedAfterCombatEnd || Owner == null || _playedThisCombat || Pile?.Type == PileType.Exhaust || Owner.Deck.Cards.Contains(this) || (DeckVersion != null && Owner.Deck.Cards.Contains(DeckVersion)))
        {
            return Task.CompletedTask;
        }

        _persistedAfterCombatEnd = true;

        // Combat cards belong to the combat card scope. Create a distinct
        // card in the run scope before moving a persistent copy into the deck.
        CardModel persistentCopy = Owner.RunState.CreateCard(ModelDb.Card<Drowsy>(), Owner);
        return CardPileCmd.Add(persistentCopy, PileType.Deck);
    }
}
