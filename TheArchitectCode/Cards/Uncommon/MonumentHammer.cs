using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class MonumentHammer() : TheArchitectCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new DynamicVar("Scaling", 3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal amount = DynamicVars.Damage.BaseValue + ArchitectCombatState.CardsEnchantedThisCombat(this) * DynamicVars["Scaling"].BaseValue;
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Scaling"].UpgradeValueBy(2m);
    }

    protected override string GetCombatPreviewText()
    {
        int damage = DynamicVars.Damage.IntValue + ArchitectCombatState.CardsEnchantedThisCombat(this) * DynamicVars["Scaling"].IntValue;
        return $"deals {damage} damage";
    }
}
