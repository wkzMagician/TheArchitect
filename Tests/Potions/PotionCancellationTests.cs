using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Potions;

namespace TheArchitect.Tests.Potions;

public static class PotionCancellationTests
{
    private static PotionModel? _observed;
    private static int _beforeCalls;
    private static int _afterCalls;
    private static void Before(PotionModel potion) { if (potion == _observed) _beforeCalls++; }
    private static void After(PotionModel potion) { if (potion == _observed) _afterCalls++; }

    [ArchitectTest]
    public static async Task CancellingSelectionNeitherConsumesBottleNorDispatchesUsageHooks()
    {
        using CombatTestContext ctx = new();
        var card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        var potion = ctx.Potion<RevisionSolvent>();
        _observed = potion; _beforeCalls = _afterCalls = 0;
        var harmony = new Harmony("TheArchitect.Tests.PotionCancellation");
        var before = AccessTools.Method(typeof(Hook), nameof(Hook.BeforePotionUsed));
        var after = AccessTools.Method(typeof(Hook), nameof(Hook.AfterPotionUsed));
        harmony.Patch(before, prefix: new HarmonyMethod(typeof(PotionCancellationTests), nameof(Before)));
        harmony.Patch(after, prefix: new HarmonyMethod(typeof(PotionCancellationTests), nameof(After)));
        try
        {
            typeof(PotionModel).GetProperty(nameof(PotionModel.IsQueued))!.SetValue(potion, true);
            ctx.Select();
            await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
            AssertEx.True(ctx.Player.Potions.Contains(potion), "Bottle remains in inventory");
            AssertEx.False(potion.IsQueued, "Cancellation clears queued flag so bottle is usable again");
            AssertEx.Equal(0, _beforeCalls, "Cancelled selection cannot trigger before-use relic effects");
            AssertEx.Equal(0, _afterCalls, "Cancelled selection cannot farm after-use effects");
            ctx.CardInDraw<MegaCrit.Sts2.Core.Models.Cards.Mocks.MockSkillCard>();
            ctx.Select(card);
            await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
            AssertEx.Equal(1, _beforeCalls, "Successful use dispatches before hook once");
            AssertEx.Equal(1, _afterCalls, "Successful use dispatches after hook once");
            AssertEx.False(ctx.Player.Potions.Contains(potion), "Successful use consumes bottle");
        }
        finally
        {
            harmony.Unpatch(before, HarmonyPatchType.Prefix, harmony.Id);
            harmony.Unpatch(after, HarmonyPatchType.Prefix, harmony.Id);
            _observed = null;
        }
    }
}
