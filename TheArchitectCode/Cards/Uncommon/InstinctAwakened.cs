using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class InstinctAwakened() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Instinct, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPrompt",
            static card => ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(card, ArchitectEnchantKind.Instinct),
            this);
        if (card != null)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Instinct, 1m);
        }
    }
}
