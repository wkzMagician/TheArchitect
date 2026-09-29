using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Basic;

public sealed class Sigilbreaker() : TheArchitectCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("BonusDamage", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int enchantedInHand = ArchitectEnchantmentHelper.CountOtherEnchantedCards(this, PileType.Hand.GetPile(Owner).Cards);
        decimal damage = DynamicVars.Damage.BaseValue + enchantedInHand * DynamicVars["BonusDamage"].BaseValue;
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }

    protected override string GetCombatPreviewText()
    {
        return Owner?.PlayerCombatState == null ? string.Empty : ArchitectEnchantmentHelper.DescribeSigilbreakerDamage(this, Owner.PlayerCombatState.Hand.Cards);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        // The compendium formats descriptions on the canonical model, which has no combat owner.
        // Use base damage there and only inspect hand/strength on a mutable combat card.
        if (!IsMutable || Owner?.PlayerCombatState == null)
        {
            description.Add(ArchitectEnchantmentHelper.PreviewAttackDamageVar(this,
                DynamicVars.Damage.BaseValue, "DynamicDamage"));
            return;
        }

        int enchantedInHand = ArchitectEnchantmentHelper.CountOtherEnchantedCards(this, Owner.PlayerCombatState.Hand.Cards);
        description.Add(ArchitectEnchantmentHelper.PreviewAttackDamageVar(this,
            DynamicVars.Damage.BaseValue + enchantedInHand * DynamicVars["BonusDamage"].BaseValue,
            "DynamicDamage"));
    }
}
