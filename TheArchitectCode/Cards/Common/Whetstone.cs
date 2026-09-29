using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class Whetstone() : TheArchitectCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move),
        new DynamicVar("Sharp", 3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["Sharp"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue);
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", target => ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(target, ArchitectEnchantKind.Sharp), this);
        if (card != null)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, DynamicVars["Sharp"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Sharp"].UpgradeValueBy(2m);
    }
}
