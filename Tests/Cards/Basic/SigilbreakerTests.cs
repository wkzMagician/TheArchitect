using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Basic;

public static class SigilbreakerTests
{
    [ArchitectTest]
    public static async Task CombatDescriptionUsesHoveredTargetsVulnerability()
    {
        using CombatTestContext ctx = new();
        Sigilbreaker card = ctx.CardInHand<Sigilbreaker>();
        for (int i = 0; i < 3; i++)
            ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Swift, 1);

        await ctx.ApplyPower<VulnerablePower>(ctx.Enemy);
        string untargeted = card.GetDescriptionForPile(PileType.Hand);
        string targeted = card.GetDescriptionForPile(PileType.Hand, ctx.Enemy);
        string untargetedAgain = card.GetDescriptionForPile(PileType.Hand);

        AssertEx.True(untargeted.Contains("20"), $"Untargeted damage should be 20. Actual: {untargeted}");
        AssertEx.True(targeted.Contains("30"), $"Vulnerable target should take 30 damage. Actual: {targeted}");
        AssertEx.True(untargetedAgain.Contains("20"), "Target preview must not persist after targeting ends");
    }

    [ArchitectTest]
    public static async Task CombatDescriptionUsesNativeDamagePreviewUnderShrink()
    {
        using CombatTestContext ctx = new();
        Sigilbreaker card = ctx.CardInHand<Sigilbreaker>();
        for (int i = 0; i < 3; i++)
        {
            ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Swift, 1);
        }

        await ctx.ApplyPower<ShrinkPower>();
        string description = card.GetDescriptionForPile(PileType.Hand);

        AssertEx.True(description.Contains("14"), $"Shrink should reduce Sigilbreaker's 20 damage to 14. Actual: {description}");
        AssertEx.True(description.Contains("[red]"), $"Reduced damage should use native red styling. Actual: {description}");
    }

    private static readonly MethodInfo SetIsMutableMethod =
        typeof(AbstractModel).GetMethod("NeverEverCallThisOutsideOfTests_SetIsMutable", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private sealed class TestEnchantment : EnchantmentModel;

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Sigilbreaker>();
    }

    [ArchitectTest]
    public static void CombatPreviewShowsCurrentDamage()
    {
        TestMode.TurnOnInternal();
        Sigilbreaker card = TestModels.MutableCard<Sigilbreaker>();
        StrikeArchitect a = TestModels.MutableCard<StrikeArchitect>();
        StrikeArchitect b = TestModels.MutableCard<StrikeArchitect>();

        a.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);
        b.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);

        string preview = ArchitectEnchantmentHelper.DescribeSigilbreakerDamage(card, [a, b]);

        AssertEx.True(
            preview.Contains("16") && (preview.Contains("damage", StringComparison.OrdinalIgnoreCase) || preview.Contains("伤害")),
            $"Sigilbreaker should show its current combat damage in the active language. Actual: {preview}");
    }

    [ArchitectTest]
    public static void CombatPreviewExcludesSelfWhenCountingEnchantedCards()
    {
        TestMode.TurnOnInternal();
        Sigilbreaker card = TestModels.MutableCard<Sigilbreaker>();
        StrikeArchitect other = TestModels.MutableCard<StrikeArchitect>();

        card.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);
        other.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);

        int enchantedOthers = ArchitectEnchantmentHelper.CountOtherEnchantedCards(card, [card, other]);

        AssertEx.Equal(1, enchantedOthers, "Sigilbreaker should exclude itself from enchanted card count.");
    }

    private static T MakeMutable<T>(T model) where T : AbstractModel
    {
        SetIsMutableMethod.Invoke(model, [true]);
        return model;
    }
}
