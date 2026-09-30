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

public sealed class ShieldOfSacrifice() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int removed = ArchitectEnchantmentHelper.RemoveAll(
            ArchitectEnchantmentHelper.Hand(Owner).Where(card => !ReferenceEquals(card, this)));
        for (int i = 0; i < removed; i++)
        {
            await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override string GetCombatPreviewText()
    {
        int removed = ArchitectEnchantmentHelper.CountOtherEnchantedCards(this, ArchitectEnchantmentHelper.Hand(Owner));
        BlockVar block = new(DynamicVars.Block.BaseValue, DynamicVars.Block.Props);
        block.UpdateCardPreview(this, CardPreviewMode.Normal, null,
            IsMutable && CombatState != null && Pile?.Type is PileType.Hand or PileType.Play);
        return GetLocalizedCombatPreview("THEARCHITECT-SHIELD_OF_SACRIFICE.combatPreview",
            ("Block", Math.Max(0, (int)block.PreviewValue) * removed));
    }
}
