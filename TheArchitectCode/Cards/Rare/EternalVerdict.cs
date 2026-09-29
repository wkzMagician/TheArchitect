using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class EternalVerdict() : TheArchitectCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RefreshEnchantments]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        await PowerCmd.Apply<RefreshNextCardEnchantmentPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
