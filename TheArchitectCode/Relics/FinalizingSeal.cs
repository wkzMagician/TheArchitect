using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class FinalizingSeal : TheArchitectRelic
{
    private int _selectedDeckIndex = -1;
    private CardModel? _selectedCard;
    public override RelicRarity Rarity => RelicRarity.Shop;
    public override bool HasUponPickupEffect => true;

    // Saves track the current deck position; live play uses identity, never a model ID.
    [SavedProperty]
    public int SelectedDeckIndex
    {
        get => _selectedCard == null ? _selectedDeckIndex : Owner.Deck.Cards.ToList().IndexOf(_selectedCard);
        set { _selectedDeckIndex = value; _selectedCard = null; }
    }

    public CardModel? SelectedCard
    {
        get
        {
            if (_selectedCard == null && _selectedDeckIndex >= 0)
            {
                _selectedCard = Owner.Deck.Cards.ElementAtOrDefault(_selectedDeckIndex);
                if (_selectedCard == null) _selectedDeckIndex = -1;
            }
            return _selectedCard != null && Owner.Deck.Cards.Contains(_selectedCard) ? _selectedCard : null;
        }
    }

    public override async Task AfterObtained()
    {
        _selectedCard = (await CardSelectCmd.FromDeckGeneric(Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c.Type != CardType.Quest)).FirstOrDefault();
        _selectedDeckIndex = _selectedCard == null ? -1 : Owner.Deck.Cards.ToList().IndexOf(_selectedCard);
    }

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        _ = SelectedCard; // Resolve after load before rewards/events can alter the deck.
        return Task.CompletedTask;
    }

    public override Task BeforeCardRemoved(CardModel card)
    {
        if (card == SelectedCard) { _selectedCard = null; _selectedDeckIndex = -1; }
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _ = SelectedCard;
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner == Owner && !play.Card.IsClone && SelectedCard is { } selected && play.Card.DeckVersion == selected && ArchitectEnchantmentHelper.HasAny(play.Card))
        {
            Flash();
            ArchitectEnchantmentHelper.Refresh(play.Card);
        }
        return Task.CompletedTask;
    }
}
