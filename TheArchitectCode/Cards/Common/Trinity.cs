using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class Trinity() : TheArchitectCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => IsMutable && CombatState != null && (ArchitectCombatState.TimesPlayed(this) >= 2);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new DynamicVar("BigDamage", 30)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal amount = ArchitectCombatState.TimesPlayed(this) >= 2 ? DynamicVars["BigDamage"].BaseValue : DynamicVars.Damage.BaseValue;
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BigDamage"].UpgradeValueBy(10m);
    }

    protected override string GetCombatPreviewText()
    {
        return GetLocalizedCombatDamagePreview("THEARCHITECT-TRINITY.combatPreview",
            ArchitectCombatState.TimesPlayed(this) >= 2 ? DynamicVars["BigDamage"].BaseValue : DynamicVars.Damage.BaseValue);
    }
}
