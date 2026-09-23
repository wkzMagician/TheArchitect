using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class DismantlingPliers : ArchitectItemRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public async Task OnActiveRemoval(CardModel card)
    {
        if (card.Owner != Owner || card.CombatState == null || card.Pile?.Type == PileType.Deck || Data.TriggeredThisTurn) return;
        Data.TriggeredThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(1, Owner);
    }
}
