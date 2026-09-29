using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class SeedcoreCannon() : TheArchitectCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<(ArchitectEnchantKind Kind, int Amount)> StartingEnchantments => [(ArchitectEnchantKind.Sown, 3)];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(32, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sown, 3);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
