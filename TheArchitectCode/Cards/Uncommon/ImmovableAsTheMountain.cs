using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

// todo: 英文名字太长了
// todo: ImmovableAsTheMountainPower 的回合结束触发逻辑调用了选择界面，但缺少对应的 prompt 配置，导致结束回合时报错。
public sealed class ImmovableAsTheMountain() : TheArchitectCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Stable, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ImmovableAsTheMountainPower>(Owner.Creature, DynamicVars["Cards"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
