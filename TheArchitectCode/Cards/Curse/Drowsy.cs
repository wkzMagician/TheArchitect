using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Extensions;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

[Pool(typeof(TheArchitectTokenPool))]
public sealed class Drowsy : CustomCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

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
    public void PreventPersistence() => _playedThisCombat = true;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner == null || _playedThisCombat || Pile?.Type == PileType.Exhaust || Owner.Deck.Cards.Contains(this) || (DeckVersion != null && Owner.Deck.Cards.Contains(DeckVersion)))
        {
            return Task.CompletedTask;
        }

        return CardPileCmd.Add(this, PileType.Deck);
    }
}
