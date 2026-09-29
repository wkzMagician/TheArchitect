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

public sealed class SpearOfSacrifice() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        List<CardModel> affectedCards = ArchitectEnchantmentHelper.Hand(Owner)
            .Where(card => card != this && ArchitectEnchantmentHelper.HasAny(card))
            .ToList();
        int removed = 0;
        foreach (CardModel card in affectedCards)
        {
            if (ArchitectEnchantmentHelper.Remove(card)) removed++;
        }

        List<Creature> enemies = CombatState!.HittableEnemies.ToList();
        for (int i = 0; i < removed && enemies.Count > 0; i++)
        {
            await ArchitectEnchantmentHelper.Attack(this, choiceContext, Owner.RunState.Rng.CombatTargets.NextItem(enemies), DynamicVars.Damage.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override string GetCombatPreviewText()
    {
        int removed = ArchitectEnchantmentHelper.Hand(Owner)
            .Count(card => card != this && ArchitectEnchantmentHelper.HasAny(card));
        return GetLocalizedCombatPreview("THEARCHITECT-SPEAR_OF_SACRIFICE.combatPreview", ("Hits", removed));
    }
}
