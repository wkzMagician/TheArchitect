using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class WriteDestiny() : TheArchitectCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public static bool CanInscribe(CardModel card) => card.DeckVersion is { } deckCard
        && card.Owner.Deck.Cards.Contains(deckCard) && ArchitectEnchantmentHelper.HasAny(card);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", CanInscribe, this);
        if (card == null) return;

        EnchantmentModel selected = ArchitectEnchantmentHelper.Get(card)!;

        // Inscription copies the existing amount, rather than applying the combat multiplier again.
        CardModel deckCard = card.DeckVersion!;
        ArchitectEnchantmentHelper.Remove(deckCard);
        EnchantmentModel copy = EnchantmentModel.FromSerializable(selected.ToSerializable());
        ArchitectEnchantmentHelper.AddRaw(deckCard, copy, selected.Amount);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
