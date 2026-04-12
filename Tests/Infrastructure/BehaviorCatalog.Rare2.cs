using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task CursePurge()
    {
        using BehaviorTestContext ctx = new();
        CursePurge card = ctx.CardInHand<CursePurge>();
        CardModel c1 = ctx.CardInDeck<MockCurseCard>();
        CardModel c2 = ctx.CardInDeck<MockCurseCard>();
        await ctx.Play(card);
        AssertEx.Equal(6, BehaviorTestContext.PowerAmount<PlatingPower>(ctx.Player.Creature), "CursePurge should gain Plating per curse");
        AssertEx.False(ctx.Player.Deck.Cards.Contains(c1) || ctx.Player.Deck.Cards.Contains(c2), "CursePurge should remove curses from deck");
    }

    private static async Task Depose()
    {
        using BehaviorTestContext ctx = new();
        Depose card = ctx.CardInHand<Depose>();
        CardModel removable = ctx.CardInDeck<MockAttackCard>();
        await CreatureCmd.Damage(ctx.ChoiceContext, ctx.Enemy, 9998m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable | MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null, null);
        ctx.Select(removable);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.False(ctx.Player.Deck.Cards.Contains(removable), "Depose should remove a deck card on kill");
    }

    private static async Task Rollback()
    {
        using BehaviorTestContext ctx = new();
        Rollback card = ctx.CardInHand<Rollback>();
        StrikeArchitect target = ctx.CardInHand<StrikeArchitect>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(target), "Rollback should mark selected card to return to draw");
    }

    private static async Task WriteDestiny()
    {
        using BehaviorTestContext ctx = new();
        WriteDestiny card = ctx.CardInHand<WriteDestiny>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(ctx.Player.Deck.Cards.Contains(target), "WriteDestiny should move selected hand card to deck");
    }

    private static async Task ChannelPower()
    {
        using BehaviorTestContext ctx = new();
        ChannelPower card = ctx.CardInHand<ChannelPower>();
        MockSkillCard chosen = ctx.MockSkillInHand().MockBlock(5);
        ctx.Select(chosen);
        await ctx.Play(card, ctx.Enemy, xValue: 2);
        AssertEx.Equal(0, ctx.Player.Creature.Block, "ChannelPower should not immediately replay the selected card");
        await ctx.Play(chosen);
        AssertEx.Equal(15, ctx.Player.Creature.Block, "ChannelPower should replay the chosen card when it is played");
    }

    private static async Task LayeredBrace()
    {
        using BehaviorTestContext ctx = new();
        LayeredBrace card = ctx.CardInHand<LayeredBrace>();
        ctx.MarkPlayed(card, 1);
        await ctx.Play(card);
        AssertEx.Equal(12, ctx.Player.Creature.Block, "LayeredBrace should scale with prior plays");
    }

    private static async Task Ascend()
    {
        using BehaviorTestContext ctx = new();
        Ascend card = ctx.CardInHand<Ascend>();
        ctx.MarkPlayed(card, 2);
        MockSkillCard drawn = ctx.CardInDraw<MockSkillCard>();
        int beforeEnergy = ctx.Player.PlayerCombatState!.Energy;
        int beforeHp = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(5, ctx.HpLost(ctx.Enemy, beforeHp), "Ascend should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState.Hand.Cards.Contains(drawn), "Ascend should draw after repeated plays");
        AssertEx.Equal(beforeEnergy + 1, ctx.Player.PlayerCombatState.Energy, "Ascend should gain energy after enough plays");
    }

    private static async Task Trinity()
    {
        using BehaviorTestContext ctx = new();
        Trinity card = ctx.CardInHand<Trinity>();
        ctx.MarkPlayed(card, 2);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(30, ctx.HpLost(ctx.Enemy, before), "Trinity should switch to big damage after repeated plays");
    }

    private static async Task Proliferation()
    {
        using BehaviorTestContext ctx = new();
        Proliferation card = ctx.CardInHand<Proliferation>();
        ctx.MarkPlayed(card, 2);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(9, ctx.HpLost(ctx.Enemy, before), "Proliferation should scale hits and damage with plays");
    }

    private static async Task RaiseOffspring()
    {
        using BehaviorTestContext ctx = new();
        RaiseOffspring card = ctx.CardInHand<RaiseOffspring>();
        ctx.MarkPlayed(card, 1);
        MockSkillCard a = ctx.CardInDraw<MockSkillCard>();
        MockSkillCard b = ctx.CardInDraw<MockSkillCard>();
        MockSkillCard c = ctx.CardInDraw<MockSkillCard>();
        await ctx.Play(card);
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(a) && ctx.Player.PlayerCombatState.Hand.Cards.Contains(b) && ctx.Player.PlayerCombatState.Hand.Cards.Contains(c), "RaiseOffspring should draw extra cards");
    }

    private static async Task FinalJudgmentOfTheRadiantScepter()
    {
        using BehaviorTestContext ctx = new();
        FinalJudgmentOfTheRadiantScepter card = ctx.CardInHand<FinalJudgmentOfTheRadiantScepter>();
        ctx.MarkPlayed(card, 9);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(100, ctx.HpLost(ctx.Enemy, before), "FinalJudgment should hit ten times after enough plays");
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(card), "FinalJudgment should return to draw pile");
    }

    private static async Task WakingCataclysm()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        WakingCataclysm card = ctx.CardInHand<WakingCataclysm>();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card);
        AssertEx.Equal(24, ctx.HpLost(ctx.Enemy, before1), "WakingCataclysm should hit first enemy");
        AssertEx.Equal(24, ctx.HpLost(ctx.SecondEnemy, before2), "WakingCataclysm should hit second enemy");
    }

    private static async Task AncientVerdict()
    {
        using BehaviorTestContext ctx = new();
        AncientVerdict card = ctx.CardInHand<AncientVerdict>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Nimble, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(28, ctx.HpLost(ctx.Enemy, before), "AncientVerdict should scale with enchanted hand");
    }

    private static async Task InfiniteBlueprint()
    {
        using BehaviorTestContext ctx = new();
        InfiniteBlueprint card = ctx.CardInHand<InfiniteBlueprint>();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 2m);
        await ctx.Play(card);
        AssertEx.NotNull(ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Nimble, 1m),
            "InfiniteBlueprint should allow adding further enchantments to already enchanted cards");
    }
}
