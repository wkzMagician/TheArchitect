using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Enchantments.Framework;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Helpers;

public static class ArchitectEnchantmentHelperTests
{
    [ArchitectTest]
    public static void BasicEnchantAmountsMatchConfiguredValues()
    {
        AssertEx.Equal(2, ArchitectEnchantmentHelper.AmountFor(ArchitectEnchantKind.Nimble), "Nimble should be a 2-stack basic enchant.");
        AssertEx.Equal(2, ArchitectEnchantmentHelper.AmountFor(ArchitectEnchantKind.Sharp), "Sharp should be a 2-stack basic enchant.");
        AssertEx.Equal(1, ArchitectEnchantmentHelper.AmountFor(ArchitectEnchantKind.Seed), "Sown should be a 1-stack basic enchant.");
        AssertEx.Equal(2, ArchitectEnchantmentHelper.AmountFor(ArchitectEnchantKind.Swift), "Swift should be a 2-stack basic enchant.");
        AssertEx.Equal(2, ArchitectEnchantmentHelper.AmountFor(ArchitectEnchantKind.Instinct), "Instinct should be a 2-stack basic enchant.");
    }

    [ArchitectTest]
    public static void TemperingOptionsForUsesCompatibleEnchantments()
    {
        IReadOnlyList<ArchitectEnchantKind> strikeOptions = ArchitectEnchantmentHelper.TemperingOptionsFor(new StrikeArchitect());
        IReadOnlyList<ArchitectEnchantKind> defendOptions = ArchitectEnchantmentHelper.TemperingOptionsFor(new DefendArchitect());
        IReadOnlyList<ArchitectEnchantKind> blockAttackOptions = ArchitectEnchantmentHelper.TemperingOptionsFor(new WardedCut());

        AssertEx.Equal(1, strikeOptions.Count, "Tempering should offer one enchantment for non-blocking attacks");
        AssertEx.Equal(ArchitectEnchantKind.Sharp, strikeOptions[0], "Tempering should offer Sharp for non-blocking attacks");
        AssertEx.Equal(1, defendOptions.Count, "Tempering should offer one enchantment for skills that block");
        AssertEx.Equal(ArchitectEnchantKind.Nimble, defendOptions[0], "Tempering should offer Nimble for skills that block");
        AssertEx.True(blockAttackOptions.Contains(ArchitectEnchantKind.Sharp), "Tempering should offer Sharp for blocking attacks");
        AssertEx.True(blockAttackOptions.Contains(ArchitectEnchantKind.Nimble), "Tempering should offer Nimble for blocking attacks");
    }

    [ArchitectTest]
    public static void AssetPathsUseGodotSeparators()
    {
        string path = "character_icon_char_name.png".CharacterUiPath();

        AssertEx.False(path.Contains('\\'), "Godot resource paths should not contain backslashes");
        AssertEx.True(path.Contains('/'), "Godot resource paths should use forward slashes");
    }

    [ArchitectTest]
    public static void OrdinaryCardsDoNotSupportMultipleEnchantments()
    {
        WardedCut card = new();

        AssertEx.False(MultiEnchantRegistry.SupportsMultiEnchant(card), "Ordinary cards should reject multiple enchantments by default.");
    }
}
