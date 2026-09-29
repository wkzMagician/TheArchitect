using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class Fallen() : TheArchitectCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        List<CardModel> enchantedCards = ArchitectEnchantmentHelper.Hand(Owner)
            .Where(ArchitectEnchantmentHelper.HasAny)
            .ToList();

        if (enchantedCards.Count > 0)
        {
            CardModel target = Owner.RunState.Rng.CombatCardSelection.NextItem(enchantedCards)!;
            ArchitectEnchantmentHelper.Remove(target);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}
