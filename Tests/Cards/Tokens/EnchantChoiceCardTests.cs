using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards.Tokens;

public static class EnchantChoiceCardTests
{
    [ArchitectTest]
    public static void StacklessEnchantmentsOmitAmountFromDescription()
    {
        EnchantChoiceCard card = TestModels.MutableCard<EnchantChoiceCard>();
        card.SetOption(new ArchitectEnchantOption(ArchitectEnchantKind.PerfectFit, 1));
        string stackless = card.GetDescriptionForPile(PileType.None);
        AssertEx.False(stackless.Contains("1"), "Stackless enchantment choice should not show an amount");

        card.SetOption(new ArchitectEnchantOption(ArchitectEnchantKind.Sharp, 3));
        AssertEx.True(card.GetDescriptionForPile(PileType.None).Contains("3"),
            "Stacking enchantment choice should retain its amount");
    }

    [ArchitectTest]
    public static void CarriesTheEnchantmentOptionItWasBuiltFor()
    {
        EnchantChoiceCard card = TestModels.MutableCard<EnchantChoiceCard>();
        ArchitectEnchantOption option = new(ArchitectEnchantKind.Sharp, 4);

        card.SetOption(option);

        AssertEx.Equal(option, card.Option, "EnchantChoiceCard should expose the option it was built for");
        AssertEx.Equal(CardType.Skill, card.Type, "EnchantChoiceCard card type mismatch");
        AssertEx.Equal(CardRarity.Token, card.Rarity, "EnchantChoiceCard card rarity mismatch");
        AssertEx.Equal(TargetType.None, card.TargetType, "EnchantChoiceCard target type mismatch");
    }
}
