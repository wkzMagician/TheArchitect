using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Potions;

public sealed class RevisionSolvent : TheArchitectPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new BlockVar(4, ValueProp.Unpowered)];
    public override bool PassesCustomUsabilityCheck => Owner?.PlayerCombatState?.Hand.Cards.Any(ArchitectEnchantmentHelper.HasAny) == true;

    private CardModel[] _selectedCards = [];
    protected override async Task<bool> PrepareSelection(PlayerChoiceContext context)
    {
        _selectedCards = (await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, 3), ArchitectEnchantmentHelper.HasAny, this)).Distinct().ToArray();
        return _selectedCards.Length > 0;
    }

    protected override async Task OnUse(PlayerChoiceContext context, Creature? target)
    {
        int removed = ArchitectEnchantmentHelper.RemoveAll(_selectedCards);
        _selectedCards = [];
        await ArchitectEffectQueue.Drain(Owner);
        await CreatureCmd.GainBlock(Owner.Creature, removed * DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
        await CardPileCmd.Draw(context, removed * DynamicVars.Cards.IntValue, Owner);
    }
}
