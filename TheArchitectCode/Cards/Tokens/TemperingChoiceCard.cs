using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

// Token-only UI choice cards used by Tempering.
public abstract class TemperingChoiceCard(ArchitectEnchantKind kind) : TheArchitectCard(-1, CardType.Skill, CardRarity.Token, TargetType.None)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];

    public override MegaCrit.Sts2.Core.Models.CardPoolModel Pool => MegaCrit.Sts2.Core.Models.ModelDb.CardPool<TheArchitectTokenPool>();

    public ArchitectEnchantKind Kind { get; } = kind;


    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TheArchitectTokenPool))]
public sealed class TemperingSharpChoice() : TemperingChoiceCard(ArchitectEnchantKind.Sharp);

[Pool(typeof(TheArchitectTokenPool))]
public sealed class TemperingNimbleChoice() : TemperingChoiceCard(ArchitectEnchantKind.Nimble);
