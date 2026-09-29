using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Cards.Uncommon;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Relics;

public static class CalibrationRulerTests
{
    [ArchitectTest]
    public static async Task OnlyFirstSuccessfulNewEnchantmentPerOwnerTurn()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        await ctx.Relic<CalibrationRuler>();
        var card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "First enchant grants 5 block");
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(owner: ctx.Ally), ArchitectEnchantKind.Sharp, 1);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "Further enchants and ally enchants do not trigger");
        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Player);
        AssertEx.True(ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1) == null, "Unsupported stacking fails");
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "Failed enchant does not spend or trigger quota");
        ArchitectEnchantmentHelper.Remove(card);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(10, ctx.Player.Creature.Block, "New turn and rebuilding a stripped card qualify");
    }

    [ArchitectTest]
    public static async Task StackingRefreshAndOtherRelicInstancesAreIndependent()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        await ctx.Relic<CalibrationRuler>();
        await ctx.Relic<CalibrationRuler>(ctx.Ally);
        var card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Player);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Refresh(card);
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(owner: ctx.Ally), ArchitectEnchantKind.Sharp, 1);
        await ArchitectEffectQueue.Drain(ctx.Player);
        await ArchitectEffectQueue.Drain(ctx.Ally!);
        AssertEx.Equal(5, ctx.Player.Creature.Block, "Stacking/refresh is not enchanting an empty card");
        AssertEx.Equal(5, ctx.Ally!.Creature.Block, "Other mutable relic owns independent quota");
    }
}

public static class DismantlingPliersTests
{
    [ArchitectTest]
    public static async Task SingleEnchantmentRemovalGrantsEnergyOnceAndResetsOnOwnerTurn()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        await ctx.Relic<DismantlingPliers>();
        var card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 2);
        int energy = ctx.Player.PlayerCombatState!.Energy;
        AssertEx.True(ArchitectEnchantmentHelper.Remove(card), "The card loses its enchantment");
        AssertEx.False(ArchitectEnchantmentHelper.Remove(card), "Removing an empty card does nothing");
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "One removal grants energy once");

        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Ally!);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Remove(card);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Ally turn does not reset quota");

        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Player);
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Remove(card);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(energy + 2, ctx.Player.PlayerCombatState.Energy, "Owner turn resets quota");
    }

    [ArchitectTest]
    public static async Task TransferReportsOneRemovalAndOneAddition()
    {
        using CombatTestContext ctx = new();
        await ctx.Relic<DismantlingPliers>();
        await ctx.Relic<CalibrationRuler>();
        var source = ctx.MockAttackInHand();
        var target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(source, ArchitectEnchantKind.Sharp, 1);
        await ArchitectEffectQueue.Drain(ctx.Player);
        int energy = ctx.Player.PlayerCombatState!.Energy;
        int block = ctx.Player.Creature.Block;

        ArchitectEnchantmentHelper.Transfer(source, target);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(source), "Transfer removes the source enchantment");
        AssertEx.True(ArchitectEnchantmentHelper.Has<Sharp>(target), "Transfer adds the enchantment to the target");
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Transfer reports one removal");
        AssertEx.Equal(block, ctx.Player.Creature.Block, "Calibration Ruler still triggers only once this turn");
        AssertEx.Equal(2, ArchitectCombatState.CardsEnchantedThisCombat(target), "Initial enchant and transfer each report one addition");

        ArchitectEnchantmentHelper.Refresh(target);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Refresh does not report a removal");
    }
}

public static class CopyistsQuillTests
{
    [ArchitectTest]
    public static async Task RepeatedInstanceDrawsOncePerTurnIncludingEngineReplays()
    {
        using CombatTestContext ctx = new();
        await ctx.Relic<CopyistsQuill>();
        var attack = ctx.MockAttackInHand();
        var first = ctx.CardInDraw<MockSkillCard>();
        var second = ctx.CardInDraw<MockSkillCard>();
        await ctx.PlayFull(attack, ctx.Enemy);
        AssertEx.Equal(2, ctx.Player.PlayerCombatState!.DrawPile.Cards.Count, "First play grants no draw");
        ArchitectCombatState.SetPendingReplays(attack, 2);
        await ctx.PlayFull(attack, ctx.Enemy);
        AssertEx.Equal(1, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "Several replays draw once total");
        AssertEx.Equal(4, ArchitectCombatState.TimesPlayed(attack), "Every engine replay is counted exactly once");
        await Hook.AfterPlayerTurnStart(ctx.CombatState, ctx.ChoiceContext, ctx.Player);
        await ctx.PlayFull(attack, ctx.Enemy, auto: true);
        AssertEx.Equal(0, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "Auto-play qualifies on the new turn");
    }
}

public static class PalimpsestTests
{
    [ArchitectTest]
    public static async Task ThirdPlayOfOneInstanceTriggersOnlyOncePerCombat()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var relic = await ctx.Relic<Palimpsest>();
        var one = ctx.MockAttackInHand();
        var two = ctx.MockAttackInHand();
        var ally = ctx.MockAttackInHand(owner: ctx.Ally);
        for (int i = 0; i < 4; i++) ctx.CardInDraw<MockSkillCard>();
        int energy = ctx.Player.PlayerCombatState!.Energy;
        await ctx.PlayFull(one, ctx.Enemy);
        await ctx.PlayFull(two, ctx.Enemy);
        for (int i = 0; i < 3; i++) await ctx.PlayFull(ally, ctx.Enemy);
        AssertEx.Equal(energy, ctx.Player.PlayerCombatState.Energy, "Different instances and ally plays do not aggregate");
        ArchitectCombatState.SetPendingReplays(one, 1);
        await ctx.PlayFull(one, ctx.Enemy);
        AssertEx.Equal(energy + 2, ctx.Player.PlayerCombatState.Energy, "Third play grants two energy");
        AssertEx.Equal(2, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "Third play draws two");
        await ctx.PlayFull(two, ctx.Enemy);
        await ctx.PlayFull(two, ctx.Enemy);
        AssertEx.Equal(energy + 2, ctx.Player.PlayerCombatState.Energy, "Second card cannot claim another combat reward");
        await relic.BeforeCombatStart();
        for (int i = 0; i < 3; i++) await ctx.PlayFull(one, ctx.Enemy);
        AssertEx.Equal(energy + 4, ctx.Player.PlayerCombatState.Energy, "New combat resets reward and instance counts");
    }
}

public static class ReflowInkwellTests
{
    [ArchitectTest]
    public static async Task RequiresThreeDistinctCardsAndReturnsOnlyOwnersDiscardToTop()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var relic = await ctx.Relic<ReflowInkwell>();
        var first = ctx.MockAttackInHand();
        var second = ctx.MockAttackInHand();
        var third = ctx.MockAttackInHand();
        foreach (var card in new[] { first, second, third }) ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1);
        for (int i = 0; i < 3; i++) await ctx.PlayFull(first, ctx.Enemy);
        await relic.AfterSideTurnEnd(ctx.ChoiceContext, CombatSide.Player, [ctx.Player.Creature]);
        AssertEx.Equal(0, ctx.Player.PlayerCombatState!.DrawPile.Cards.Count, "Three plays of one instance do not qualify");
        await ctx.PlayFull(second, ctx.Enemy);
        await ctx.PlayFull(third, ctx.Enemy);
        var bottom = ctx.CardInDraw<MockSkillCard>();
        await CardPileCmd.Add(first, PileType.Exhaust);
        await CardPileCmd.Add(second, PileType.Exhaust);
        await Hook.AfterTurnEnd(ctx.CombatState, CombatSide.Player, [ctx.Player.Creature]);
        AssertEx.True(ReferenceEquals(third, ctx.Player.PlayerCombatState.DrawPile.Cards[0]), "Only eligible discarded card is on top");
        await relic.AfterSideTurnEnd(ctx.ChoiceContext, CombatSide.Player, [ctx.Player.Creature]);
        AssertEx.Equal(2, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "End phase cannot trigger twice");
    }
}

public static class BlankCodexTests
{
    [ArchitectTest]
    public static async Task OrdinaryPlayDrawsTwoNextTurn()
    {
        using CombatTestContext ctx = new();
        var relic = await ctx.Relic<BlankCodex>();
        var first = ctx.CardInDraw<MockSkillCard>();
        var second = ctx.CardInDraw<MockSkillCard>();

        await relic.BeforeSideTurnEnd(ctx.ChoiceContext, CombatSide.Player, [ctx.Player.Creature]);
        await relic.AfterPlayerTurnStartEarly(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(ctx.Player.PlayerCombatState!.DrawPile.Cards.Contains(first), "Idle turn does not schedule a draw");

        await ctx.PlayFull(ctx.MockAttackInHand(), ctx.Enemy);
        await relic.BeforeSideTurnEnd(ctx.ChoiceContext, CombatSide.Player, [ctx.Player.Creature]);
        await relic.AfterPlayerTurnStartEarly(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(ctx.Player.PlayerCombatState.Hand.Cards.Contains(first), "First bonus card is drawn");
        AssertEx.True(ctx.Player.PlayerCombatState.Hand.Cards.Contains(second), "Second bonus card is drawn");
    }

    [ArchitectTest]
    public static async Task EnchantedPlayDoesNotQualifyAfterItsEnchantmentIsRemoved()
    {
        using CombatTestContext ctx = new();
        var relic = await ctx.Relic<BlankCodex>();
        var hammer = ctx.CardInHand<DivineHammerfall>();
        ArchitectEnchantmentHelper.Add(hammer, ArchitectEnchantKind.Sharp, 1);
        await ctx.PlayFull(hammer, ctx.Enemy);
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(hammer), "Hammer removes its own enchantment");

        var draw = ctx.CardInDraw<MockSkillCard>();
        await relic.BeforeSideTurnEnd(ctx.ChoiceContext, CombatSide.Player, [ctx.Player.Creature]);
        await relic.AfterPlayerTurnStartEarly(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(ctx.Player.PlayerCombatState!.DrawPile.Cards.Contains(draw), "Start-of-play enchantment disqualifies the bonus draw");
    }
}

public static class FinalizingSealTests
{
    [ArchitectTest]
    public static async Task RefreshesOnlySelectedDeckInstanceAndNeverRestoresRemovedEnchantments()
    {
        using CombatTestContext ctx = new();
        var selected = ctx.CardInDeck<StrikeArchitect>();
        var other = ctx.CardInDeck<StrikeArchitect>();
        var relic = await ctx.Relic<FinalizingSeal>();
        ctx.Select(selected);
        await relic.AfterObtained();
        var combat = ctx.CardInHand<StrikeArchitect>(); combat.DeckVersion = selected;
        var duplicate = ctx.CardInHand<StrikeArchitect>(); duplicate.DeckVersion = other;
        ArchitectEnchantmentHelper.Add(combat, ArchitectEnchantKind.Swift, 1);
        ArchitectEnchantmentHelper.Add(duplicate, ArchitectEnchantKind.Swift, 1);
        for (int i = 0; i < 4; i++) ctx.CardInDraw<MockSkillCard>();
        await ctx.PlayFull(combat, ctx.Enemy);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.Get(combat)!.Status, "Selected Swift is refreshed after it triggers");
        int remaining = ctx.Player.PlayerCombatState!.DrawPile.Cards.Count;
        await ctx.PlayFull(combat, ctx.Enemy);
        AssertEx.Equal(remaining - 1, ctx.Player.PlayerCombatState.DrawPile.Cards.Count, "Refreshed enchantment triggers again");
        await ctx.PlayFull(duplicate, ctx.Enemy);
        AssertEx.True(ArchitectEnchantmentHelper.Get(duplicate)!.Status != EnchantmentStatus.Normal, "Same model with another deck identity is not sealed");
        var clone = combat.CreateClone();
        clone.DeckVersion = selected;
        await ctx.PlayFull(clone, ctx.Enemy);
        AssertEx.True(ArchitectEnchantmentHelper.Get(clone)!.Status != EnchantmentStatus.Normal, "Combat clone cannot inherit seal even with a copied deck reference");
        ArchitectEnchantmentHelper.Remove(combat);
        await ctx.PlayFull(combat, ctx.Enemy);
        AssertEx.False(ArchitectEnchantmentHelper.HasAny(combat), "Seal never recreates removed enchantments");
    }

    [ArchitectTest]
    public static async Task SelectionSurvivesSaveAndDeckReindexingButNotTargetRemoval()
    {
        using CombatTestContext ctx = new();
        var first = ctx.CardInDeck<StrikeArchitect>();
        var selected = ctx.CardInDeck<StrikeArchitect>();
        var relic = await ctx.Relic<FinalizingSeal>();
        ctx.Select(selected); await relic.AfterObtained();
        await CardPileCmd.RemoveFromDeck(first);
        AssertEx.Equal(0, relic.SelectedDeckIndex, "Saving resolves current position rather than stale pickup index");
        var restored = (FinalizingSeal)RelicModel.FromSerializable(relic.ToSerializable());
        ctx.Player.RemoveRelicInternal(relic, silent: true);
        ctx.Player.AddRelicInternal(restored, silent: true);
        await restored.BeforeRoomEntered(null!);
        AssertEx.True(ReferenceEquals(selected, restored.SelectedCard), "Restored selection points to exactly that deck instance");
        await CardPileCmd.RemoveFromDeck(selected);
        ctx.CardInDeck<StrikeArchitect>();
        AssertEx.True(restored.SelectedCard == null, "Removing selected card cannot move seal to a new occupant");
        AssertEx.Equal(-1, restored.SelectedDeckIndex, "Lost target remains unselected in saves");
    }
}
