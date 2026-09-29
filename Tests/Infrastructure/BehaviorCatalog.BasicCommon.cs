using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task StrikeArchitect()
    {
        using CombatTestContext ctx = new();
        StrikeArchitect card = ctx.CardInHand<StrikeArchitect>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, before), "StrikeArchitect should deal 6 damage");
    }

    private static async Task DefendArchitect()
    {
        using CombatTestContext ctx = new();
        DefendArchitect card = ctx.CardInHand<DefendArchitect>();
        await ctx.Play(card);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "DefendArchitect should grant 5 block");
    }

    private static async Task Tempering()
    {
        using CombatTestContext ctx = new();
        Tempering card = ctx.CardInHand<Tempering>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(1, CombatTestContext.EnchantCount(target), "Tempering should enchant the selected card");
        AssertEx.Equal(3, CombatTestContext.EnchantAmount(target), "Tempering should add 3 enchant amount");
    }

    private static async Task Sigilbreaker()
    {
        using CombatTestContext ctx = new();
        Sigilbreaker card = ctx.CardInHand<Sigilbreaker>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(block: 5), ArchitectEnchantKind.Nimble, 1m);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(17, ctx.HpLost(ctx.Enemy, before), "Sigilbreaker should scale with other enchanted hand cards but exclude itself");
    }

    private static async Task OpeningDraft()
    {
        using CombatTestContext ctx = new();
        OpeningDraft card = ctx.CardInHand<OpeningDraft>();
        MockAttackCard target = ctx.MockAttackInHand();
        MockSkillCard draw = ctx.CardInDraw<MockSkillCard>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Swift>(target), "OpeningDraft should add Swift");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(draw), "OpeningDraft should draw one card");
    }



    private static async Task MomentumStrike()
    {
        using CombatTestContext ctx = new();
        MomentumStrike card = ctx.CardInHand<MomentumStrike>();
        MockAttackCard targetCard = ctx.MockAttackInHand();
        ctx.Select(targetCard);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(9, ctx.HpLost(ctx.Enemy, before), "MomentumStrike should deal damage");
        AssertEx.True(CombatTestContext.HasEnchant<Momentum>(targetCard), "MomentumStrike should add Momentum");
    }

    private static async Task HolyLight()
    {
        using CombatTestContext ctx = new();
        HolyLight card = ctx.CardInHand<HolyLight>();
        MockAttackCard target = ctx.CardInDraw<MockAttackCard>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Glam>(target), "HolyLight should add Glam to selected draw card");
    }

    private static async Task Guardtrace()
    {
        using CombatTestContext ctx = new();
        Guardtrace card = ctx.CardInHand<Guardtrace>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(11, ctx.Player.Creature.Block, "Guardtrace should grant block");
        AssertEx.True(CombatTestContext.HasEnchant<Adroit>(target), "Guardtrace should add Adroit");
    }

    private static async Task JacobsLadder()
    {
        using CombatTestContext ctx = new();
        JacobsLadder card = ctx.CardInHand<JacobsLadder>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Slither>(target), "JacobsLadder should add Slither");
    }

    private static async Task Whetstone()
    {
        using CombatTestContext ctx = new();
        Whetstone card = ctx.CardInHand<Whetstone>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.Equal(7, ctx.Player.Creature.Block, "Whetstone should give block");
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(target), "Whetstone should add Sharp");
    }

    private static async Task Cultivate()
    {
        using CombatTestContext ctx = new();
        Cultivate card = ctx.CardInHand<Cultivate>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(8, ctx.HpLost(ctx.Enemy, before), "Cultivate should deal damage");
        AssertEx.True(CombatTestContext.HasEnchant<Sown>(target), "Cultivate should add Sown");
    }

    private static async Task Erasure()
    {
        using CombatTestContext ctx = new();
        Erasure card = ctx.CardInHand<Erasure>();
        ChannelPower target = ctx.CardInHand<ChannelPower>();
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<SoulsPower>(target), "Erasure should add SoulsPower");
    }

    private static async Task AncientSeed()
    {
        using CombatTestContext ctx = new();
        AncientSeed card = ctx.CardInHand<AncientSeed>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(1, ctx.HpLost(ctx.Enemy, before), "AncientSeed should deal its current base damage");
        AssertEx.Equal(1, ctx.Player.Creature.Block, "AncientSeed should give its current base block");
    }

    private static async Task BlueprintRevision()
    {
        using CombatTestContext ctx = new();
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
        using CombatTestContext ctx = new(includeSecondEnemy: true);
        ShardBarrage card = ctx.CardInHand<ShardBarrage>();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card);
        AssertEx.Equal(9, ctx.HpLost(ctx.Enemy, before1), "ShardBarrage should hit first enemy 3 times");
        AssertEx.Equal(9, ctx.HpLost(ctx.SecondEnemy, before2), "ShardBarrage should hit second enemy 3 times");
    }



    private static async Task StrokeOfRuin()
    {
        using CombatTestContext ctx = new();
        StrokeOfRuin card = ctx.CardInHand<StrokeOfRuin>();
        MockSkillCard drawn = ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(3, ctx.HpLost(ctx.Enemy, before), "StrokeOfRuin should deal damage");
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(drawn), "StrokeOfRuin should draw one card");
    }

    private static async Task Oracle()
    {
        using CombatTestContext ctx = new();
        Oracle card = ctx.CardInHand<Oracle>();
        ctx.CardInDraw<MockSkillCard>().MockBlock(3);
        ctx.CardInDraw<MockSkillCard>().MockBlock(7);
        CardModel unenchanted = ctx.Player.PlayerCombatState!.DrawPile.Cards.First();
        CardModel target = ctx.Player.PlayerCombatState.DrawPile.Cards.Skip(1).First();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.True(ctx.Player.Creature.Block >= 3, "Oracle should autoplay the enchanted card");
        AssertEx.True(ctx.Player.PlayerCombatState.DrawPile.Cards.Contains(unenchanted), "Oracle should skip unenchanted cards");
        AssertEx.False(ctx.Player.PlayerCombatState.DrawPile.Cards.Contains(target), "Oracle should consume the enchanted card");
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(target), "Oracle should remove the played card's enchantment");
    }

    private static async Task StayTheBlade()
    {
        using CombatTestContext ctx = new();
        StayTheBlade card = ctx.CardInHand<StayTheBlade>();
        await ctx.Play(card);
        AssertEx.Equal(15, ctx.Player.Creature.Block, "StayTheBlade should grant block");
        AssertEx.Equal(PileType.Draw, ctx.GetResultPile(card), "StayTheBlade should shuffle into the draw pile after being played");
    }

    private static async Task SweepTheHost()
    {
        using CombatTestContext ctx = new(includeSecondEnemy: true);
        SweepTheHost card = ctx.CardInHand<SweepTheHost>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(11, ctx.HpLost(ctx.Enemy, before1), "SweepTheHost should hit each enemy only once when enchanted");
        AssertEx.Equal(11, ctx.HpLost(ctx.SecondEnemy, before2), "SweepTheHost should hit other enemies once when enchanted");
    }

    private static async Task WardedCut()
    {
        using CombatTestContext ctx = new();
        WardedCut card = ctx.CardInHand<WardedCut>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(8, ctx.HpLost(ctx.Enemy, before), "WardedCut should deal damage");
        AssertEx.Equal(7, ctx.Player.Creature.Block, "WardedCut should grant block if enchanted");
    }

    private static async Task MonumentHammer()
    {
        using CombatTestContext ctx = new();
        MonumentHammer card = ctx.CardInHand<MonumentHammer>();
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(ctx.MockSkillInHand(block: 5), ArchitectEnchantKind.Nimble, 1m);
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(20, ctx.HpLost(ctx.Enemy, before), "MonumentHammer should scale with enchant count");
    }

    private static async Task CrashingBlow()
    {
        using CombatTestContext ctx = new();
        CrashingBlow card = ctx.CardInHand<CrashingBlow>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(2, CombatTestContext.PowerAmount<VulnerablePower>(ctx.Enemy), "CrashingBlow should add extra Vulnerable if enchanted");
    }

    private static async Task Chant()
    {
        using CombatTestContext ctx = new();
        Chant card = ctx.CardInHand<Chant>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(7, ctx.Player.Creature.Block, "Chant should grant 6 block plus 1 from Nimble");
        AssertEx.Equal(2, CombatTestContext.PowerAmount<WeakPower>(ctx.Enemy), "Chant should add extra Weak if enchanted");
    }

    private static async Task DoublePlatedGuard()
    {
        using CombatTestContext ctx = new();
        DoublePlatedGuard card = ctx.CardInHand<DoublePlatedGuard>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.Equal(14, ctx.Player.Creature.Block, "DoublePlatedGuard should double block if enchanted");
    }

    private static async Task Reforge()
    {
        using CombatTestContext ctx = new();
        Reforge card = ctx.CardInHand<Reforge>();
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Get(target)!.Status = EnchantmentStatus.Disabled;
        await ctx.Play(card);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.Get(target)!.Status, "Reforge should refresh enchantments");
    }

    private static async Task RetrieveTheFragments()
    {
        using CombatTestContext ctx = new();
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
        using CombatTestContext ctx = new();
        Summon card = ctx.CardInHand<Summon>();
        MockAttackCard target = ctx.CardInDraw<MockAttackCard>();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ctx.Select(target);
        await ctx.Play(card);
        AssertEx.True(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(target), "Summon should move selected draw card to hand");
    }

    private static async Task CuratedHand()
    {
        using CombatTestContext ctx = new();
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
