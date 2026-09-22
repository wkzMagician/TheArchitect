using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class DivineSelection() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPrompt",
            ArchitectEnchantmentHelper.CanTargetForRandomEnchant,
            this);
        if (card == null)
        {
            return;
        }

        List<ArchitectEnchantOption> options = ArchitectEnchantmentHelper.CompatibleEnchantOptionsFor(card)
            .Take(3)
            .ToList();
        if (options.Count == 0)
        {
            return;
        }

        List<CardModel> choices = options.Select(option =>
        {
            EnchantChoiceCard choice = (EnchantChoiceCard)Owner.Creature.CombatState!.CreateCard(ModelDb.Card<EnchantChoiceCard>(), Owner);
            choice.SetOption(option);
            return (CardModel)choice;
        }).ToList();
        EnchantChoiceCard? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner) as EnchantChoiceCard;
        if (selected != null)
        {
            ArchitectEnchantmentHelper.Add(card, selected.Option.Kind, selected.Option.Amount);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
