using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.TestSupport;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Basic;

public static class TemperingTests
{
    private static readonly MethodInfo SetIsMutableMethod =
        typeof(AbstractModel).GetMethod("NeverEverCallThisOutsideOfTests_SetIsMutable", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private sealed class TestEnchantment : EnchantmentModel;

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Tempering>();
    }

    [ArchitectTest]
    public static void TemperingTargetFilterRejectsAlreadyEnchantedOrdinaryCards()
    {
        TestMode.TurnOnInternal();
        StrikeArchitect unenchanted = TestModels.Card<StrikeArchitect>();
        StrikeArchitect enchantedOrdinary = TestModels.MutableCard<StrikeArchitect>();
        AncientSeed enchantedAncientSeed = TestModels.MutableCard<AncientSeed>();
        EnchantmentModel enchantment = MakeMutable<EnchantmentModel>(new TestEnchantment());

        enchantedOrdinary.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);
        enchantedAncientSeed.EnchantInternal(enchantment, 1);

        AssertEx.True(ArchitectEnchantmentHelper.CanTargetWithTempering(unenchanted), "Tempering should allow unenchanted compatible cards.");
        AssertEx.False(ArchitectEnchantmentHelper.CanTargetWithTempering(enchantedOrdinary), "Tempering should reject already enchanted ordinary cards.");
        AssertEx.False(ArchitectEnchantmentHelper.CanTargetWithTempering(enchantedAncientSeed), "Tempering should reject an already enchanted Ancient Seed.");
    }

    [ArchitectTest]
    public static void TemperingChoiceCardsAreCreatedFromModelDb()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Basic", "Tempering.cs"));

        AssertEx.False(source.Contains("new TemperingSharpChoice", StringComparison.Ordinal), "Tempering should not construct token model cards directly.");
        AssertEx.False(source.Contains("new TemperingNimbleChoice", StringComparison.Ordinal), "Tempering should not construct token model cards directly.");
        AssertEx.True(source.Contains("CreateCard(ModelDb.Card<TemperingSharpChoice>()", StringComparison.Ordinal), "Tempering should create combat copies from ModelDb canonical choice cards.");
    }

    private static T MakeMutable<T>(T model) where T : AbstractModel
    {
        SetIsMutableMethod.Invoke(model, [true]);
        return model;
    }
}
