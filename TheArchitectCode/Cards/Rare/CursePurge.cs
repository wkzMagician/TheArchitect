using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class CursePurge() : TheArchitectCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Plating", 3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        List<CardModel> curses = ArchitectEnchantmentHelper.AllPlayerCards(Owner)
            .Where(card => card.Type == CardType.Curse)
            .Distinct()
            .ToList();
        if (curses.Count == 0)
        {
            return;
        }

        int removedCount = curses.Select(card => card.DeckVersion ?? card).Distinct().Count();
        foreach (CardModel curse in curses)
        {
            if (curse is Drowsy drowsy) drowsy.PreventPersistence();
            if (curse.Pile?.Type != PileType.Deck)
                await CardPileCmd.RemoveFromCombat(curse);
        }

        await CardPileCmd.RemoveFromDeck(curses.Where(card => Owner.Deck.Cards.Contains(card)).ToList());
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, removedCount * DynamicVars["Plating"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Plating"].UpgradeValueBy(1m);
    }
}
