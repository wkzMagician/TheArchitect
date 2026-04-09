using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Extensions;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

[Pool(typeof(TheArchitectTokenPool))]
public sealed class Drowsy : CustomCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];

    public override string PortraitPath => ResourceLoader.Exists("strike_architect.png".CardImagePath()) ? "strike_architect.png".CardImagePath() : string.Empty;

    public override string CustomPortraitPath => ResourceLoader.Exists("strike_architect.png".BigCardImagePath()) ? "strike_architect.png".BigCardImagePath() : string.Empty;

    public Drowsy() : base(-1, CardType.Status, CardRarity.Token, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return Task.CompletedTask;
    }
}
