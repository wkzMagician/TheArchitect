using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Ancient;

// Hidden ancient-upgrade variant of Sigilbreaker. It is intentionally excluded from reward pools.
public sealed class AncientVerdict() : TheArchitectCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move), new DynamicVar("BonusDamage", 8)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal amount = DynamicVars.Damage.BaseValue + DynamicVars["BonusDamage"].BaseValue * ArchitectEnchantmentHelper.CountEnchantedCardsInHand(Owner);
        return ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDamage"].UpgradeValueBy(4m);
    }

    protected override string GetCombatPreviewText()
    {
        int damage = DynamicVars.Damage.IntValue + DynamicVars["BonusDamage"].IntValue * ArchitectEnchantmentHelper.CountEnchantedCardsInHand(Owner);
        return $"deals {damage} damage";
    }
}
