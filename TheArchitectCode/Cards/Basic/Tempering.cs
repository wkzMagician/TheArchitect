using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Basic;

public sealed class Tempering() : TheArchitectCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("EnchantAmount", 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["EnchantAmount"].IntValue)
            .Concat(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, DynamicVars["EnchantAmount"].IntValue));

    private static TemperingChoiceCard CreateChoiceCard(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Sharp => (TemperingChoiceCard)ModelDb.Card<TemperingSharpChoice>().ToMutable(),
            ArchitectEnchantKind.Nimble => (TemperingChoiceCard)ModelDb.Card<TemperingNimbleChoice>().ToMutable(),
            _ => throw new InvalidOperationException($"Unsupported Tempering option {kind}")
        };
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPrompt",
            ArchitectEnchantmentHelper.CanTargetWithTempering,
            this);

        if (card == null)
        {
            return;
        }

        IReadOnlyList<ArchitectEnchantKind> options = ArchitectEnchantmentHelper.TemperingOptionsFor(card);
        if (options.Count == 1)
        {
            ArchitectEnchantmentHelper.Add(card, options[0], DynamicVars["EnchantAmount"].IntValue);
            return;
        }

        List<CardModel> choiceCards = options.Select(kind =>
        {
            CardModel choice = CreateChoiceCard(kind);
            choice.Owner = Owner;
            return choice;
        }).ToList();
        TemperingChoiceCard? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, choiceCards, Owner) as TemperingChoiceCard;
        if (selected == null)
        {
            return;
        }

        ArchitectEnchantmentHelper.Add(card, selected.Kind, DynamicVars["EnchantAmount"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnchantAmount"].UpgradeValueBy(2m);
    }
}
