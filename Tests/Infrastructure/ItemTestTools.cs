using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.Tests.Infrastructure;

public static class ItemTestTools
{
    public static async Task<T> Relic<T>(this CombatTestContext ctx, Player? owner = null) where T : RelicModel
    {
        T relic = TestModels.MutableRelic<T>();
        (owner ?? ctx.Player).AddRelicInternal(relic, silent: true);
        await relic.BeforeCombatStart();
        return relic;
    }

    public static T Potion<T>(this CombatTestContext ctx) where T : PotionModel
    {
        T potion = (T)ModelDb.Potion<T>().ToMutable();
        ctx.Player.AddPotionInternal(potion);
        return potion;
    }

    public static async Task PlayFull(this CombatTestContext ctx, CardModel card, Creature? target = null, bool auto = false, int energy = 1)
    {
        if (card.Pile?.Type != PileType.Hand) await CardPileCmd.Add(card, PileType.Hand);
        await card.OnPlayWrapper(ctx.ChoiceContext, target, auto, new ResourceInfo
        {
            EnergySpent = energy, EnergyValue = energy, StarsSpent = 0, StarValue = 0
        }, skipCardPileVisuals: true);
    }
}
