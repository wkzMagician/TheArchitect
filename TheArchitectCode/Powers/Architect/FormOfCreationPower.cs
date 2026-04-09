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
    private sealed class Data
    {
        public int TriggeredThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterEnergyReset(Player player)
    {
        if (player == Owner.Player)
        {
            GetInternalData<Data>().TriggeredThisTurn = 0;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !ArchitectEnchantmentHelper.HasAny(cardPlay.Card))
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (data.TriggeredThisTurn >= Amount)
        {
            return;
        }

        data.TriggeredThisTurn++;
        await PlayerCmd.GainEnergy(1, cardPlay.Card.Owner);
        await CardPileCmd.Draw(context, 1, cardPlay.Card.Owner);
    }
}
