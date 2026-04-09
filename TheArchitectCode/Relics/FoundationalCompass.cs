using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class FoundationalCompass : TheArchitectRelic
{
    private bool _triggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task BeforeCombatStart()
    {
        _triggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _triggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return Task.CompletedTask;
        }

        if (_triggeredThisCombat)
        {
            return Task.CompletedTask;
        }

        _triggeredThisCombat = true;
        List<CardModel> choices = PileType.Hand.GetPile(player).Cards.Where(ArchitectEnchantmentHelper.CanTargetForRandomBasic).ToList();
        if (choices.Count == 0)
        {
            return Task.CompletedTask;
        }

        CardModel chosen = player.RunState.Rng.CombatCardSelection.NextItem(choices)!;
        ArchitectEnchantmentHelper.AddRandomBasic(player, chosen);
        Flash();
        return Task.CompletedTask;
    }
}
