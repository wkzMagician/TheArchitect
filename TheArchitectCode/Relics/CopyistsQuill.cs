using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class CopyistsQuill : ArchitectItemRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    protected override async Task OnOwnerCardPlayed(PlayerChoiceContext context, CardPlay play, int count)
    {
        if (count < 2 || Data.TriggeredThisTurn) return;
        Data.TriggeredThisTurn = true;
        Flash();
        await CardPileCmd.Draw(context, 1, Owner);
    }
}
