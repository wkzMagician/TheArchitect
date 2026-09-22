using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class AncientSeed() : TheArchitectCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2, ValueProp.Move), new BlockVar(2, ValueProp.Move), new DynamicVar("EnchantBonus", 2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal bonus = ArchitectEnchantmentHelper.HasAny(this) ? DynamicVars["EnchantBonus"].BaseValue : 0;
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue + bonus);
        await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue + bonus);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(1m);
        DynamicVars["EnchantBonus"].UpgradeValueBy(1m);
    }
}
