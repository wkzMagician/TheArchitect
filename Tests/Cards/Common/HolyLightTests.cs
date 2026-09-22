using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class HolyLightTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<HolyLight>();
    }

    [ArchitectTest]
    public static void SourceFiltersAlreadyEnchantedDrawCards()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Rare", "HolyLight.cs"));

        AssertEx.True(source.Contains("CanTargetForSpecificEnchant(card, ArchitectEnchantKind.Glam)", StringComparison.Ordinal), "Holy Light should only offer draw cards that can receive Glam.");
    }
}
