using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Potions;

public sealed class RepriseElixir : TheArchitectPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Replays", 1)];
    public override bool PassesCustomUsabilityCheck => Owner?.PlayerCombatState?.Hand.Cards.Count > 0;
    private CardModel? _selectedCard;
    protected override async Task<bool> PrepareSelection(PlayerChoiceContext context)
    {
        _selectedCard = (await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this)).FirstOrDefault();
        return _selectedCard != null;
    }

    protected override Task OnUse(PlayerChoiceContext context, Creature? target)
    {
        ArchitectCombatState.SetPotionReplays(_selectedCard!, DynamicVars["Replays"].IntValue);
        _selectedCard = null;
        return Task.CompletedTask;
    }
}
