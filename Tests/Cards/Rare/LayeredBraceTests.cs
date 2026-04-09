using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Rare;

public static class LayeredBraceTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<LayeredBrace>(CardType.Skill, CardRarity.Uncommon, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<LayeredBrace>();
    }

    [ArchitectTest]
    public static void CombatPreviewShowsCurrentTriggerCount()
    {
        LayeredBrace card = new();
        ArchitectCombatState.RecordPlayed(card);
        ArchitectCombatState.RecordPlayed(card);

        string preview = (string)typeof(LayeredBrace)
            .GetMethod("GetCombatPreviewText", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(card, null)!;

        AssertEx.True(preview.Contains("triggers 3 times"), "Layered Brace should show its current trigger count.");
    }
}
