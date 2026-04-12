using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class CrashingBlowTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<CrashingBlow>(CardType.Attack, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<CrashingBlow>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotContainRawFormatTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-CRASHING_BLOW.description");

        AssertEx.False(description.Contains("IfUpgraded:"), "Crashing Blow should not expose raw upgrade format tokens.");
    }
}
