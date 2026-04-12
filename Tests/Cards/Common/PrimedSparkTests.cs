using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class PrimedSparkTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<PrimedSpark>(CardType.Skill, CardRarity.Rare, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<PrimedSpark>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotExposeRawConditionalTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-PRIMED_SPARK.description");

        AssertEx.False(description.Contains("IfUpgraded:", StringComparison.Ordinal), "Primed Spark should not expose raw upgrade format tokens.");
        AssertEx.True(description.Contains("[gold]Sown[/gold] 1", StringComparison.Ordinal), "Primed Spark should describe the base Sown amount.");
        AssertEx.True(description.Contains("[gold]Swift[/gold] 1", StringComparison.Ordinal), "Primed Spark should describe the base Swift amount.");
    }
}
