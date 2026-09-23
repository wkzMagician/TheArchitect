using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class ReflowInkwell : ArchitectItemRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature) || Data.TriggeredThisTurn || Data.EnchantedThisTurn.Count < 3) return;
        List<CardModel> choices = PileType.Discard.GetPile(Owner).Cards.Where(ArchitectEnchantmentHelper.HasAny).ToList();
        if (choices.Count == 0) return;
        Data.TriggeredThisTurn = true;
        CardModel chosen = Owner.RunState.Rng.CombatCardSelection.NextItem(choices)!;
        Flash();
        await CardPileCmd.Add(chosen, PileType.Draw, CardPilePosition.Top);
    }
}
