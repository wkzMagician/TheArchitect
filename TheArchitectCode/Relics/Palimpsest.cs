using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class Palimpsest : ArchitectItemRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    protected override async Task OnOwnerCardPlayed(PlayerChoiceContext context, CardPlay play, int count)
    {
        if (count != 3 || Data.TriggeredThisCombat) return;
        Data.TriggeredThisCombat = true;
        Flash();
        await PlayerCmd.GainEnergy(2, Owner);
        await CardPileCmd.Draw(context, 2, Owner);
    }
}
