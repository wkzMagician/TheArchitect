using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Cards.Uncommon;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Cards;

public static class ArchitectCardHighlightTests
{
    [ArchitectTest]
    public static async Task CreationCountsEnchantedPlaysBeforeApplication()
    {
        using CombatTestContext ctx = new();
        var first = ctx.MockAttackInHand();
        var second = ctx.MockAttackInHand();
        var third = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(first, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Add(second, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Add(third, ArchitectEnchantKind.Sharp, 1);
        await ctx.PlayFull(first, ctx.Enemy);
        await ctx.ApplyPower<FormOfCreationPower>(amount: 1);
        AssertEx.False(second.ShouldGlowGold, "Earlier enchanted plays consume Creation's quota");
        await ctx.ApplyPower<FormOfCreationPower>(amount: 1);
        AssertEx.True(second.ShouldGlowGold, "Stacking extends the first-N threshold this turn");
        ctx.CardInDraw<MockSkillCard>();
        int energy = ctx.Player.PlayerCombatState!.Energy;
        await ctx.PlayFull(second, ctx.Enemy);
        AssertEx.Equal(energy + 1, ctx.Player.PlayerCombatState.Energy, "Second enchanted play receives the second reward");
        AssertEx.False(third.ShouldGlowGold, "Third enchanted play exceeds two layers");
        energy = ctx.Player.PlayerCombatState.Energy;
        await ctx.PlayFull(third, ctx.Enemy);
        AssertEx.Equal(energy, ctx.Player.PlayerCombatState.Energy, "Out-of-quota play receives no reward");
        ArchitectCombatState.OnTurnStart(ctx.Player);
        AssertEx.True(second.ShouldGlowGold, "New turn resets the first-N threshold");
    }

    [ArchitectTest]
    public static Task PlayThresholdsAndCanonicalModels()
    {
        using CombatTestContext ctx = new();
        Check(ctx.CardInHand<Trinity>(), 2);
        Check(ctx.CardInHand<FinalJudgmentOfTheRadiantScepter>(), 4);
        Check(ctx.CardInHand<Ascend>(), 1);
        AssertEx.False(ModelDb.Card<Trinity>().ShouldGlowGold, "Compendium cards have no combat glow");
        AssertEx.True(ctx.CardInHand<Ftl>().ShouldGlowGold, "Native gold glow is preserved");
        return Task.CompletedTask;

        void Check(CardModel card, int threshold)
        {
            AssertEx.False(card.ShouldGlowGold, "Fresh card has no conditional glow");
            ctx.MarkPlayed(card, threshold - 1);
            AssertEx.False(card.ShouldGlowGold, "Below the next-play threshold");
            ctx.MarkPlayed(card);
            AssertEx.True(card.ShouldGlowGold, "Next play activates its bonus");
        }
    }

    [ArchitectTest]
    public static Task EnchantmentBonusesAndOriginalSin()
    {
        using CombatTestContext ctx = new();
        CardModel[] cards = [ctx.CardInHand<SweepTheHost>(), ctx.CardInHand<WardedCut>(),
            ctx.CardInHand<CrashingBlow>(), ctx.CardInHand<Chant>(), ctx.CardInHand<DoublePlatedGuard>(),
            ctx.CardInHand<DivineHammerfall>()];
        var sin = ctx.CardInHand<OriginalSin>();
        AssertEx.True(sin.ShouldGlowGold, "Original Sin satisfies its play condition");
        foreach (CardModel card in cards)
        {
            AssertEx.False(card.ShouldGlowGold, "Unenchanted card has no bonus");
            ArchitectEnchantmentHelper.Add(card, card.Type == CardType.Attack ? ArchitectEnchantKind.Sharp : ArchitectEnchantKind.Nimble, 1);
            AssertEx.True(card.ShouldGlowGold, "Enchanted card has its conditional bonus");
            AssertEx.False(sin.ShouldGlowGold, "Original Sin is blocked by an enchanted hand card");
            card.Enchantment!.Status = EnchantmentStatus.Disabled;
            AssertEx.True(card.ShouldGlowGold, "Having an enchantment still satisfies these card bonuses");
            ArchitectEnchantmentHelper.Remove(card);
            AssertEx.False(card.ShouldGlowGold, "Removing the enchantment removes its glow");
        }
        AssertEx.True(sin.ShouldGlowGold, "Original Sin becomes available again");
        return Task.CompletedTask;
    }

    [ArchitectTest]
    public static Task RefreshCardsOnlyGlowForInactiveEnchantments()
    {
        using CombatTestContext ctx = new();
        var reforge = ctx.CardInHand<Reforge>();
        CardModel[] cards = [ctx.CardInHand<AncientSeed>(), ctx.CardInHand<CyclingEtch>()];
        foreach (CardModel card in cards)
        {
            AssertEx.False(card.ShouldGlowGold, "No enchantment to refresh");
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Swift, 1);
            AssertEx.False(card.ShouldGlowGold, "Active enchantment needs no recovery");
            card.Enchantment!.Status = EnchantmentStatus.Disabled;
            AssertEx.True(card.ShouldGlowGold, "Inactive self enchantment can be refreshed");
            AssertEx.True(reforge.ShouldGlowGold, "Reforge has an inactive hand target");
            ArchitectEnchantmentHelper.Refresh(card);
            AssertEx.False(card.ShouldGlowGold, "Refresh extinguishes the conditional glow");
            AssertEx.False(reforge.ShouldGlowGold, "Reforge has no inactive targets left");
        }
        ArchitectEnchantmentHelper.Add(reforge, ArchitectEnchantKind.Swift, 1);
        reforge.Enchantment!.Status = EnchantmentStatus.Disabled;
        AssertEx.False(reforge.ShouldGlowGold, "Reforge leaves the hand before refreshing other cards");
        return Task.CompletedTask;
    }

    [ArchitectTest]
    public static async Task RequiemAndVerdictRespectOwnersAndEffectRemoval()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        var own = ctx.MockAttackInHand();
        var ally = ctx.MockAttackInHand(owner: ctx.Ally);
        var requiem = await ctx.ApplyPower<RequiemPower>();
        ctx.MarkPlayed(own);
        ctx.MarkPlayed(ally, 2);
        AssertEx.False(own.ShouldGlowGold, "One previous play is below Requiem's threshold");
        AssertEx.False(ally.ShouldGlowGold, "An ally does not inherit this player's Requiem");
        ctx.MarkPlayed(own);
        AssertEx.True(own.ShouldGlowGold, "Requiem marks off-class cards before their third play");
        await PowerCmd.Remove(requiem);
        AssertEx.False(own.ShouldGlowGold, "Removing Requiem clears its glow");

        var verdict = await ctx.ApplyPower<RefreshNextCardEnchantmentPower>();
        AssertEx.False(own.ShouldGlowGold, "Verdict does not mark an unenchanted card");
        ArchitectEnchantmentHelper.Add(own, ArchitectEnchantKind.Sharp, 1);
        AssertEx.False(own.ShouldGlowGold, "Verdict does not mark an active enchantment");
        own.Enchantment!.Status = EnchantmentStatus.Disabled;
        AssertEx.True(own.ShouldGlowGold, "Verdict marks the inactive enchantment");
        ArchitectEnchantmentHelper.Add(ally, ArchitectEnchantKind.Sharp, 1);
        ally.Enchantment!.Status = EnchantmentStatus.Disabled;
        AssertEx.False(ally.ShouldGlowGold, "Verdict does not mark an ally's card");
        await PowerCmd.Remove(verdict);
        AssertEx.False(own.ShouldGlowGold, "Removing Verdict clears its glow");
    }

    [ArchitectTest]
    public static async Task CreationAndTeachingUseRemainingRewards()
    {
        using CombatTestContext ctx = new();
        var creation = await ctx.ApplyPower<FormOfCreationPower>();
        var first = ctx.MockAttackInHand();
        var second = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(first, ArchitectEnchantKind.Sharp, 1);
        ArchitectEnchantmentHelper.Add(second, ArchitectEnchantKind.Sharp, 1);
        AssertEx.True(second.ShouldGlowGold, "Creation has a remaining reward");
        await ctx.PlayFull(first, ctx.Enemy);
        AssertEx.False(second.ShouldGlowGold, "Creation's quota is exhausted");
        ArchitectCombatState.OnTurnStart(ctx.Player);
        AssertEx.True(second.ShouldGlowGold, "New turn restores Creation's quota");
        await PowerCmd.Remove(creation);

        await ctx.ApplyPower<TeachAToFishPower>();
        var attack = ctx.MockAttackInHand();
        var skill = ctx.MockSkillInHand(block: 2);
        AssertEx.True(attack.ShouldGlowGold, "Teaching can enchant the next attack");
        AssertEx.True(skill.ShouldGlowGold, "Teaching can enchant the next skill");
        AssertEx.False(second.ShouldGlowGold, "An already enchanted card cannot receive Teaching's enchantment");
        await ctx.PlayFull(attack, ctx.Enemy);
        AssertEx.False(ctx.MockAttackInHand().ShouldGlowGold, "Attack reward has been consumed");
        AssertEx.True(skill.ShouldGlowGold, "Skill reward remains available");
        await ctx.PlayFull(skill);
        AssertEx.False(ctx.MockSkillInHand().ShouldGlowGold, "Both Teaching rewards have been consumed");
    }

    [ArchitectTest]
    public static async Task ChannelMarkStacksAndDisappearsAfterOnePlay()
    {
        using CombatTestContext ctx = new();
        var channel = ctx.CardInHand<ChannelPower>();
        var target = ctx.MockAttackInHand(damage: 2);
        var other = ctx.MockAttackInHand();
        ctx.Select(target);
        await ctx.Play(channel, xValue: 2);
        ArchitectCombatState.SetPendingReplays(target, 1);
        AssertEx.True(target.ShouldGlowGold, "Channel marks only its target");
        AssertEx.False(other.ShouldGlowGold, "Other card instances have no mark");
        string description = target.GetDescriptionForPile(PileType.Hand, ctx.Enemy);
        AssertEx.True(description.Contains("3"), "Description shows the pending replay count");
        AssertEx.False(description.Contains("THEARCHITECT-CHANNEL_REPLAY"), "Description is localized");
        int hp = ctx.Enemy.CurrentHp;
        await ctx.PlayFull(target, ctx.Enemy);
        AssertEx.Equal(8, hp - ctx.Enemy.CurrentHp, "One play plus three pending replays");
        AssertEx.False(target.ShouldGlowGold, "The one-shot replay mark is consumed");
        AssertEx.Equal(0, ArchitectCombatState.PendingReplays(target), "No replays remain");
        AssertEx.False(target.GetDescriptionForPile(PileType.Hand, ctx.Enemy).Contains("3"), "Pending replay text disappears");
        target.BaseReplayCount = 1;
        ArchitectCombatState.SetPendingReplays(target, 1);
        hp = ctx.Enemy.CurrentHp;
        await ctx.PlayFull(target, ctx.Enemy);
        AssertEx.Equal(6, hp - ctx.Enemy.CurrentHp, "Permanent and pending replays are added once");
        AssertEx.Equal(1, target.BaseReplayCount, "Permanent replay count is preserved");
    }
}
