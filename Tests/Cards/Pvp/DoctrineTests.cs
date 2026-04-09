using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Uncommon;

namespace TheArchitect.Tests.Cards.Pvp;

public static class DoctrineTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<Doctrine>(CardType.Skill, CardRarity.Uncommon, TargetType.Self);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<Doctrine>();
    }

    [ArchitectTest]
    public static void DescriptionMentionsDexterity()
    {
        string description = LocalizationCatalog.CardEntry("THEARCHITECT-DOCTRINE.description");

        AssertEx.True(description.Contains("Dexterity"), "Doctrine should mention Dexterity for Nimble cards.");
        AssertEx.False(description.Contains("Block equal"), "Doctrine should no longer mention Block for Nimble cards.");
    }
}
