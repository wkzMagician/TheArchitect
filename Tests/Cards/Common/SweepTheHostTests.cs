using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class SweepTheHostTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<SweepTheHost>(CardType.Attack, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<SweepTheHost>();
    }

    [ArchitectTest]
    public static void DescriptionMentionsUpgradeTargetingAllEnemies()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-SWEEP_THE_HOST.description");

        AssertEx.True(description.Contains("Upgraded", StringComparison.Ordinal), "Sweep the Host should explain the upgraded targeting change.");
        AssertEx.True(description.Contains("ALL enemies", StringComparison.Ordinal), "Sweep the Host should describe upgraded all-enemy targeting.");
        AssertEx.False(description.Contains("Enchanted", StringComparison.Ordinal), "Sweep the Host should no longer tie targeting to enchantment.");
    }

    [ArchitectTest]
    public static void SourceSetsUpgradedTargetTypeToAllEnemies()
    {
        string source = File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitectCode", "Cards", "Common", "SweepTheHost.cs")));
        AssertEx.True(source.Contains("public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;", StringComparison.Ordinal),
            "Sweep the Host should change its target type to all enemies when upgraded.");
    }
}
