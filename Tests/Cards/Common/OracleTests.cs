using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;

namespace TheArchitect.Tests.Cards.Common;

public static class OracleTests
{

    [ArchitectTest]
    public static void DoesNotRetain()
    {
        Oracle card = TestModels.Card<Oracle>();

        AssertEx.False(card.Keywords.Contains(CardKeyword.Retain), "Oracle should not Retain.");
    }

    [ArchitectTest]
    public static async Task NoEnchantedCardLeavesDrawPileAlone()
    {
        using CombatTestContext ctx = new();
        Oracle oracle = ctx.CardInHand<Oracle>();
        MockSkillCard unenchanted = ctx.CardInDraw<MockSkillCard>().MockBlock(5);

        await ctx.Play(oracle);

        AssertEx.Equal(0, ctx.Player.Creature.Block, "Oracle should not play an unenchanted card");
        AssertEx.True(ctx.Player.PlayerCombatState!.DrawPile.Cards.Contains(unenchanted),
            "Oracle should leave the draw pile unchanged when no enchanted card exists");
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<Oracle>();
    }
}
