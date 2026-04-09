using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class DoublePlatedGuardTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<DoublePlatedGuard>(CardType.Skill, CardRarity.Common, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<DoublePlatedGuard>();
    }

    [ArchitectTest]
    public static void CombatPreviewIsDisabled()
    {
        DoublePlatedGuard card = new();

        string preview = (string)typeof(DoublePlatedGuard)
            .GetMethod("GetCombatPreviewText", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(card, null)!;

        AssertEx.Equal(string.Empty, preview, "Double-Plated Guard should not add dynamic combat preview text.");
    }
}
