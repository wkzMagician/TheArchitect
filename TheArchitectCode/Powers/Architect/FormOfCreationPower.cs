using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class FormOfCreationPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public bool CanTriggerFor(CardModel card) => card.Owner.Creature == Owner &&
        ArchitectEnchantmentHelper.HasAny(card) &&
        ArchitectCombatState.EnchantedCardSeriesPlayedThisTurn(card.Owner) < Amount;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !ArchitectCombatState.WasEnchantedOnPlay(cardPlay.Card))
        {
            return;
        }

        // The combat hook records this play before powers inspect it.
        if (ArchitectCombatState.EnchantedCardSeriesPlayedThisTurn(cardPlay.Card.Owner) > Amount)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1, cardPlay.Card.Owner);
        await CardPileCmd.Draw(context, 1, cardPlay.Card.Owner);
    }
}
