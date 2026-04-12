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
    private static readonly MethodInfo? CreateChoiceCardMethod =
        typeof(Tempering).GetMethod("CreateChoiceCard", BindingFlags.Static | BindingFlags.NonPublic);

    private sealed class TestEnchantment : EnchantmentModel;

    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<Tempering>(CardType.Skill, CardRarity.Basic, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<Tempering>();
    }

    [ArchitectTest]
    public static void TemperingTargetFilterRejectsAlreadyEnchantedOrdinaryCards()
    {
        TestMode.TurnOnInternal();
        StrikeArchitect unenchanted = new();
        StrikeArchitect enchantedOrdinary = MakeMutable(new StrikeArchitect());
        AncientSeed enchantedAncientSeed = MakeMutable(new AncientSeed());
        EnchantmentModel enchantment = MakeMutable<EnchantmentModel>(new TestEnchantment());

        enchantedOrdinary.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);
        enchantedAncientSeed.EnchantInternal(enchantment, 1);

        AssertEx.True(ArchitectEnchantmentHelper.CanTargetWithTempering(unenchanted), "Tempering should allow unenchanted compatible cards.");
        AssertEx.False(ArchitectEnchantmentHelper.CanTargetWithTempering(enchantedOrdinary), "Tempering should reject already enchanted ordinary cards.");
        AssertEx.True(ArchitectEnchantmentHelper.CanTargetWithTempering(enchantedAncientSeed), "Tempering should allow explicit multi-enchant exceptions.");
    }

    [ArchitectTest]
    public static void TemperingCreatesFreshChoiceCards()
    {
        AssertEx.NotNull(CreateChoiceCardMethod, "Tempering should create dual-choice cards through a dedicated factory.");

        TemperingChoiceCard first = (TemperingChoiceCard)CreateChoiceCardMethod!.Invoke(null, [ArchitectEnchantKind.Sharp])!;
        TemperingChoiceCard second = (TemperingChoiceCard)CreateChoiceCardMethod.Invoke(null, [ArchitectEnchantKind.Sharp])!;
        TemperingChoiceCard nimble = (TemperingChoiceCard)CreateChoiceCardMethod.Invoke(null, [ArchitectEnchantKind.Nimble])!;

        AssertEx.False(ReferenceEquals(first, second), "Tempering should create a fresh mutable choice card each time.");
        AssertEx.Equal("TemperingSharpChoice", first.GetType().Name, "Tempering should create the Sharp choice card for Sharp.");
        AssertEx.Equal("TemperingNimbleChoice", nimble.GetType().Name, "Tempering should create the Nimble choice card for Nimble.");
    }

    private static T MakeMutable<T>(T model) where T : AbstractModel
    {
        SetIsMutableMethod.Invoke(model, [true]);
        return model;
    }
}
