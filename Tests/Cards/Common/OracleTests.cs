using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class OracleTests
{

    [ArchitectTest]
    public static void HasRetain()
    {
        Oracle card = TestModels.Card<Oracle>();

        AssertEx.True(card.Keywords.Contains(CardKeyword.Retain), "Oracle should Retain.");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Oracle>();
    }
}
