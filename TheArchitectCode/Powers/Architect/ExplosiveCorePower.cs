using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class ExplosiveCorePower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // This hook runs after the card's play effects, including its
        // enchantment effects, have resolved.
        if (cardPlay.Card.Owner.Creature == Owner)
            ArchitectEnchantmentHelper.Remove(cardPlay.Card);
        return Task.CompletedTask;
    }

    public async Task OnEnchantmentsRemoved(CardModel card, int count)
    {
        for (int i = 0; i < count; i++)
        {
            List<Creature> enemies = card.CombatState?.HittableEnemies.ToList() ?? [];
            if (enemies.Count == 0)
                break;

            Creature target = Owner.Player!.RunState.Rng.CombatTargets.NextItem(enemies)!;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, Amount, ValueProp.Unpowered, Owner, null);
        }
    }
}
