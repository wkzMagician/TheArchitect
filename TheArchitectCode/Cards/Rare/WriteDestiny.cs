using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class WriteDestiny() : TheArchitectCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public static bool CanInscribe(CardModel card) => card.DeckVersion is { } deckCard
        && card.Owner.Deck.Cards.Contains(deckCard) && ArchitectEnchantmentHelper.HasAny(card);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", CanInscribe, this);
        if (card == null) return;

        IReadOnlyList<EnchantmentModel> enchantments = ArchitectEnchantmentHelper.GetAll(card);
        EnchantmentModel selected = enchantments[0];
        if (enchantments.Count > 1)
        {
            List<CardModel> choices = enchantments.Select(enchantment =>
            {
                EnchantChoiceCard choice = (EnchantChoiceCard)CombatState!.CreateCard(ModelDb.Card<EnchantChoiceCard>(), Owner);
                choice.SetEnchantment(enchantment);
                return (CardModel)choice;
            }).ToList();
            if (await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner) is not EnchantChoiceCard choice) return;
            selected = choice.SelectedEnchantment!;
        }

        // Inscription copies the existing amount, rather than applying the combat multiplier again.
        CardModel deckCard = card.DeckVersion!;
        CardCmd.ClearEnchantment(deckCard);
        EnchantmentModel copy = EnchantmentModel.FromSerializable(selected.ToSerializable());
        CardCmd.Enchant(copy, deckCard, selected.Amount);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
