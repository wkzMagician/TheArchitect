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

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class JacobsLadder() : TheArchitectCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Slither, 1));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", target => ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(target, ArchitectEnchantKind.Slither), this);
        if (card != null)
        {
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Slither, 1m);
        }
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
