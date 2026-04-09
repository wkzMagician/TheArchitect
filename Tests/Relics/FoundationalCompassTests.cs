using MegaCrit.Sts2.Core.Entities.Relics;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Relics;

public static class FoundationalCompassTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertRelicMetadata<FoundationalCompass>(RelicRarity.Starter);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertRelicBehavior<FoundationalCompass>();
    }

    [ArchitectTest]
    public static void DescriptionListsRandomBasicEnchantPool()
    {
        string description = LocalizationCatalog.RelicEntry("THEARCHITECT-FOUNDATIONAL_COMPASS.description");

        AssertEx.True(description.Contains("[gold]Nimble[/gold] 2"), "Foundational Compass should list Nimble 2.");
        AssertEx.True(description.Contains("[gold]Sharp[/gold] 2"), "Foundational Compass should list Sharp 2.");
        AssertEx.True(description.Contains("[gold]Sown[/gold] 1"), "Foundational Compass should list Sown 1.");
        AssertEx.True(description.Contains("[gold]Swift[/gold] 2"), "Foundational Compass should list Swift 2.");
        AssertEx.True(description.Contains("[gold]Instinct[/gold] 2"), "Foundational Compass should list Instinct 2.");
    }
}
