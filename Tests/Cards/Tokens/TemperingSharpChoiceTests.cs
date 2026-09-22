using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Tokens;

public static class TemperingSharpChoiceTests
{
    [ArchitectTest]
    public static void IsATokenChoiceForSharp()
    {
        TemperingSharpChoice card = TestModels.Card<TemperingSharpChoice>();

        AssertEx.Equal(ArchitectEnchantKind.Sharp, card.Kind, "TemperingSharpChoice should select the Sharp enchantment");
        AssertEx.Equal(CardType.Skill, card.Type, "TemperingSharpChoice card type mismatch");
        AssertEx.Equal(CardRarity.Token, card.Rarity, "TemperingSharpChoice card rarity mismatch");
        AssertEx.Equal(TargetType.None, card.TargetType, "TemperingSharpChoice target type mismatch");
    }
}
