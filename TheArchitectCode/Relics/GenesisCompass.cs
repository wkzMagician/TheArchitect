using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class GenesisCompass : TheArchitectRelic
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

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || _triggeredThisCombat)
        {
            return Task.CompletedTask;
        }

        _triggeredThisCombat = true;
        // Snapshot the starting hand so effects triggered by enchanting cannot
        // extend the set of cards that receive this once-per-combat reward.
        var cards = PileType.Hand.GetPile(player).Cards
            .Where(ArchitectEnchantmentHelper.CanTargetForRandomBasic).ToList();
        bool enchanted = false;
        foreach (var card in cards)
        {
            enchanted |= ArchitectEnchantmentHelper.AddRandomBasic(player, card) != null;
        }
        if (enchanted)
        {
            Flash();
        }
        return Task.CompletedTask;
    }
}
