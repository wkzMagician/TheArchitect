using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class RefreshNextCardEnchantmentPower : TheArchitectPower
{
    private sealed class Data
    {
        public bool InitialCardObserved;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    protected override object InitInternalData() => new Data();

    public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (!data.InitialCardObserved)
        {
            data.InitialCardObserved = true;
            return;
        }

        ArchitectEnchantmentHelper.Refresh(cardPlay.Card, triggerAutomaton: false);
        await PowerCmd.Remove(this);
    }
}
