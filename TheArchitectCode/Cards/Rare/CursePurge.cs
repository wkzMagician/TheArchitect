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
        List<CardModel> combatPiles = ArchitectEnchantmentHelper.Hand(Owner)
            .Concat(ArchitectEnchantmentHelper.DrawPile(Owner))
            .Concat(ArchitectEnchantmentHelper.DiscardPile(Owner))
            .Concat(Owner.PlayerCombatState!.PlayPile.Cards)
            .ToList();
        List<CardModel> curses = combatPiles
            .Where(card => card.Type == CardType.Curse)
            .Distinct()
            .ToList();
        if (curses.Count == 0)
        {
            return;
        }

        foreach (CardModel curse in curses)
        {
            await CardCmd.Exhaust(choiceContext, curse);
        }

        await PowerCmd.Apply<PlatingPower>(
            choiceContext,
            Owner.Creature,
            curses.Count * DynamicVars["Plating"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Plating"].UpgradeValueBy(1m);
    }
}
