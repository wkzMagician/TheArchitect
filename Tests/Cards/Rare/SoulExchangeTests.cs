using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class SoulExchangeTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<SoulExchange>(CardType.Skill, CardRarity.Uncommon, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<SoulExchange>();
    }

    [ArchitectTest]
    public static void DescriptionDoesNotRepeatExhaustOrExposeRawFormatTokens()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-SOUL_EXCHANGE.description");

        AssertEx.False(description.Contains("IfUpgraded:"), "Soul Exchange should not expose raw upgrade format tokens.");
        AssertEx.False(description.Contains("Exhaust"), "Soul Exchange should rely on the Exhaust keyword instead of repeating it in the description.");
    }
}
