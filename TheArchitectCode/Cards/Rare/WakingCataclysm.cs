using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class WakingCataclysm() : TheArchitectCard(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(32, ValueProp.Move)];

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterAutoPrePlayPhaseEntered(choiceContext, player);
        if (player != Owner || CombatState == null || !ArchitectEnchantmentHelper.HasAny(this))
        {
            return;
        }

        // Let native enchantments (such as Imbued) resolve first. If one of
        // them already auto-played this card, do not play it a second time.
        if (TimesPlayedThisCombat == 0)
        {
            await CardCmd.AutoPlay(choiceContext, this, null);
        }

        ArchitectEnchantmentHelper.RemoveAll([this]);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
