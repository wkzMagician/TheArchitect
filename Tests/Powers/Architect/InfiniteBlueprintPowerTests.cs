using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Powers.Architect;

public static class InfiniteBlueprintPowerTests
{
    [ArchitectTest]
    public static async Task GuardtraceUsesTheSameApplicationMultiplier()
    {
        using CombatTestContext ctx = new();
        await ctx.ApplyPower<InfiniteBlueprintPower>();
        var card = ctx.CardInHand<Guardtrace>();
        var target = ctx.CardInHand<StrikeArchitect>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Adroit>(target), "Guardtrace applies Adroit");
        AssertEx.Equal(8, CombatTestContext.EnchantAmount(target), "Guardtrace's four stacks are doubled");
    }

    [ArchitectTest]
    public static async Task DoublesNewApplicationsWithoutChangingExistingEnchantments()
    {
        using CombatTestContext ctx = new();
        var existing = ctx.CardInHand<StrikeArchitect>();
        ArchitectEnchantmentHelper.Add(existing, ArchitectEnchantKind.Sharp, 3);
        await ctx.ApplyPower<InfiniteBlueprintPower>();
        var attack = ctx.CardInHand<StrikeArchitect>();
        var block = ctx.CardInHand<DefendArchitect>();
        ArchitectEnchantmentHelper.Add(attack, ArchitectEnchantKind.Sharp, 3);
        ArchitectEnchantmentHelper.Add(block, ArchitectEnchantKind.Nimble, 5);
        AssertEx.Equal(3, CombatTestContext.EnchantAmount(existing), "Existing enchantment is unchanged");
        AssertEx.Equal(6, CombatTestContext.EnchantAmount(attack), "Sharp application doubles");
        AssertEx.Equal(10, CombatTestContext.EnchantAmount(block), "Nimble application doubles");
        AssertEx.False(ArchitectEnchantmentHelper.CanReceiveAnotherEnchant(attack), "Blueprint no longer opens multiple slots");
        ArchitectEnchantmentHelper.Refresh(attack);
        AssertEx.Equal(6, CombatTestContext.EnchantAmount(attack), "Refresh does not double again");
        var transfer = ctx.CardInHand<StrikeArchitect>();
        ArchitectEnchantmentHelper.Transfer(attack, transfer);
        AssertEx.Equal(6, CombatTestContext.EnchantAmount(transfer), "Transfer preserves stack count");
    }

    [ArchitectTest]
    public static async Task MultiplierBelongsToTheEnchanterRatherThanTargetOwner()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        await ctx.ApplyPower<InfiniteBlueprintPower>();
        var allyTarget = ctx.CardInHand<StrikeArchitect>(ctx.Ally);
        ArchitectEnchantmentHelper.Add(allyTarget, ArchitectEnchantKind.Sharp, 3, ctx.Player);
        AssertEx.Equal(6, CombatTestContext.EnchantAmount(allyTarget), "Caster's blueprint applies to ally targets");
        var ownTarget = ctx.CardInHand<StrikeArchitect>();
        ArchitectEnchantmentHelper.Add(ownTarget, ArchitectEnchantKind.Sharp, 3, ctx.Ally);
        AssertEx.Equal(3, CombatTestContext.EnchantAmount(ownTarget), "Recipient's blueprint does not multiply ally applications");
    }
}
