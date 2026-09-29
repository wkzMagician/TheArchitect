using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class MonumentHammer() : TheArchitectCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new DynamicVar("Scaling", 5)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal amount = DynamicVars.Damage.BaseValue + ArchitectCombatState.CardsEnchantedThisCombat(this) * DynamicVars["Scaling"].BaseValue;
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Scaling"].UpgradeValueBy(3m);
    }

    protected override string GetCombatPreviewText()
    {
        return GetLocalizedCombatDamagePreview("THEARCHITECT-MONUMENT_HAMMER.combatPreview",
            DynamicVars.Damage.BaseValue + ArchitectCombatState.CardsEnchantedThisCombat(this) * DynamicVars["Scaling"].BaseValue);
    }
}
