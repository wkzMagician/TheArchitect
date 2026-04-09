using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task RapidDrafting()
    {
        using BehaviorTestContext ctx = new();
        RapidDrafting card = ctx.CardInHand<RapidDrafting>();
        MockSkillCard a = ctx.CardInDraw<MockSkillCard>();
        MockSkillCard b = ctx.CardInDraw<MockSkillCard>();
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Swift>(a) && BehaviorTestContext.HasEnchant<Swift>(b), "RapidDrafting should enchant drawn cards");
    }

    private static async Task HotStart()
    {
        using BehaviorTestContext ctx = new();
        HotStart card = ctx.CardInHand<HotStart>();
        MockAttackCard a = ctx.MockAttackInHand();
        MockSkillCard b = ctx.MockSkillInHand();
        await ctx.Play(card);
        AssertEx.True(BehaviorTestContext.HasEnchant<Swift>(a) && BehaviorTestContext.HasEnchant<Swift>(b), "HotStart should enchant all cards in hand");
    }

    private static async Task PrimedSpark()
    {
        using BehaviorTestContext ctx = new();
        PrimedSpark card = ctx.CardInHand<PrimedSpark>();
        MockSkillCard drawn = ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        AssertEx.Equal(1, BehaviorTestContext.EnchantCount(card), "PrimedSpark should start enchanted");
        await ctx.Play(card);
        AssertEx.Equal(before + 1, ctx.Player.PlayerCombatState.Energy, "PrimedSpark should gain energy");
        AssertEx.True(ctx.Player.PlayerCombatState.Hand.Cards.Contains(drawn), "PrimedSpark should draw");
    }

    private static async Task MagicCircle()
    {
        using BehaviorTestContext ctx = new();
        MagicCircle card = ctx.CardInHand<MagicCircle>();
        AssertEx.True(BehaviorTestContext.HasEnchant<Swift>(card), "MagicCircle should start with Swift");
        AssertEx.Equal(2, BehaviorTestContext.EnchantAmount(card), "MagicCircle should start with Swift 2");
        await ctx.Play(card);
        AssertEx.Equal(8, ctx.Player.Creature.Block, "MagicCircle should give block");
    }

    private static async Task SeedcoreCannon()
    {
        using BehaviorTestContext ctx = new();
        SeedcoreCannon card = ctx.CardInHand<SeedcoreCannon>();
        AssertEx.True(BehaviorTestContext.HasEnchant<Sown>(card), "SeedcoreCannon should start with Sown");
        AssertEx.Equal(3, BehaviorTestContext.EnchantAmount(card), "SeedcoreCannon should start with Sown 3");
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(32, ctx.HpLost(ctx.Enemy, before), "SeedcoreCannon should deal damage");
    }

    private static async Task OriginalSin()
    {
        using BehaviorTestContext ctx = new();
        OriginalSin card = ctx.CardInHand<OriginalSin>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(14, ctx.HpLost(ctx.Enemy, before), "OriginalSin should deal damage");
    }

    private static async Task Lullaby()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        Lullaby card = ctx.CardInHand<Lullaby>();
        AssertEx.True(BehaviorTestContext.HasEnchant<Glam>(card), "Lullaby should start with Glam");
        await ctx.Play(card);
        AssertEx.Equal(1, BehaviorTestContext.PowerAmount<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(ctx.Enemy), "Lullaby should weaken first enemy");
        AssertEx.Equal(1, BehaviorTestContext.PowerAmount<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(ctx.SecondEnemy!), "Lullaby should weaken second enemy");
    }

    private static async Task GuardedDrowse()
    {
        using BehaviorTestContext ctx = new();
        GuardedDrowse card = ctx.CardInHand<GuardedDrowse>();
        await ctx.Play(card);
        AssertEx.Equal(13, ctx.Player.Creature.Block, "GuardedDrowse should grant block");
        AssertEx.Equal(1, ctx.CountInHand<Drowsy>(), "GuardedDrowse should add Drowsy");
    }

    private static async Task CelestialWar()
    {
        using BehaviorTestContext ctx = new();
        CelestialWar card = ctx.CardInHand<CelestialWar>();
        int before = ctx.Enemy.CurrentHp;
        await ctx.Play(card, ctx.Enemy);
        AssertEx.Equal(16, ctx.HpLost(ctx.Enemy, before), "CelestialWar should deal damage");
        AssertEx.Equal(1, ctx.CountInHand<Drowsy>(), "CelestialWar should add Drowsy");
    }

    private static async Task AppliesPowerCard<TCard, TPower>(int amount)
        where TCard : CardModel
        where TPower : PowerModel
    {
        using BehaviorTestContext ctx = new();
        TCard card = ctx.CardInHand<TCard>();
        await ctx.Play(card);
        AssertEx.Equal(amount, BehaviorTestContext.PowerAmount<TPower>(ctx.Player.Creature), $"{typeof(TCard).Name} should apply {typeof(TPower).Name}");
    }

    private static async Task DrowsyCard()
    {
        using BehaviorTestContext ctx = new();
        Drowsy card = ctx.CardInHand<Drowsy>();
        await MegaCrit.Sts2.Core.Commands.CardCmd.AutoPlay(ctx.ChoiceContext, card, null);
        AssertEx.False(ctx.Player.PlayerCombatState!.Hand.Cards.Contains(card), "Drowsy should leave hand when auto-played");
    }
}
