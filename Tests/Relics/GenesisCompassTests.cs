using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Relics;

public static class GenesisCompassTests
{
    [ArchitectTest]
    public static async Task EnchantsEntireStartingHandOnceAndPreservesExistingEnchantments()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var relic = TestModels.MutableRelic<GenesisCompass>();
        ctx.Player.AddRelicInternal(relic, silent: true);
        var attack = ctx.MockAttackInHand();
        var skill = ctx.MockSkillInHand();
        var existing = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(existing, ArchitectEnchantKind.Sharp, 7);
        var enchantment = existing.Enchantment;
        var allyCard = ctx.MockAttackInHand(owner: ctx.Ally);

        await relic.BeforeCombatStart();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Ally!);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(attack), "Ally turn does not consume trigger");
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(1, CombatTestContext.EnchantCount(attack), "Starting attack enchanted");
        AssertEx.Equal(1, CombatTestContext.EnchantCount(skill), "Starting skill enchanted");
        AssertEx.True(ReferenceEquals(enchantment, existing.Enchantment), "Existing enchantment preserved");
        AssertEx.Equal(7, CombatTestContext.EnchantAmount(existing), "Existing amount preserved");
        AssertEx.Equal(0, CombatTestContext.EnchantCount(allyCard), "Ally hand unaffected");

        var later = ctx.MockAttackInHand();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(later), "Later turns do not trigger again");
        await relic.BeforeCombatStart();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(1, CombatTestContext.EnchantCount(later), "New combat resets trigger");
    }

    [ArchitectTest]
    public static async Task EmptyStartingHandStillConsumesTrigger()
    {
        using CombatTestContext ctx = new();
        var relic = TestModels.MutableRelic<GenesisCompass>();
        ctx.Player.AddRelicInternal(relic, silent: true);
        await relic.BeforeCombatStart();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        var later = ctx.MockAttackInHand();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(later), "Empty initial hand cannot delay the trigger");
    }

    [ArchitectTest]
    public static async Task OrobasReplacesCompassAndKeepsVanillaMapping()
    {
        using CombatTestContext ctx = new();
        var starter = TestModels.MutableRelic<FoundationalCompass>();
        ctx.Player.AddRelicInternal(starter, silent: true);
        var touch = TestModels.MutableRelic<TouchOfOrobas>();
        AssertEx.True(touch.SetupForPlayer(ctx.Player), "Orobas offers upgrade");
        AssertEx.Equal(ModelDb.Relic<GenesisCompass>().Id, touch.UpgradedRelic!, "Compass upgrade selected");
        AssertEx.True(touch.GetUpgradedStarterRelic(ModelDb.Relic<BurningBlood>()) is BlackBlood,
            "Vanilla Burning Blood mapping preserved");
        ctx.Player.AddRelicInternal(touch, silent: true);
        await touch.AfterObtained();
        AssertEx.False(ctx.Player.Relics.Any(r => r is FoundationalCompass), "Starter removed");
        AssertEx.Equal(1, ctx.Player.Relics.Count(r => r is GenesisCompass), "Exactly one upgraded compass granted");
    }
}
