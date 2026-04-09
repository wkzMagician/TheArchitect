using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class SweepTheHostTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<SweepTheHost>(CardType.Attack, CardRarity.Common, TargetType.AnyEnemy);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<SweepTheHost>();
    }

    [ArchitectTest]
    public static void DescriptionMentionsTargetingAllEnemiesInsteadOfExtraHit()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-SWEEP_THE_HOST.description");

        AssertEx.True(description.Contains("target ALL enemies"), "Sweep the Host should describe targeting all enemies when enchanted.");
        AssertEx.False(description.Contains("also deal"), "Sweep the Host should no longer describe an extra all-enemies hit.");
        AssertEx.False(description.Contains("{CombatPreview}"), "Sweep the Host should not include a combat preview placeholder.");
    }
}
