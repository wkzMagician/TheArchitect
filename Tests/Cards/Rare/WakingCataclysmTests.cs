using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class WakingCataclysmTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<WakingCataclysm>(CardType.Attack, CardRarity.Rare, TargetType.AllEnemies);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<WakingCataclysm>();
    }

    [ArchitectTest]
    public static void DescriptionSaysItIsPlayedImmediately()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-WAKING_CATACLYSM.description");

        AssertEx.True(description.Contains("play it immediately", StringComparison.Ordinal), "Waking Cataclysm should say it is played immediately.");
    }
}
