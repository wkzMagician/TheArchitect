using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class RadiantMight() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new DynamicVar("Sharp", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["Sharp"].IntValue);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        ArchitectEnchantmentHelper.EnchantAll(ArchitectEnchantmentHelper.Hand(Owner), ArchitectEnchantKind.Sharp, DynamicVars["Sharp"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Sharp"].UpgradeValueBy(1m);
    }
}
