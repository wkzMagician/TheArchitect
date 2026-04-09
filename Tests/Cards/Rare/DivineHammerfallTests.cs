using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class DivineHammerfallTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<DivineHammerfall>(CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<DivineHammerfall>();
    }

    [ArchitectTest]
    public static void DescriptionIncludesDebuffAmounts()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-DIVINE_HAMMERFALL.description");

        AssertEx.True(description.Contains("2 [gold]Weak[/gold]"), "Divine Hammerfall should state its Weak amount.");
        AssertEx.True(description.Contains("2 [gold]Vulnerable[/gold]"), "Divine Hammerfall should state its Vulnerable amount.");
    }
}
