using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Basic;

public static class SigilbreakerTests
{
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

        AssertEx.True(preview.Contains("deals 16 damage"), "Sigilbreaker should show its current combat damage.");
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
