using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Helpers;

public static class ArchitectCardSelectionHelper
{
    public static async Task<CardModel?> ChooseFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter, AbstractModel source)
    {
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(new LocString("cards", promptKey), 1),
            filter,
            source)).FirstOrDefault();
    }

    public static async Task<CardModel?> ChooseFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Draw.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), 1))).FirstOrDefault();
    }

    public static async Task<CardModel?> ChooseFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Discard.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), 1))).FirstOrDefault();
    }

    public static async Task<List<CardModel>> ChooseManyFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter, AbstractModel source)
    {
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(new LocString("cards", promptKey), min, max),
            filter,
            source)).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Draw.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDeck(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Deck.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Discard.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
    }
}
