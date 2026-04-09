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

public sealed class OmnipotencePower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player)
        {
            return Task.CompletedTask;
        }

        List<CardModel> choices = PileType.Hand.GetPile(player).Cards.Where(ArchitectEnchantmentHelper.CanTargetForRandomBasic).ToList();
        if (choices.Count == 0)
        {
            return Task.CompletedTask;
        }

        for (int i = 0; i < Amount && choices.Count > 0; i++)
        {
            CardModel card = player.RunState.Rng.CombatCardSelection.NextItem(choices)!;
            choices.Remove(card);
            ArchitectEnchantmentHelper.AddRandomBasic(player, card);
        }

        return Task.CompletedTask;
    }
}
