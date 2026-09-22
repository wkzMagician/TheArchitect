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
        using CombatTestContext ctx = new();
        GrandOpus card = ctx.CardInHand<GrandOpus>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy, xValue: 2);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "GrandOpus should scale damage with X");
        AssertEx.Equal(8, ctx.Player.Creature.Block, "GrandOpus should scale block with X");
    }

    private static async Task SoulExchange()
    {
        using CombatTestContext ctx = new();
        SoulExchange card = ctx.CardInHand<SoulExchange>();
        MockAttackCard source = ctx.MockAttackInHand();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(source, ArchitectEnchantKind.Sharp, 2m);
        ctx.Select(source);
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(source), "SoulExchange should clear source");
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(target), "SoulExchange should transfer enchantments");
    }

    private static async Task BlightAnointing()
    {
        using CombatTestContext ctx = new();
        BlightAnointing card = ctx.CardInHand<BlightAnointing>();
        MockAttackCard attack = ctx.MockAttackInHand();
        MockSkillCard skill = ctx.MockSkillInHand();
        MockAttackCard futureAttack = ctx.CardInDraw<MockAttackCard>();
        await ctx.Play(card);
        await CardPileCmd.Add(futureAttack, PileType.Hand);
        AssertEx.True(CombatTestContext.HasEnchant<Corrupted>(attack), "BlightAnointing should enchant attacks");
        AssertEx.False(CombatTestContext.HasEnchant<Corrupted>(skill), "BlightAnointing should ignore skills");
        AssertEx.True(CombatTestContext.HasEnchant<Corrupted>(futureAttack), "BlightAnointing should affect future attacks entering hand");
    }

    private static async Task DivineSelection()
    {
        using CombatTestContext ctx = new();
        DivineSelection card = ctx.CardInHand<DivineSelection>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        ctx.SelectIndexes(1);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(target), "DivineSelection should apply the selected offered enchantment");
    }

    private static async Task EternalVerdict()
    {
        using CombatTestContext ctx = new();
        EternalVerdict card = ctx.CardInHand<EternalVerdict>();
        MockAttackCard common = ctx.MockAttackInHand();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "EternalVerdict should deal damage");
        AssertEx.True(CombatTestContext.HasEnchant<TezcatarasEmber>(common), "EternalVerdict should ember a common card");
    }

    private static async Task RadiantMight()
    {
        using CombatTestContext ctx = new();
        RadiantMight card = ctx.CardInHand<RadiantMight>();
        MockAttackCard other = ctx.MockAttackInHand();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before), "RadiantMight should deal damage");
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(other), "RadiantMight should enchant hand with Sharp");
    }

    private static async Task ChorusOfEvasion()
    {
        using CombatTestContext ctx = new();
        ChorusOfEvasion card = ctx.CardInHand<ChorusOfEvasion>();
        MockAttackCard attack = ctx.MockAttackInHand();
        MockSkillCard skill = ctx.MockSkillInHand(block: 5);
        await ctx.Play(card);
        AssertEx.Equal(8, ctx.Player.Creature.Block, "ChorusOfEvasion should give block");
        AssertEx.True(CombatTestContext.HasEnchant<Nimble>(skill), "ChorusOfEvasion should enchant block cards in hand with Nimble");
        AssertEx.False(CombatTestContext.HasEnchant<Nimble>(attack), "ChorusOfEvasion should skip cards that cannot take Nimble");
    }

    private static async Task Daydream()
    {
        using CombatTestContext ctx = new();
        Daydream card = ctx.CardInHand<Daydream>();
        MockAttackCard a = ctx.MockAttackInHand();
        MockSkillCard b = ctx.MockSkillInHand();
        await ctx.Play(card);
        AssertEx.Equal(2, CombatTestContext.EnchantCount(a) + CombatTestContext.EnchantCount(b), "Daydream should enchant all cards in hand");
    }

    private static async Task DivineHammerfall()
    {
        using CombatTestContext ctx = new();
        DivineHammerfall card = ctx.CardInHand<DivineHammerfall>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(card), "DivineHammerfall should remove its enchantments");
        AssertEx.Equal(2, CombatTestContext.PowerAmount<WeakPower>(ctx.Enemy), "DivineHammerfall should apply Weak");
        AssertEx.Equal(2, CombatTestContext.PowerAmount<VulnerablePower>(ctx.Enemy), "DivineHammerfall should apply Vulnerable");
    }

    private static async Task StripLife()
    {
        using CombatTestContext ctx = new();
        StripLife card = ctx.CardInHand<StripLife>();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Player.PlayerCombatState!.Energy;
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(target), "StripLife should remove enchantments");
        AssertEx.Equal(before + 2, ctx.Player.PlayerCombatState.Energy, "StripLife should gain energy");
    }

    private static async Task ShieldOfSacrifice()
    {
        using CombatTestContext ctx = new();
        ShieldOfSacrifice card = ctx.CardInHand<ShieldOfSacrifice>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(block: 5), ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.Equal(14, ctx.Player.Creature.Block, "ShieldOfSacrifice should scale block by removed cards");
    }

    private static async Task SpearOfSacrifice()
    {
        using CombatTestContext ctx = new();
        SpearOfSacrifice card = ctx.CardInHand<SpearOfSacrifice>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(block: 5), ArchitectEnchantKind.Nimble, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(18, ctx.HpLost(ctx.Enemy, before), "SpearOfSacrifice should hit once per removed enchanted card");
    }

    private static async Task CyclingEtch()
    {
        using CombatTestContext ctx = new();
        CyclingEtch card = ctx.CardInHand<CyclingEtch>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.GetAll(card)[0].Status = MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Disabled;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(card), "CyclingEtch should return to draw");
        AssertEx.Equal(MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Normal, ArchitectEnchantmentHelper.GetAll(card)[0].Status, "CyclingEtch should refresh enchantments");
    }

    private static async Task SkyrendJudgment()
    {
        using CombatTestContext ctx = new(includeSecondEnemy: true);
        SkyrendJudgment card = ctx.CardInHand<SkyrendJudgment>();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card);
        AssertEx.Equal(50, ctx.HpLost(ctx.Enemy, before1), "SkyrendJudgment should hit first enemy");
        AssertEx.Equal(50, ctx.HpLost(ctx.SecondEnemy, before2), "SkyrendJudgment should hit second enemy");
    }

    private static async Task TeaOfDrowsiness()
    {
        using CombatTestContext ctx = new();
        TeaOfDrowsiness card = ctx.CardInHand<TeaOfDrowsiness>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        await ctx.Play(card);
        AssertEx.Equal(before + 2, ctx.Player.PlayerCombatState.Energy, "TeaOfDrowsiness should gain energy");
        int drowsy = ctx.CountInDraw<TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy>()
            + ctx.CountInHand<TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy>();
        AssertEx.Equal(3, drowsy, "TeaOfDrowsiness should shuffle 3 Drowsy into the draw pile");
    }
}
