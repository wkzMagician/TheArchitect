using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class DivineSelectionTests
{

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<DivineSelection>();
    }

    [ArchitectTest]
    public static void ChoiceCardsAreCreatedFromModelDb()
    {
        string source = File.ReadAllText(TestPaths.RepoPath("TheArchitectCode", "Cards", "Uncommon", "DivineSelection.cs"));

        AssertEx.False(source.Contains("new EnchantChoiceCard", StringComparison.Ordinal), "Divine Selection should not construct token model cards directly.");
        AssertEx.True(source.Contains("CreateCard(ModelDb.Card<EnchantChoiceCard>()", StringComparison.Ordinal), "Divine Selection should create combat copies from the canonical choice card.");
    }
}
