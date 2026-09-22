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

public sealed class SharedSanctumPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants)
    {
        return BeforeTurnEnd(choiceContext, side);
    }

    public Task BeforeTurnEnd(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.CombatSide side)
    {
        if (side != Owner.Side || CombatState == null)
        {
            return Task.CompletedTask;
        }

        foreach (Player player in CombatState.PlayerCreatures.Where(creature => creature.IsAlive).Select(creature => creature.Player!).Where(player => player != null))
        {
            List<CardModel> hand = ArchitectEnchantmentHelper.Hand(player).Where(ArchitectEnchantmentHelper.CanTargetForRandomEnchant).ToList();
            if (hand.Count == 0)
            {
                continue;
            }

            CardModel chosen = player.RunState.Rng.CombatCardSelection.NextItem(hand)!;
            ArchitectEnchantmentHelper.AddRandomCompatible(Owner.Player!, chosen);
        }

        return Task.CompletedTask;
    }
}
