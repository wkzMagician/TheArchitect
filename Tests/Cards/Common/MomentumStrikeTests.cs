using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.TestSupport;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Common;

public static class MomentumStrikeTests
{
    private static readonly MethodInfo SetIsMutableMethod =
        typeof(AbstractModel).GetMethod("NeverEverCallThisOutsideOfTests_SetIsMutable", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private sealed class TestEnchantment : EnchantmentModel;

    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<MomentumStrike>(CardType.Attack, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<MomentumStrike>();
    }

    [ArchitectTest]
    public static void MomentumStrikeTargetFilterRejectsAlreadyEnchantedOrdinaryCards()
    {
        TestMode.TurnOnInternal();
        MomentumStrike unenchanted = new();
        MomentumStrike enchantedOrdinary = MakeMutable(new MomentumStrike());
        AncientSeed enchantedAncientSeed = MakeMutable(new AncientSeed());

        enchantedOrdinary.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);
        enchantedAncientSeed.EnchantInternal(MakeMutable<EnchantmentModel>(new TestEnchantment()), 1);

        AssertEx.True(ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(unenchanted, ArchitectEnchantKind.Momentum), "Momentum Strike should allow unenchanted cards.");
        AssertEx.False(ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(enchantedOrdinary, ArchitectEnchantKind.Momentum), "Momentum Strike should reject already enchanted ordinary cards.");
        AssertEx.True(ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(enchantedAncientSeed, ArchitectEnchantKind.Momentum), "Momentum Strike should allow explicit multi-enchant exceptions.");
    }

    private static T MakeMutable<T>(T model) where T : AbstractModel
    {
        SetIsMutableMethod.Invoke(model, [true]);
        return model;
    }
}
