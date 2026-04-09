using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Basic;

public static class TemperingTests
{
    private static readonly MethodInfo SetIsMutableMethod =
        typeof(AbstractModel).GetMethod("NeverEverCallThisOutsideOfTests_SetIsMutable", BindingFlags.Instance | BindingFlags.NonPublic)!;

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

    private static T MakeMutable<T>(T model) where T : AbstractModel
    {
        SetIsMutableMethod.Invoke(model, [true]);
        return model;
    }
}
