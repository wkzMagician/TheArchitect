using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Cards.Rare;

public static class GenesisTests
{
    [ArchitectTest]
    public static async Task ExplicitDrawPileAddTriggersForTopAndRandomButReshuffleDoesNot()
    {
        using CombatTestContext ctx = new();
        await ctx.ApplyPower<GenesisPower>(amount: 4);

        MockSkillCard fromHand = ctx.MockSkillInHand();
        await CardPileCmd.Add(fromHand, PileType.Draw, CardPilePosition.Top);
        AssertEx.Equal(4, ctx.Player.Creature.Block, "Putting a card on top of the draw pile triggers Genesis");
        AssertEx.True(!ctx.Player.PlayerCombatState!.Hand.Cards.Contains(fromHand), "Moved card leaves the hand");

        MockSkillCard fromDiscard = ctx.CardInDiscard<MockSkillCard>();
        await CardPileCmd.Add(fromDiscard, PileType.Draw, CardPilePosition.Random);
        AssertEx.Equal(8, ctx.Player.Creature.Block, "Random placement triggers Genesis");
        AssertEx.True(!ctx.Player.PlayerCombatState.DiscardPile.Cards.Contains(fromDiscard), "Moved card leaves discard");

        MockSkillCard reshuffled = ctx.CardInDiscard<MockSkillCard>();
        await CardPileCmd.Shuffle(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(ctx.Player.PlayerCombatState.DrawPile.Cards.Contains(reshuffled), "Shuffle moves discard into draw");
        AssertEx.Equal(8, ctx.Player.Creature.Block, "Automatic discard reshuffle does not trigger Genesis");
    }

    [ArchitectTest]
    public static void UpgradeKeepsBlockVarUsableInCardLibrary()
    {
        using CombatTestContext context = new();
        Genesis card = (Genesis)ModelDb.Card<Genesis>().ToMutable();
        card.Owner = context.Player;

        AssertEx.Equal(4m, card.DynamicVars.Block.BaseValue, "Genesis base block value");
        card.UpgradeInternal();
        AssertEx.Equal(5m, card.DynamicVars.Block.BaseValue, "Genesis upgraded block value");
    }
}
