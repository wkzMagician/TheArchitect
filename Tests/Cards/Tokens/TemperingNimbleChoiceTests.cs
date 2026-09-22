using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Tokens;

public static class TemperingNimbleChoiceTests
{
    [ArchitectTest]
    public static void IsATokenChoiceForNimble()
    {
        TemperingNimbleChoice card = TestModels.Card<TemperingNimbleChoice>();

        AssertEx.Equal(ArchitectEnchantKind.Nimble, card.Kind, "TemperingNimbleChoice should select the Nimble enchantment");
        AssertEx.Equal(CardType.Skill, card.Type, "TemperingNimbleChoice card type mismatch");
        AssertEx.Equal(CardRarity.Token, card.Rarity, "TemperingNimbleChoice card rarity mismatch");
        AssertEx.Equal(TargetType.None, card.TargetType, "TemperingNimbleChoice target type mismatch");
    }
}
