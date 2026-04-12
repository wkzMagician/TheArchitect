using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class ChantTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<Chant>(CardType.Skill, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<Chant>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotContainRawFormatTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-CHANT.description");

        AssertEx.False(description.Contains("IfUpgraded:"), "Chant should not expose raw upgrade format tokens.");
    }
}
