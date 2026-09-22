using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Basic;

public sealed class Sigilbreaker() : TheArchitectCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
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
        return Owner == null ? string.Empty : ArchitectEnchantmentHelper.DescribeSigilbreakerDamage(this, PileType.Hand.GetPile(Owner).Cards);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        int enchantedInHand = Owner == null ? 0 : ArchitectEnchantmentHelper.CountOtherEnchantedCards(this, PileType.Hand.GetPile(Owner).Cards);
        int damage = DynamicVars.Damage.IntValue + enchantedInHand * DynamicVars["BonusDamage"].IntValue;
        description.Add("DynamicDamage", damage);
    }
}
