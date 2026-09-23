using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Cards.Uncommon;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Cards;

public static class FullEngineRegressionTests
{
    [ArchitectTest]
    public static async Task EnchantedPlayRewardsUseOwnerAndStartOfPlaySnapshot()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var judgment = ctx.CardInHand<SkyrendJudgment>();
        var allyCard = ctx.MockAttackInHand(owner: ctx.Ally);
        ArchitectEnchantmentHelper.Add(allyCard, ArchitectEnchantKind.Sharp, 1);
        await ctx.PlayFull(allyCard, ctx.Enemy);
        AssertEx.Equal(9, judgment.EnergyCost.GetWithModifiers(CostModifiers.All), "Teammate does not discount owner's Judgment");
        await ctx.ApplyPower<ResonancePower>(amount: 3);
        await ctx.ApplyPower<FormOfCreationPower>(amount: 1);
        var hammer = ctx.CardInHand<DivineHammerfall>();
        ArchitectEnchantmentHelper.Add(hammer, ArchitectEnchantKind.Sharp, 1);
        ctx.CardInDraw<MockSkillCard>();
        int energy = ctx.Player.PlayerCombatState!.Energy;
        int hp = ctx.Enemy.CurrentHp;
        await ctx.PlayFull(hammer, ctx.Enemy);
        AssertEx.Equal(8, judgment.EnergyCost.GetWithModifiers(CostModifiers.All), "Own enchanted play still discounts after stripping itself");
        AssertEx.Equal(20, ctx.HpLost(ctx.Enemy, hp), "Hammer deals 16; its Vulnerable increases Resonance's 3 to 4");
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Creation rewards the enchanted play snapshot");
    }

    [ArchitectTest]
    public static async Task CursePurgeCountsDeckAndCombatCopiesOnceAndPreventsDrowsyReturning()
    {
        foreach (bool upgraded in new[] { false, true })
        {
            using CombatTestContext ctx = new();
            var original = ctx.CardInDeck<Drowsy>();
            var combat = ctx.CardInHand<Drowsy>(); combat.DeckVersion = original;
            var generated = ctx.CardInDraw<Drowsy>();
            var purge = ctx.CardInHand<CursePurge>();
            if (upgraded) purge.UpgradeInternal();
            await ctx.PlayFull(purge);
            AssertEx.Equal(upgraded ? 8 : 6, CombatTestContext.PowerAmount<MegaCrit.Sts2.Core.Models.Powers.PlatingPower>(ctx.Player.Creature), "One original plus one generated curse, not three copies");
            AssertEx.False(ctx.Player.Deck.Cards.Contains(original), "Deck curse is removed");
            AssertEx.False(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(combat), "Combat counterpart is removed");
            await combat.AfterCombatEnd(null!);
            await generated.AfterCombatEnd(null!);
            AssertEx.Equal(0, ctx.Player.Deck.Cards.Count, "Purged Drowsy cards cannot return at combat end");
        }
    }

    [ArchitectTest]
    public static async Task DrowsyIsOneCostPlayableCurseAndDeletesOnlyItsOwnDeckCard()
    {
        using CombatTestContext ctx = new();
        var deck = ctx.CardInDeck<Drowsy>();
        var other = ctx.CardInDeck<Drowsy>();
        var card = ctx.CardInHand<Drowsy>(); card.DeckVersion = deck;
        AssertEx.Equal(1, card.EnergyCost.GetWithModifiers(CostModifiers.All), "Drowsy costs one energy");
        AssertEx.Equal(CardType.Curse, card.Type, "Drowsy is a curse");
        AssertEx.False(card.Keywords.Contains(CardKeyword.Unplayable), "Drowsy can be manually played");
        await ctx.PlayFull(card, ctx.Player.Creature);
        AssertEx.Equal(PileType.Exhaust, card.Pile!.Type, "Played Drowsy exhausts");
        AssertEx.False(ctx.Player.Deck.Cards.Contains(deck), "Corresponding deck card is removed");
        AssertEx.True(ctx.Player.Deck.Cards.Contains(other), "Other copy is retained");
        await card.AfterCombatEnd(null!);
        AssertEx.Equal(1, ctx.Player.Deck.Cards.Count, "Played card is not added again");
    }

    [ArchitectTest]
    public static async Task ReturningCardsGoToDrawOnTheirVeryFirstPlay()
    {
        using CombatTestContext ctx = new();
        var stay = ctx.CardInHand<StayTheBlade>();
        await ctx.PlayFull(stay);
        AssertEx.Equal(PileType.Draw, stay.Pile!.Type, "Stay the Blade returns on first play");
        AssertEx.Equal(2, stay.EnergyCost.GetWithModifiers(CostModifiers.All), "Its next play costs one more");
        var cycle = ctx.CardInHand<CyclingEtch>();
        await ctx.PlayFull(cycle, ctx.Enemy);
        AssertEx.Equal(PileType.Draw, cycle.Pile!.Type, "Cycling Etch returns on first play");
        var grow = ctx.CardInHand<Proliferation>(); grow.UpgradeInternal();
        await ctx.PlayFull(grow, ctx.Enemy);
        AssertEx.Equal(PileType.Draw, grow.Pile!.Type, "Upgraded Proliferation returns on first play");
        var final = ctx.CardInHand<FinalJudgmentOfTheRadiantScepter>(); final.UpgradeInternal();
        await ctx.PlayFull(final, ctx.Enemy);
        AssertEx.True(ReferenceEquals(final, ctx.Player.PlayerCombatState!.DrawPile.Cards[0]), "Upgraded final judgment ends on top after full resolution");
    }

    [ArchitectTest]
    public static async Task CyclingEtchRefreshesAfterItsEnchantmentHasActuallyTriggered()
    {
        using CombatTestContext ctx = new();
        var card = ctx.CardInHand<CyclingEtch>();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Swift, 1);
        for (int i = 0; i < 4; i++) ctx.CardInDraw<MockSkillCard>();
        await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.GetAll(card)[0].Status, "Swift is refreshed after OnPlay disables it");
        int inHand = ctx.Player.PlayerCombatState!.Hand.Cards.Count;
        await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(inHand + 1, ctx.Player.PlayerCombatState.Hand.Cards.Count, "Next play triggers Swift again");
    }

    [ArchitectTest]
    public static async Task RollbackCanMarkAnOffClassCardWithoutChangingItsOriginal()
    {
        using CombatTestContext ctx = new();
        var rollback = ctx.CardInHand<Rollback>();
        var target = ctx.MockAttackInHand();
        ctx.Select(target); await ctx.Play(rollback);
        await ctx.PlayFull(target, ctx.Enemy);
        AssertEx.Equal(PileType.Draw, target.Pile!.Type, "Off-class card receives the shuffle effect");
        var other = ctx.MockAttackInHand(); await ctx.PlayFull(other, ctx.Enemy);
        AssertEx.Equal(PileType.Discard, other.Pile!.Type, "Other instances do not inherit combat-only mark");
    }

    [ArchitectTest]
    public static async Task EnchantedTurnCountersResetAndDoNotIncludeTeammates()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var power = await ctx.ApplyPower<RecuperatePower>(amount: 10);
        var allyCard = ctx.MockAttackInHand(owner: ctx.Ally);
        ArchitectEnchantmentHelper.Add(allyCard, ArchitectEnchantKind.Sharp, 1);
        await ctx.PlayFull(allyCard, ctx.Enemy);
        await power.BeforeTurnEnd(ctx.ChoiceContext, CombatSide.Player);
        AssertEx.Equal(10, ctx.Player.Creature.Block, "Ally enchant plays do not disable Recuperate");
        var own = ctx.MockAttackInHand(); ArchitectEnchantmentHelper.Add(own, ArchitectEnchantKind.Sharp, 1);
        await ctx.PlayFull(own, ctx.Enemy);
        await power.BeforeTurnEnd(ctx.ChoiceContext, CombatSide.Player);
        AssertEx.Equal(10, ctx.Player.Creature.Block, "Own enchant play disables this turn");
        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Player);
        await power.BeforeTurnEnd(ctx.ChoiceContext, CombatSide.Player);
        AssertEx.Equal(20, ctx.Player.Creature.Block, "New turn resets history");
        await power.BeforeTurnEnd(ctx.ChoiceContext, CombatSide.Enemy);
        AssertEx.Equal(20, ctx.Player.Creature.Block, "Enemy turn does not grant block");
    }
}
