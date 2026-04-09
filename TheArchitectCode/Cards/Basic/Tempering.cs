using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using TheArchitect.TheArchitectCode.Helpers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.TheArchitectCode.Cards.Basic;

public sealed class Tempering() : TheArchitectCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    private abstract class TemperingChoiceCard(ArchitectEnchantKind kind) : CustomCardModel(-1, CardType.Skill, CardRarity.Token, TargetType.None)
    {
        public ArchitectEnchantKind Kind { get; } = kind;

        public override string PortraitPath => ResourceLoader.Exists("card.png".CardImagePath()) ? "card.png".CardImagePath() : string.Empty;

        public override string CustomPortraitPath => ResourceLoader.Exists("card.png".BigCardImagePath()) ? "card.png".BigCardImagePath() : string.Empty;

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
        {
            return Task.CompletedTask;
        }
    }

    [Pool(typeof(TheArchitectTokenPool))]
    private sealed class TemperingSharpChoice() : TemperingChoiceCard(ArchitectEnchantKind.Sharp);

    [Pool(typeof(TheArchitectTokenPool))]
    private sealed class TemperingNimbleChoice() : TemperingChoiceCard(ArchitectEnchantKind.Nimble);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("EnchantAmount", 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["EnchantAmount"].IntValue)
            .Concat(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, DynamicVars["EnchantAmount"].IntValue));

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

        List<CardModel> choiceCards = options.Select(kind => kind switch
        {
            ArchitectEnchantKind.Sharp => (CardModel)new TemperingSharpChoice { Owner = Owner },
            ArchitectEnchantKind.Nimble => new TemperingNimbleChoice { Owner = Owner },
            _ => throw new InvalidOperationException($"Unsupported Tempering option {kind}")
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
