using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Enchantments.Framework;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Cards;

public static class V2CardBehaviorTests
{
    [ArchitectTest]
    public static async Task AncientSeedBaseAndUpgradeWithAndWithoutEnchantments()
    {
        foreach (bool upgraded in new[] { false, true })
        foreach (bool enchanted in new[] { false, true })
        {
            using CombatTestContext ctx = new();
            AncientSeed card = ctx.CardInHand<AncientSeed>();
            if (upgraded) card.UpgradeInternal();
            if (enchanted) ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
            int expected = (upgraded ? 3 : 2) * (enchanted ? 2 : 1);
            int hp = ctx.Enemy.CurrentHp;
            await ctx.Play(card, ctx.Enemy);
            AssertEx.Equal(expected + (enchanted ? 1 : 0), ctx.HpLost(ctx.Enemy, hp), "Seed damage includes its conditional bonus and Sharp");
            AssertEx.Equal(expected, ctx.Player.Creature.Block, "Seed block matches v2");
            AssertEx.False(card.Keywords.Contains(CardKeyword.Retain), "Upgrading Seed no longer adds Retain");
        }
    }

    [ArchitectTest]
    public static async Task SweepChangesTargetingAndHitsEachEnemyExactlyOnce()
    {
        foreach (bool upgraded in new[] { false, true })
        foreach (bool enchanted in new[] { false, true })
        {
            using CombatTestContext ctx = new(includeSecondEnemy: true);
            SweepTheHost card = ctx.CardInHand<SweepTheHost>();
            if (upgraded) card.UpgradeInternal();
            if (enchanted) ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
            AssertEx.Equal(enchanted ? TargetType.AllEnemies : TargetType.AnyEnemy, card.TargetType, "Sweep targeting follows enchantment state");
            int hp1 = ctx.Enemy.CurrentHp, hp2 = ctx.SecondEnemy!.CurrentHp;
            await ctx.Play(card, enchanted ? null : ctx.Enemy);
            int expected = (upgraded ? 13 : 10) + (enchanted ? 1 : 0);
            AssertEx.Equal(expected, ctx.HpLost(ctx.Enemy, hp1), "Primary enemy takes exactly one hit");
            AssertEx.Equal(enchanted ? expected : 0, ctx.HpLost(ctx.SecondEnemy, hp2), "Other enemy only hit by enchanted Sweep");
            if (enchanted) ArchitectEnchantmentHelper.RemoveAll(card);
            AssertEx.Equal(TargetType.AnyEnemy, card.TargetType, "Removing enchantment restores single targeting");
        }
    }

    [ArchitectTest]
    public static async Task AncientVerdictCountsFourCombatPilesButNotDeckOrPlayPile()
    {
        foreach (bool upgraded in new[] { false, true })
        {
            using CombatTestContext ctx = new();
            AncientVerdict card = ctx.CardInHand<AncientVerdict>();
            if (upgraded) card.UpgradeInternal();
            var hand = ctx.CardInHand<StrikeArchitect>();
            var draw = ctx.CardInDraw<StrikeArchitect>();
            var discard = ctx.CardInDiscard<StrikeArchitect>();
            var exhaust = ctx.CardInHand<StrikeArchitect>();
            var play = ctx.CardInHand<StrikeArchitect>();
            var deck = ctx.CardInDeck<StrikeArchitect>();
            foreach (var target in new[] { hand, draw, discard, exhaust, play, deck })
                ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1);
            await CardPileCmd.Add(exhaust, PileType.Exhaust);
            await CardPileCmd.Add(play, PileType.Play);
            ctx.CardInDraw<DefendArchitect>();
            int hp = ctx.Enemy.CurrentHp;
            await ctx.Play(card, ctx.Enemy);
            AssertEx.Equal(upgraded ? 28 : 24, ctx.HpLost(ctx.Enemy, hp), "Only four specified combat piles count");
        }
    }

    [ArchitectTest]
    public static async Task LayeredBraceUsesFiveOrSevenPerTrigger()
    {
        foreach (bool upgraded in new[] { false, true })
        {
            using CombatTestContext ctx = new();
            LayeredBrace card = ctx.CardInHand<LayeredBrace>();
            if (upgraded) card.UpgradeInternal();
            await ctx.Play(card);
            await ctx.Play(card);
            AssertEx.Equal(upgraded ? 21 : 15, ctx.Player.Creature.Block, "First and second plays grant one then two triggers");
        }
    }

    [ArchitectTest]
    public static void DeletedCardsAreAbsentFromProductionAssembly()
    {
        var names = typeof(AncientSeed).Assembly.GetTypes().Select(type => type.Name).ToHashSet();
        AssertEx.False(names.Contains("Sketchcleave"), "Sketchcleave is retired");
        AssertEx.False(names.Contains("InstinctAwakened"), "InstinctAwakened is retired");
    }

    [ArchitectTest]
    public static async Task RandomEnchantmentsUseCompatiblePoolAndStandardAmounts()
    {
        using CombatTestContext ctx = new();
        Daydream card = ctx.CardInHand<Daydream>();
        var attack = ctx.CardInHand<StrikeArchitect>();
        var block = ctx.CardInHand<DefendArchitect>();
        var curse = ctx.CardInHand<Drowsy>();
        var already = ctx.CardInHand<StrikeArchitect>();
        ArchitectEnchantmentHelper.Add(already, ArchitectEnchantKind.Sharp, 7);
        var options = new Dictionary<CardModel, IReadOnlyList<ArchitectEnchantOption>>
        {
            [attack] = ArchitectEnchantmentHelper.CompatibleEnchantOptionsFor(attack),
            [block] = ArchitectEnchantmentHelper.CompatibleEnchantOptionsFor(block)
        };
        AssertEx.True(options[attack].Any(option => option.Kind == ArchitectEnchantKind.Momentum), "Pool is not restricted to old basic enchants");
        AssertEx.False(options[block].Any(option => option.Kind == ArchitectEnchantKind.Sharp), "Block-only skills cannot receive Sharp");
        await ctx.Play(card);
        foreach (var entry in options)
        {
            var enchantment = ArchitectEnchantmentHelper.GetAll(entry.Key).Single();
            AssertEx.True(entry.Value.Any(option => ArchitectEnchantmentHelper.Create(option.Kind).GetType() == enchantment.GetType() && option.Amount == enchantment.Amount), "Chosen enchantment was compatible and used its standard amount");
        }
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(curse), "Invalid targets remain unenchanted");
        AssertEx.Equal(7, CombatTestContext.EnchantAmount(already), "Existing enchantment remains unchanged");
    }

    [ArchitectTest]
    public static async Task EternalVerdictSkipsIncompatibleCommonCards()
    {
        using CombatTestContext ctx = new();
        EternalVerdict card = ctx.CardInHand<EternalVerdict>();
        WardedCut valid = ctx.CardInHand<WardedCut>();
        MagicCircle invalid = ctx.CardInHand<MagicCircle>();
        await ctx.Play(card, ctx.Enemy);
        AssertEx.True(CombatTestContext.HasEnchant<TezcatarasEmber>(valid), "Compatible common receives Ember");
        AssertEx.False(CombatTestContext.HasEnchant<TezcatarasEmber>(invalid), "Incompatible card is excluded");
    }

    [ArchitectTest]
    public static async Task WriteDestinyReplacesOnlyTheOriginalDeckEnchantAndPreservesCombatCard()
    {
        using CombatTestContext ctx = new();
        WriteDestiny card = ctx.CardInHand<WriteDestiny>();
        StrikeArchitect deck = ctx.CardInDeck<StrikeArchitect>();
        StrikeArchitect sameName = ctx.CardInDeck<StrikeArchitect>();
        StrikeArchitect combat = ctx.CardInHand<StrikeArchitect>();
        combat.DeckVersion = deck;
        ArchitectEnchantmentHelper.Add(deck, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Add(combat, ArchitectEnchantKind.Sharp, 5);
        await ctx.ApplyPower<InfiniteBlueprintPower>();
        ctx.Select(combat);
        await ctx.Play(card);
        AssertEx.Equal(5, CombatTestContext.EnchantAmount(deck), "Inscription preserves the selected amount without compounding it");
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(sameName), "Unrelated same-name deck card is untouched");
        AssertEx.Equal(5, CombatTestContext.EnchantAmount(combat), "Combat enchantment is retained");
        AssertEx.Equal(2, ctx.Player.Deck.Cards.Count, "Inscription never adds a new card");
        AssertEx.Equal(PileType.Exhaust, ctx.GetResultPile(card), "Inscription exhausts");
        AssertEx.False(WriteDestiny.CanInscribe(ctx.CardInHand<StrikeArchitect>()), "Generated card without a deck original is not selectable");
    }

    [ArchitectTest]
    public static async Task WriteDestinyAllowsChoosingOneOfMultipleEnchantments()
    {
        using CombatTestContext ctx = new();
        WriteDestiny card = ctx.CardInHand<WriteDestiny>();
        WardedCut deck = ctx.CardInDeck<WardedCut>();
        WardedCut combat = ctx.CardInHand<WardedCut>();
        combat.DeckVersion = deck;
        MultiEnchantRegistry.Register(combat);
        ArchitectEnchantmentHelper.Add(combat, ArchitectEnchantKind.Sharp, 2);
        ArchitectEnchantmentHelper.Add(combat, ArchitectEnchantKind.Nimble, 4);
        // Two valid targets force the hand-selection step instead of its single-option shortcut.
        WardedCut other = ctx.CardInHand<WardedCut>();
        other.DeckVersion = ctx.CardInDeck<WardedCut>();
        ArchitectEnchantmentHelper.Add(other, ArchitectEnchantKind.Sharp, 1);
        ctx.Select(combat);
        ctx.SelectIndexes(1);
        await ctx.Play(card);
        AssertEx.True(CombatTestContext.HasEnchant<Nimble>(deck), "Second selected enchantment is inscribed");
        AssertEx.Equal(4, CombatTestContext.EnchantAmount(deck), "Selected stack amount is persisted");
        AssertEx.Equal(2, CombatTestContext.EnchantCount(combat), "Both combat enchantments remain");
    }

    [ArchitectTest]
    public static async Task DrowsyAddsOnlyUnexhaustedNewCopiesAtCombatEnd()
    {
        using CombatTestContext ctx = new();
        Drowsy kept = ctx.CardInHand<Drowsy>();
        Drowsy exhausted = ctx.CardInHand<Drowsy>();
        await CardPileCmd.Add(exhausted, PileType.Exhaust);
        await kept.AfterCombatEnd(null!);
        await exhausted.AfterCombatEnd(null!);
        await kept.AfterCombatEnd(null!);
        AssertEx.Equal(CardType.Curse, kept.Type, "Drowsy is a curse");
        AssertEx.Equal(1, ctx.Player.Deck.Cards.Count, "Only one unexhausted Drowsy persists, without duplicates");
    }

    [ArchitectTest]
    public static async Task PlayingDrowsyRemovesItsDeckOriginalOnly()
    {
        using CombatTestContext ctx = new();
        Drowsy original = ctx.CardInDeck<Drowsy>();
        Drowsy other = ctx.CardInDeck<Drowsy>();
        Drowsy combat = ctx.CardInHand<Drowsy>();
        combat.DeckVersion = original;
        await CardCmd.AutoPlay(ctx.ChoiceContext, combat, null);
        AssertEx.False(ctx.Player.Deck.Cards.Contains(original), "Played Drowsy removes its original");
        AssertEx.True(ctx.Player.Deck.Cards.Contains(other), "Another Drowsy is not removed");
        await combat.AfterCombatEnd(null!);
        AssertEx.Equal(1, ctx.Player.Deck.Cards.Count, "Played Drowsy does not return at combat end");
    }

    [ArchitectTest]
    public static async Task UnplayedDeckDrowsyIsNotDuplicatedOrRemovedByExhaustion()
    {
        using CombatTestContext ctx = new();
        Drowsy original = ctx.CardInDeck<Drowsy>();
        Drowsy combat = ctx.CardInHand<Drowsy>();
        combat.DeckVersion = original;
        await combat.AfterCombatEnd(null!);
        AssertEx.Equal(1, ctx.Player.Deck.Cards.Count, "Existing original is not duplicated");
        await CardPileCmd.Add(combat, PileType.Exhaust);
        await combat.AfterCombatEnd(null!);
        AssertEx.True(ctx.Player.Deck.Cards.Contains(original), "Exhausting without playing does not delete original");
    }
}
