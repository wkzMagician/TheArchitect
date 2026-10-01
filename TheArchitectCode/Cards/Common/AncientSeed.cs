using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class AncientSeed() : TheArchitectCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => IsMutable && CombatState != null && (ArchitectEnchantmentHelper.HasInactive(this));

    protected override bool ShufflesAfterPlay => IsUpgraded || base.ShufflesAfterPlay;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RefreshEnchantments]);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(1, ValueProp.Move), new BlockVar(1, ValueProp.Move), new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card == this)
        {
            ArchitectEnchantmentHelper.Refresh(this);
        }

        return Task.CompletedTask;
    }

}
