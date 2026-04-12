using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class SkyrendJudgmentTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<SkyrendJudgment>(CardType.Skill, CardRarity.Rare, TargetType.AllEnemies);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<SkyrendJudgment>();
    }

    [ArchitectTest]
    public static void DescriptionMentionsCostsOneLessPerEnchantedCardPlayed()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-SKYREND_JUDGMENT.description");

        AssertEx.True(description.Contains("Costs 1 less", StringComparison.Ordinal), "Skyrend Judgment should explicitly say it costs 1 less.");
        AssertEx.True(description.Contains("Enchanted card", StringComparison.Ordinal), "Skyrend Judgment should say the discount is tied to Enchanted cards being played.");
    }

    [ArchitectTest]
    public static void SourceReducesCostWhenEnchantedCardsArePlayed()
    {
        string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitectCode", "Cards", "Rare", "SkyrendJudgment.cs"));

        AssertEx.True(source.Contains("AfterCardPlayed", StringComparison.Ordinal), "Skyrend Judgment should react after cards are played.");
        AssertEx.True(source.Contains("EnergyCost.AddThisCombat(-1);", StringComparison.Ordinal), "Skyrend Judgment should reduce its cost by 1 this combat.");
    }
}
