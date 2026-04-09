using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task StrikeArchitect()
    {
        using BehaviorTestContext ctx = new();
        StrikeArchitect card = ctx.CardInHand<StrikeArchitect>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, before), "StrikeArchitect should deal 6 damage");
    }

    private static async Task DefendArchitect()
    {
        using BehaviorTestContext ctx = new();
        DefendArchitect card = ctx.CardInHand<DefendArchitect>();
        await ctx.Play(card);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "DefendArchitect should grant 5 block");
    }

    private static async Task Tempering()
    {
        using BehaviorTestContext ctx = new();
        Tempering card = ctx.CardInHand<Tempering>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(1, BehaviorTestContext.EnchantCount(target), "Tempering should enchant the selected card");
        AssertEx.Equal(3, BehaviorTestContext.EnchantAmount(target), "Tempering should add 3 enchant amount");
    }

    private static async Task Sigilbreaker()
    {
        using BehaviorTestContext ctx = new();
        Sigilbreaker card = ctx.CardInHand<Sigilbreaker>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Nimble, 1m);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(16, ctx.HpLost(ctx.Enemy, before), "Sigilbreaker should scale with other enchanted hand cards but exclude itself");
    }

    private static async Task OpeningDraft()
    {
        using BehaviorTestContext ctx = new();
        OpeningDraft card = ctx.CardInHand<OpeningDraft>();
        MockAttackCard target = ctx.MockAttackInHand();
        MockSkillCard draw = ctx.CardInDraw<MockSkillCard>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Swift>(target), "OpeningDraft should add Swift");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(draw), "OpeningDraft should draw one card");
    }

    private static async Task InstinctAwakened()
    {
        using BehaviorTestContext ctx = new();
        InstinctAwakened card = ctx.CardInHand<InstinctAwakened>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Instinct>(target), "InstinctAwakened should add Instinct");
    }

    private static async Task MomentumStrike()
    {
        using BehaviorTestContext ctx = new();
        MomentumStrike card = ctx.CardInHand<MomentumStrike>();
        MockAttackCard targetCard = ctx.MockAttackInHand();
        ctx.Select(targetCard);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(9, ctx.HpLost(ctx.Enemy, before), "MomentumStrike should deal damage");
        AssertEx.True(BehaviorTestContext.HasEnchant<Momentum>(targetCard), "MomentumStrike should add Momentum");
    }

    private static async Task HolyLight()
    {
        using BehaviorTestContext ctx = new();
        HolyLight card = ctx.CardInHand<HolyLight>();
        MockAttackCard target = ctx.CardInDraw<MockAttackCard>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Glam>(target), "HolyLight should add Glam to selected draw card");
    }

    private static async Task Guardtrace()
    {
        using BehaviorTestContext ctx = new();
        Guardtrace card = ctx.CardInHand<Guardtrace>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(11, ctx.Player.Creature.Block, "Guardtrace should grant block");
        AssertEx.True(BehaviorTestContext.HasEnchant<Adroit>(target), "Guardtrace should add Adroit");
    }

    private static async Task JacobsLadder()
    {
        using BehaviorTestContext ctx = new();
        JacobsLadder card = ctx.CardInHand<JacobsLadder>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Slither>(target), "JacobsLadder should add Slither");
    }

    private static async Task Whetstone()
    {
        using BehaviorTestContext ctx = new();
        Whetstone card = ctx.CardInHand<Whetstone>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(7, ctx.Player.Creature.Block, "Whetstone should give block");
        AssertEx.True(BehaviorTestContext.HasEnchant<Sharp>(target), "Whetstone should add Sharp");
    }

    private static async Task Cultivate()
    {
        using BehaviorTestContext ctx = new();
        Cultivate card = ctx.CardInHand<Cultivate>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(8, ctx.HpLost(ctx.Enemy, before), "Cultivate should deal damage");
        AssertEx.True(BehaviorTestContext.HasEnchant<Sown>(target), "Cultivate should add Sown");
    }

    private static async Task Erasure()
    {
        using BehaviorTestContext ctx = new();
        Erasure card = ctx.CardInHand<Erasure>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<SoulsPower>(target), "Erasure should add SoulsPower");
    }

    private static async Task AncientSeed()
    {
        using BehaviorTestContext ctx = new();
        AncientSeed card = ctx.CardInHand<AncientSeed>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(1, ctx.HpLost(ctx.Enemy, before), "AncientSeed should deal 1 damage");
        AssertEx.Equal(1, ctx.Player.Creature.Block, "AncientSeed should give 1 block");
    }

    private static async Task BlueprintRevision()
    {
        using BehaviorTestContext ctx = new();
        BlueprintRevision card = ctx.CardInHand<BlueprintRevision>();
        MockAttackCard draw = ctx.CardInDraw<MockAttackCard>();
        MockSkillCard discard = ctx.CardInDiscard<MockSkillCard>();
        ctx.Select(draw);
        ctx.Select(discard);
        await ctx.Play(card);
        AssertEx.Equal(3, ctx.Player.Creature.Block, "BlueprintRevision should grant block");
        AssertEx.True(ctx.Player.PlayerCombatState!.DiscardPile.Cards.Contains(draw), "BlueprintRevision should move selected draw card");
        AssertEx.True(ctx.Player.PlayerCombatState.DrawPile.Cards.Contains(discard), "BlueprintRevision should move selected discard card");
    }

    private static async Task ShardBarrage()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        ShardBarrage card = ctx.CardInHand<ShardBarrage>();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card);
        AssertEx.Equal(9, ctx.HpLost(ctx.Enemy, before1), "ShardBarrage should hit first enemy 3 times");
        AssertEx.Equal(9, ctx.HpLost(ctx.SecondEnemy, before2), "ShardBarrage should hit second enemy 3 times");
    }

    private static async Task Sketchcleave()
    {
        using BehaviorTestContext ctx = new();
        Sketchcleave card = ctx.CardInHand<Sketchcleave>();
        MockSkillCard a = ctx.CardInDraw<MockSkillCard>();
        MockSkillCard b = ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, before), "Sketchcleave should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(a) && ctx.Player.PlayerCombatState.Hand.Cards.Contains(b), "Sketchcleave should draw two cards");
    }

    private static async Task StrokeOfRuin()
    {
        using BehaviorTestContext ctx = new();
        StrokeOfRuin card = ctx.CardInHand<StrokeOfRuin>();
        MockSkillCard drawn = ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(5, ctx.HpLost(ctx.Enemy, before), "StrokeOfRuin should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(drawn), "StrokeOfRuin should draw one card");
    }

    private static async Task Oracle()
    {
        using BehaviorTestContext ctx = new();
        Oracle card = ctx.CardInHand<Oracle>();
        MockSkillCard top = ctx.CardInDraw<MockSkillCard>().MockBlock(7);
        await ctx.Play(card);
        AssertEx.Equal(7, ctx.Player.Creature.Block, "Oracle should autoplay the top card");
        AssertEx.False(ctx.Player.PlayerCombatState!.DrawPile.Cards.Contains(top), "Oracle should consume the top card");
    }

    private static async Task StayTheBlade()
    {
        using BehaviorTestContext ctx = new();
        StayTheBlade card = ctx.CardInHand<StayTheBlade>();
        await ctx.Play(card);
        AssertEx.Equal(15, ctx.Player.Creature.Block, "StayTheBlade should grant block");
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(card), "StayTheBlade should return to draw pile");
    }

    private static async Task SweepTheHost()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        SweepTheHost card = ctx.CardInHand<SweepTheHost>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(10, ctx.HpLost(ctx.Enemy, before1), "SweepTheHost should hit target once when enchanted");
        AssertEx.Equal(10, ctx.HpLost(ctx.SecondEnemy, before2), "SweepTheHost should target all enemies when enchanted");
    }

    private static async Task WardedCut()
    {
        using BehaviorTestContext ctx = new();
        WardedCut card = ctx.CardInHand<WardedCut>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(7, ctx.HpLost(ctx.Enemy, before), "WardedCut should deal damage");
        AssertEx.Equal(7, ctx.Player.Creature.Block, "WardedCut should grant block if enchanted");
    }

    private static async Task MonumentHammer()
    {
        using BehaviorTestContext ctx = new();
        MonumentHammer card = ctx.CardInHand<MonumentHammer>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(), ArchitectEnchantKind.Nimble, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(16, ctx.HpLost(ctx.Enemy, before), "MonumentHammer should scale with enchant count");
    }

    private static async Task CrashingBlow()
    {
        using BehaviorTestContext ctx = new();
        CrashingBlow card = ctx.CardInHand<CrashingBlow>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(2, BehaviorTestContext.PowerAmount<VulnerablePower>(ctx.Enemy), "CrashingBlow should add extra Vulnerable if enchanted");
    }

    private static async Task Chant()
    {
        using BehaviorTestContext ctx = new();
        Chant card = ctx.CardInHand<Chant>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "Chant should grant block");
        AssertEx.Equal(2, BehaviorTestContext.PowerAmount<WeakPower>(ctx.Enemy), "Chant should add extra Weak if enchanted");
    }

    private static async Task DoublePlatedGuard()
    {
        using BehaviorTestContext ctx = new();
        DoublePlatedGuard card = ctx.CardInHand<DoublePlatedGuard>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card);
        AssertEx.Equal(16, ctx.Player.Creature.Block, "DoublePlatedGuard should double block if enchanted");
    }

    private static async Task Reforge()
    {
        using BehaviorTestContext ctx = new();
        Reforge card = ctx.CardInHand<Reforge>();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.GetAll(target)[0].Status = EnchantmentStatus.Disabled;
        await ctx.Play(card);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.GetAll(target)[0].Status, "Reforge should refresh enchantments");
    }

    private static async Task RetrieveTheFragments()
    {
        using BehaviorTestContext ctx = new();
        RetrieveTheFragments card = ctx.CardInHand<RetrieveTheFragments>();
        MockAttackCard targetCard = ctx.CardInDiscard<MockAttackCard>();
        ArchitectEnchantmentHelper.Add(targetCard, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(4, ctx.HpLost(ctx.Enemy, before), "RetrieveTheFragments should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(targetCard), "RetrieveTheFragments should return enchanted discard");
    }

    private static async Task Summon()
    {
        using BehaviorTestContext ctx = new();
        Summon card = ctx.CardInHand<Summon>();
        MockAttackCard target = ctx.CardInDraw<MockAttackCard>();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(target), "Summon should move selected draw card to hand");
    }

    private static async Task CuratedHand()
    {
        using BehaviorTestContext ctx = new();
        CuratedHand card = ctx.CardInHand<CuratedHand>();
        MockAttackCard keep = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(keep, ArchitectEnchantKind.Sharp, 1m);
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(7, ctx.HpLost(ctx.Enemy, before), "CuratedHand should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(keep), "CuratedHand should keep enchanted cards");
        AssertEx.Equal(0, ctx.Player.PlayerCombatState.Hand.Cards.OfType<MockSkillCard>().Count(), "CuratedHand should discard non-enchanted cards");
    }
}
