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

public sealed class FateVortexPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player)
        {
            return Task.CompletedTask;
        }

        // todo: 能力不起效果，没有附魔任何一张卡
        // ? 会和 AfterEnergyReset 有关吗？ 难道不是回合开始时？
        List<CardModel> hand = ArchitectEnchantmentHelper.Hand(player).Where(ArchitectEnchantmentHelper.CanTargetForRandomBasic).ToList();
        if (hand.Count == 0)
        {
            return Task.CompletedTask;
        }

        CardModel chosen = player.RunState.Rng.CombatCardSelection.NextItem(hand)!;
        ArchitectEnchantmentHelper.AddRandomBasic(player, chosen);
        return Task.CompletedTask;
    }
}
