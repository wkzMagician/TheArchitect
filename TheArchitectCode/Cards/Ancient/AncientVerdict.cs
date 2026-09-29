using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Ancient;

// Ancient card that is part of the Architect's visible card pool.
[Pool(typeof(TheArchitectCardPool))]
public sealed class AncientVerdict() : TheArchitectCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move), new DynamicVar("BonusDamage", 4)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal amount = DynamicVars.Damage.BaseValue + DynamicVars["BonusDamage"].BaseValue * ArchitectEnchantmentHelper.CountEnchantedCardsInCombatPiles(Owner);
        return ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }

    protected override string GetCombatPreviewText()
    {
        return GetLocalizedCombatDamagePreview("THEARCHITECT-ANCIENT_VERDICT.combatPreview",
            DynamicVars.Damage.BaseValue + DynamicVars["BonusDamage"].BaseValue * ArchitectEnchantmentHelper.CountEnchantedCardsInCombatPiles(Owner));
    }
}
