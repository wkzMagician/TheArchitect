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

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class DivineHammerfall() : TheArchitectCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => IsMutable && CombatState != null && (ArchitectEnchantmentHelper.HasAny(this));

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    ArchitectKeywordHoverTips.IncludeEnchant([
        ArchitectKeywordHoverTips.RemoveEnchantments,
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ]);
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        if (!ArchitectEnchantmentHelper.HasAny(this))
        {
            return;
        }

        if (play.Target != null)
        {
            await ArchitectEnchantmentHelper.ApplyWeak(choiceContext, play.Target, 2m, Owner.Creature, this);
            await ArchitectEnchantmentHelper.ApplyVulnerable(choiceContext, play.Target, 2m, Owner.Creature, this);
        }
        ArchitectEnchantmentHelper.Remove(this);
    }


    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }

    protected override string GetCombatPreviewText()
    {
        return ArchitectEnchantmentHelper.HasAny(this)
            ? GetLocalizedCombatPreview("THEARCHITECT-DIVINE_HAMMERFALL.combatPreview")
            : string.Empty;
    }
}
