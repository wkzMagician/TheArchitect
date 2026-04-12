using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;

namespace TheArchitect.Tests.Cards.Tokens;

public static class TemperingSharpChoiceTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<TemperingSharpChoice>(CardType.Skill, CardRarity.Token, TargetType.None);
    }
}
