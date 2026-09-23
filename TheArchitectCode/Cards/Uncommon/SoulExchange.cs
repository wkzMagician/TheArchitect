using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class SoulExchange() : TheArchitectCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? source = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPromptSource",
            ArchitectEnchantmentHelper.HasAny,
            this);
        if (source == null)
        {
            return;
        }

        CardModel? target = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPromptTarget",
            card => card != source && ArchitectEnchantmentHelper.CanReceiveTransferredEnchantments(card, source),
            this);
        if (target == null)
        {
            return;
        }

        ArchitectEnchantmentHelper.Transfer(source, target);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
