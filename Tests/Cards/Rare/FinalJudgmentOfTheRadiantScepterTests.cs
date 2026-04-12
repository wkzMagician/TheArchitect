using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class FinalJudgmentOfTheRadiantScepterTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<FinalJudgmentOfTheRadiantScepter>(CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<FinalJudgmentOfTheRadiantScepter>();
    }

    [ArchitectTest]
    public static void LocalizationUsesShortenedTitle()
    {
        string title = LocalizationCatalog.CardEntry("THEARCHITECT-FINAL_JUDGMENT_OF_THE_RADIANT_SCEPTER.title");
        AssertEx.Equal("Final Judgment", title, "Final Judgment of the Radiant Scepter should use the shortened visible title.");
    }
}
