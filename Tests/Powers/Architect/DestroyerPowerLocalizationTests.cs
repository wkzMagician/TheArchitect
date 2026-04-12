using TheArchitect.Tests.Infrastructure;

namespace TheArchitect.Tests.Powers.Architect;

public static class DestroyerPowerLocalizationTests
{
    [ArchitectTest]
    public static void DescriptionUsesLocStringAmountFormatter()
    {
        string description = LocalizationCatalog.PowerEntry("THEARCHITECT-DESTROYER_POWER.description");

        AssertEx.False(description.Contains("#b{amount}", StringComparison.Ordinal),
            "Destroyer power description should not use legacy '#b{amount}' formatting.");
        AssertEx.True(description.Contains("{Amount:", StringComparison.Ordinal),
            "Destroyer power description should use the Amount LocString formatter.");
    }
}
