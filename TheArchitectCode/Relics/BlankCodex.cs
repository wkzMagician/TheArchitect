using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class BlankCodex : ArchitectItemRelic
{
    private bool _drawExtraCardsNextTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 2)];

    public override Task BeforeCombatStart()
    {
        _drawExtraCardsNextTurn = false;
        return base.BeforeCombatStart();
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        await base.AfterPlayerTurnStartEarly(context, player);
        if (player != Owner || !_drawExtraCardsNextTurn)
        {
            return;
        }

        _drawExtraCardsNextTurn = false;
        Flash();
        await CardPileCmd.Draw(context, 2, player);
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature) || Data.TriggeredThisTurn || !Data.PlayedThisTurn || Data.PlayedEnchantedThisTurn)
        {
            return Task.CompletedTask;
        }

        Data.TriggeredThisTurn = true;
        _drawExtraCardsNextTurn = true;
        Flash();
        return Task.CompletedTask;
    }
}
