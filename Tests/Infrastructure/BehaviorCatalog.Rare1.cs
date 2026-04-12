using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task GrandOpus()
    {
        using BehaviorTestContext ctx = new();
        GrandOpus card = ctx.CardInHand<GrandOpus>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy, xValue: 2);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "GrandOpus should scale damage with X");
        AssertEx.Equal(8, ctx.Player.Creature.Block, "GrandOpus should scale block with X");
    }

    private static async Task SoulExchange()
    {
        using BehaviorTestContext ctx = new();
        SoulExchange card = ctx.CardInHand<SoulExchange>();
        MockAttackCard source = ctx.MockAttackInHand();
        MockSkillCard target = ctx.MockSkillInHand();
        ArchitectEnchantmentHelper.Add(source, ArchitectEnchantKind.Sharp, 2m);
        ctx.Select(source);
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(0, BehaviorTestContext.EnchantCount(source), "SoulExchange should clear source");
        AssertEx.True(BehaviorTestContext.HasEnchant<Sharp>(target), "SoulExchange should transfer enchantments");
    }

    private static async Task BlightAnointing()
    {
        using BehaviorTestContext ctx = new();
        BlightAnointing card = ctx.CardInHand<BlightAnointing>();
        MockAttackCard attack = ctx.MockAttackInHand();
        MockSkillCard skill = ctx.MockSkillInHand();
        MockAttackCard futureAttack = ctx.CardInDraw<MockAttackCard>();
        await ctx.Play(card);
        await CardPileCmd.Add(futureAttack, PileType.Hand);
        AssertEx.True(BehaviorTestContext.HasEnchant<Corrupted>(attack), "BlightAnointing should enchant attacks");
        AssertEx.False(BehaviorTestContext.HasEnchant<Corrupted>(skill), "BlightAnointing should ignore skills");
        AssertEx.True(BehaviorTestContext.HasEnchant<Corrupted>(futureAttack), "BlightAnointing should affect future attacks entering hand");
    }

    private static async Task DivineSelection()
    {
        using BehaviorTestContext ctx = new();
        DivineSelection card = ctx.CardInHand<DivineSelection>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(1, BehaviorTestContext.EnchantCount(target), "DivineSelection should add one enchantment");
    }

    private static async Task EternalVerdict()
    {
        using BehaviorTestContext ctx = new();
        EternalVerdict card = ctx.CardInHand<EternalVerdict>();
        MockAttackCard common = ctx.MockAttackInHand();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "EternalVerdict should deal damage");
        AssertEx.True(BehaviorTestContext.HasEnchant<TezcatarasEmber>(common), "EternalVerdict should ember a common card");
    }

    private static async Task RadiantMight()
    {
        using BehaviorTestContext ctx = new();
        RadiantMight card = ctx.CardInHand<RadiantMight>();
        MockAttackCard other = ctx.MockAttackInHand();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "RadiantMight should deal damage");
        AssertEx.True(BehaviorTestContext.HasEnchant<Sharp>(other), "RadiantMight should enchant hand with Sharp");
    }

    private static async Task ChorusOfEvasion()
    {
        using BehaviorTestContext ctx = new();
        ChorusOfEvasion card = ctx.CardInHand<ChorusOfEvasion>();
        MockAttackCard other = ctx.MockAttackInHand();
        await ctx.Play(card);
        AssertEx.Equal(8, ctx.Player.Creature.Block, "ChorusOfEvasion should give block");
        AssertEx.True(BehaviorTestContext.HasEnchant<Nimble>(other), "ChorusOfEvasion should enchant hand with Nimble");
    }

    private static async Task Daydream()
    {
        using BehaviorTestContext ctx = new();
        Daydream card = ctx.CardInHand<Daydream>();
        MockAttackCard a = ctx.MockAttackInHand();
        MockSkillCard b = ctx.MockSkillInHand();
        await ctx.Play(card);
        AssertEx.Equal(2, BehaviorTestContext.EnchantCount(a) + BehaviorTestContext.EnchantCount(b), "Daydream should enchant all cards in hand");
    }

    private static async Task DivineHammerfall()
    {
        using BehaviorTestContext ctx = new();
        DivineHammerfall card = ctx.CardInHand<DivineHammerfall>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(0, BehaviorTestContext.EnchantCount(card), "DivineHammerfall should remove its enchantments");
        AssertEx.Equal(2, BehaviorTestContext.PowerAmount<WeakPower>(ctx.Enemy), "DivineHammerfall should apply Weak");
        AssertEx.Equal(2, BehaviorTestContext.PowerAmount<VulnerablePower>(ctx.Enemy), "DivineHammerfall should apply Vulnerable");
    }

    private static async Task StripLife()
    {
        using BehaviorTestContext ctx = new();
        StripLife card = ctx.CardInHand<StripLife>();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Player.PlayerCombatState!.Energy;
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(0, BehaviorTestContext.EnchantCount(target), "StripLife should remove enchantments");
        AssertEx.Equal(before + 2, ctx.Player.PlayerCombatState.Energy, "StripLife should gain energy");
    }

    private static async Task ShieldOfSacrifice()
    {
        using BehaviorTestContext ctx = new();
        ShieldOfSacrifice card = ctx.CardInHand<ShieldOfSacrifice>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.Equal(14, ctx.Player.Creature.Block, "ShieldOfSacrifice should scale block by removed cards");
    }

    private static async Task SpearOfSacrifice()
    {
        using BehaviorTestContext ctx = new();
        SpearOfSacrifice card = ctx.CardInHand<SpearOfSacrifice>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Nimble, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(18, ctx.HpLost(ctx.Enemy, before), "SpearOfSacrifice should hit once per removed enchanted card");
    }

    private static async Task CyclingEtch()
    {
        using BehaviorTestContext ctx = new();
        CyclingEtch card = ctx.CardInHand<CyclingEtch>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.GetAll(card)[0].Status = MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Disabled;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(card), "CyclingEtch should return to draw");
        AssertEx.Equal(MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Normal, ArchitectEnchantmentHelper.GetAll(card)[0].Status, "CyclingEtch should refresh enchantments");
    }

    private static async Task SkyrendJudgment()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        SkyrendJudgment card = ctx.CardInHand<SkyrendJudgment>();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card);
        AssertEx.Equal(50, ctx.HpLost(ctx.Enemy, before1), "SkyrendJudgment should hit first enemy");
        AssertEx.Equal(50, ctx.HpLost(ctx.SecondEnemy, before2), "SkyrendJudgment should hit second enemy");
    }

    private static async Task TeaOfDrowsiness()
    {
        using BehaviorTestContext ctx = new();
        TeaOfDrowsiness card = ctx.CardInHand<TeaOfDrowsiness>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        await ctx.Play(card);
        AssertEx.Equal(before + 2, ctx.Player.PlayerCombatState.Energy, "TeaOfDrowsiness should gain energy");
        AssertEx.True(ctx.CountInDraw<TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy>() >= 3, "TeaOfDrowsiness should add Drowsy to draw");
    }
}
