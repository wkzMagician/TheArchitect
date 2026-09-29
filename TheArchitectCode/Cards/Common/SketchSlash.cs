using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class SketchSlash() : TheArchitectCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Swift, IsUpgraded ? 2 : 1));

    protected override IEnumerable<(ArchitectEnchantKind Kind, int Amount)> StartingEnchantments => [];

    protected override void ApplyStartingEnchantments()
    {
        if (!ArchitectEnchantmentHelper.HasAny(this))
        {
            ApplyStartingEnchantment(ArchitectEnchantKind.Swift, IsUpgraded ? 2 : 1);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

}
