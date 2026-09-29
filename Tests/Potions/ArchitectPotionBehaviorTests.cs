using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Cards.Uncommon;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Potions;
using TheArchitect.TheArchitectCode.Powers.Architect;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Potions;

public static class InscriptionPotionTests
{
    [ArchitectTest]
    public static async Task CompatibleEnchantmentIsCombatOnlyAndConsumesBottle()
    {
        using CombatTestContext ctx = new();
        var original = ctx.CardInDeck<StrikeArchitect>();
        var card = ctx.CardInHand<StrikeArchitect>(); card.DeckVersion = original;
        var potion = ctx.Potion<InscriptionPotion>();
        AssertEx.True(potion.PassesCustomUsabilityCheck, "Attack is eligible");
        ctx.Select(card);
        await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(card), "Attack receives Sharp");
        AssertEx.Equal(6, CombatTestContext.EnchantAmount(card), "Base potency is six");
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(original), "No permanent enchantment leaks to deck");
        AssertEx.False(ctx.Player.Potions.Contains(potion), "Successful use consumes bottle");
    }

    [ArchitectTest]
    public static async Task DualPurposeChoiceAndModifiedPotency()
    {
        using CombatTestContext ctx = new();
        var card = ctx.CardInHand<AncientSeed>();
        ctx.MockAttackInHand();
        var potion = ctx.Potion<InscriptionPotion>();
        potion.DynamicVars["EnchantAmount"].UpgradeValueBy(6);
        ctx.Select(card); ctx.SelectIndexes(1);
        await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.True(CombatTestContext.HasEnchant<Nimble>(card), "Second option grants Nimble");
        AssertEx.Equal(12, CombatTestContext.EnchantAmount(card), "Modified potency applies");
    }

    [ArchitectTest]
    public static async Task NoTargetIsUnusableAndCancelledSelectionRefundsBottle()
    {
        using CombatTestContext ctx = new();
        var potion = ctx.Potion<InscriptionPotion>();
        AssertEx.False(potion.PassesCustomUsabilityCheck, "Empty hand cannot consume potion");
        ctx.MockAttackInHand(); ctx.MockAttackInHand(); ctx.Select();
        await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.True(ctx.Player.Potions.Contains(potion), "Cancelled selection refunds bottle");
    }
}

public static class RevisionSolventTests
{
    [ArchitectTest]
    public static async Task RewardsCountSelectedCardsAndDoNotStripNewDraws()
    {
        using CombatTestContext ctx = new();
        await ctx.Relic<DismantlingPliers>();
        await ctx.ApplyPower<DestroyerPower>(amount: 1);
        var selected = Enumerable.Range(0, 3).Select(_ => ctx.MockAttackInHand()).ToArray();
        foreach (var card in selected)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        }
        var drawn = ctx.CardInDraw<StrikeArchitect>();
        ArchitectEnchantmentHelper.Add(drawn, ArchitectEnchantKind.Sharp, 1);
        ctx.CardInDraw<MockSkillCard>(); ctx.CardInDraw<MockSkillCard>();
        int energy = ctx.Player.PlayerCombatState!.Energy;
        var potion = ctx.Potion<RevisionSolvent>();
        ctx.Select(selected);
        await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.True(selected.All(card => !ArchitectEnchantmentHelper.HasAny(card)), "Selected enchantments are removed");
        AssertEx.Equal(12, ctx.Player.Creature.Block, "Three selected cards grant twelve block");
        AssertEx.Equal(3, CombatTestContext.PowerAmount<StrengthPower>(ctx.Player.Creature), "Destroyer triggers per card");
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Pliers triggers once");
        AssertEx.Equal(0, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "Exactly three cards drawn");
        AssertEx.True(ArchitectEnchantmentHelper.HasAny(drawn), "Drawn card is outside selection snapshot");
    }

    [ArchitectTest]
    public static async Task ZeroChoiceRefundsAndModifiedPotencyChangesRewards()
    {
        using CombatTestContext ctx = new();
        var target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1);
        var potion = ctx.Potion<RevisionSolvent>();
        ctx.Select(); await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.True(ctx.Player.Potions.Contains(potion), "Zero selected cards refund solvent");
        potion.DynamicVars.Cards.UpgradeValueBy(1); potion.DynamicVars.Block.UpgradeValueBy(4);
        ctx.CardInDraw<MockSkillCard>(); ctx.CardInDraw<MockSkillCard>();
        ctx.Select(target); await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        AssertEx.Equal(8, ctx.Player.Creature.Block, "Modified reward gives eight block per card");
        AssertEx.Equal(0, ctx.Player.PlayerCombatState!.DrawPile.Cards.Count, "Modified reward draws two per card");
        AssertEx.False(potion.PassesCustomUsabilityCheck, "No targets remain");
    }
}

public static class RepriseElixirTests
{
    [ArchitectTest]
    public static async Task ManualReplayDoesNotRecurseOrChangeFutureCost()
    {
        using CombatTestContext ctx = new();
        var card = ctx.CardInHand<StrikeArchitect>();
        var potion = ctx.Potion<RepriseElixir>();
        ctx.Select(card); await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        int hp = ctx.Enemy.CurrentHp;
        await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(12, ctx.HpLost(ctx.Enemy, hp), "One manual strike plus one replay");
        AssertEx.Equal(2, ArchitectCombatState.TimesPlayed(card), "Both plays recorded once");
        AssertEx.Equal(PileType.Discard, card.Pile!.Type, "Final pile resolved once");
        AssertEx.Equal(1, card.EnergyCost.GetWithModifiers(CostModifiers.All), "Future plays are not made free");
        hp = ctx.Enemy.CurrentHp; await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, hp), "Replay mark is consumed");
    }

    [ArchitectTest]
    public static async Task AutoPlayPreservesPotionMarkAndOwnerTurnEndExpiresIt()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var card = ctx.CardInHand<StrikeArchitect>();
        var potion = ctx.Potion<RepriseElixir>();
        ctx.Select(card); await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        int hp = ctx.Enemy.CurrentHp; await ctx.PlayFull(card, ctx.Enemy, auto: true);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, hp), "Auto-play does not consume mark");
        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Ally!);
        hp = ctx.Enemy.CurrentHp; await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(12, ctx.HpLost(ctx.Enemy, hp), "Ally turn preserves mark for manual play");
        ArchitectCombatState.SetPotionReplays(card, 1);
        await Hook.BeforeTurnEnd(ctx.CombatState, CombatSide.Player, [ctx.Player.Creature]);
        hp = ctx.Enemy.CurrentHp; await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(6, ctx.HpLost(ctx.Enemy, hp), "Unused mark expires at owner turn end");
    }

    [ArchitectTest]
    public static async Task ChannelAndPotionStackWithCorrectGrowthAndFinalExhaustPile()
    {
        using CombatTestContext ctx = new();
        var card = ctx.CardInHand<LayeredBrace>();
        var potion = ctx.Potion<RepriseElixir>();
        potion.DynamicVars["Replays"].UpgradeValueBy(1);
        ctx.Select(card); await potion.OnUseWrapper(ctx.ChoiceContext, ctx.Player.Creature);
        ArchitectCombatState.SetPendingReplays(card, 1);
        await ctx.PlayFull(card);
        AssertEx.Equal(50, ctx.Player.Creature.Block, "Four plays produce 5 + 10 + 15 + 20 block");
        AssertEx.Equal(4, ArchitectCombatState.TimesPlayed(card), "All three extra replays counted");
        var exhaust = ctx.CardInHand<PrimedSpark>();
        for (int i = 0; i < 6; i++) ctx.CardInDraw<MockSkillCard>();
        ArchitectCombatState.SetPendingReplays(exhaust, 1);
        await ctx.PlayFull(exhaust);
        AssertEx.Equal(PileType.Exhaust, exhaust.Pile!.Type, "Exhaust happens once after all replays");
    }

    [ArchitectTest]
    public static async Task ChannelAppliesToAutomaticPlayToo()
    {
        using CombatTestContext ctx = new();
        var card = ctx.CardInHand<StrikeArchitect>();
        ArchitectCombatState.SetPendingReplays(card, 2);
        int hp = ctx.Enemy.CurrentHp; await ctx.PlayFull(card, ctx.Enemy, auto: true);
        AssertEx.Equal(18, ctx.HpLost(ctx.Enemy, hp), "Channel applies to next play including automatic play");
    }
}
