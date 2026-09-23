using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Potions;

public sealed class InscriptionPotion : TheArchitectPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("EnchantAmount", 6)];
    public override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["EnchantAmount"].IntValue)
        .Concat(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, DynamicVars["EnchantAmount"].IntValue));
    public override bool PassesCustomUsabilityCheck => Owner?.PlayerCombatState?.Hand.Cards.Any(ArchitectEnchantmentHelper.CanTargetWithTempering) == true;

    private CardModel? _selectedCard;
    private ArchitectEnchantKind _selectedKind;

    protected override async Task<bool> PrepareSelection(PlayerChoiceContext context)
    {
        CardModel? card = (await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), ArchitectEnchantmentHelper.CanTargetWithTempering, this)).FirstOrDefault();
        if (card == null) return false;
        IReadOnlyList<ArchitectEnchantKind> options = ArchitectEnchantmentHelper.TemperingOptionsFor(card);
        if (options.Count == 0) return false;
        ArchitectEnchantKind kind = options[0];
        if (options.Count > 1)
        {
            List<CardModel> choices = options.Select(option =>
            {
                EnchantChoiceCard choice = (EnchantChoiceCard)Owner.Creature.CombatState!.CreateCard(ModelDb.Card<EnchantChoiceCard>(), Owner);
                choice.SetOption(new ArchitectEnchantOption(option, DynamicVars["EnchantAmount"].IntValue));
                return (CardModel)choice;
            }).ToList();
            if (await CardSelectCmd.FromChooseACardScreen(context, choices, Owner) is not EnchantChoiceCard selected) return false;
            kind = selected.Option.Kind;
        }
        _selectedCard = card;
        _selectedKind = kind;
        return true;
    }

    protected override async Task OnUse(PlayerChoiceContext context, Creature? target)
    {
        ArchitectEnchantmentHelper.Add(_selectedCard!, _selectedKind, DynamicVars["EnchantAmount"].BaseValue);
        await ArchitectEffectQueue.Drain(Owner);
        _selectedCard = null;
    }
}
