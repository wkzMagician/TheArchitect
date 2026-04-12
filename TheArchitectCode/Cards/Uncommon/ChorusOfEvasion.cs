using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class ChorusOfEvasion() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move), new DynamicVar("Nimble", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, DynamicVars["Nimble"].IntValue);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue);
        ArchitectEnchantmentHelper.EnchantAll(ArchitectEnchantmentHelper.Hand(Owner), ArchitectEnchantKind.Nimble, DynamicVars["Nimble"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Nimble"].UpgradeValueBy(1m);
    }
}
