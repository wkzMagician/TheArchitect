using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Cards.Rare;

public static class FinalJudgmentOfTheRadiantScepterTests
{
    [ArchitectTest]
    public static async Task TenHitComboStartsOnFifthPlay()
    {
        foreach (int priorPlays in new[] { 3, 4, 5 })
        {
            using CombatTestContext ctx = new();
            var card = ctx.CardInHand<FinalJudgmentOfTheRadiantScepter>();
            ctx.MarkPlayed(card, priorPlays);
            int hits = priorPlays < 4 ? 1 : 10;
            string preview = (string)typeof(FinalJudgmentOfTheRadiantScepter)
                .GetMethod("GetCombatPreviewText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(card, null)!;
            AssertEx.Equal(hits.ToString(), System.Text.RegularExpressions.Regex.Match(preview, @"\d+").Value,
                "Combo preview uses the fifth-play threshold");
            int before = ctx.Enemy.CurrentHp;
            await ctx.Play(card, ctx.Enemy);
            AssertEx.Equal(hits * 10, ctx.HpLost(ctx.Enemy, before),
                "Fourth play hits once; fifth and subsequent plays hit ten times");
        }
    }

    [ArchitectTest]
    public static Task CombatScenario()
    {
        return BehaviorCatalog.AssertCardBehavior<FinalJudgmentOfTheRadiantScepter>();
    }
}
