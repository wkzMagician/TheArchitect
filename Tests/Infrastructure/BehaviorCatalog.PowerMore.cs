using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task RequiemPowerBehavior()
    {
        using CombatTestContext ctx = new();
        RequiemPower power = await ctx.ApplyPower<RequiemPower>(amount: 1m);
        MockAttackCard card = ctx.MockAttackInHand();
        ctx.MarkPlayed(card, 3);
        await PowerCmd.Apply<StrengthPower>(ctx.ChoiceContext, ctx.Enemy, 2m, ctx.Enemy, null);
        CardPlay play = new() { Card = card, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.AfterCardPlayed(ctx.ChoiceContext, play);
        AssertEx.Equal(1, CombatTestContext.PowerAmount<StrengthPower>(ctx.Enemy), "RequiemPower should reduce enemy Strength");
    }

    private static async Task SharedSanctumPowerBehavior()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        SharedSanctumPower power = await ctx.ApplyPower<SharedSanctumPower>();
        MockAttackCard a = ctx.MockAttackInHand();
        MockAttackCard b = ctx.CardInHand<MockAttackCard>(ctx.Ally);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.Equal(1, CombatTestContext.EnchantCount(a), "SharedSanctumPower should enchant local hand");
        AssertEx.Equal(1, CombatTestContext.EnchantCount(b), "SharedSanctumPower should enchant ally hand");
    }

    private static async Task TeachAToFishPowerBehavior()
    {
        using CombatTestContext ctx = new();
        TeachAToFishPower power = await ctx.ApplyPower<TeachAToFishPower>(amount: 3m);
        MockAttackCard attack = ctx.MockAttackInHand();
        CardPlay attackPlay = new() { Card = attack, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.BeforeCardPlayed(attackPlay);
        MockSkillCard skill = ctx.MockSkillInHand(block: 5);
        CardPlay skillPlay = new() { Card = skill, Target = null, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.BeforeCardPlayed(skillPlay);
        AssertEx.True(CombatTestContext.HasEnchant<MegaCrit.Sts2.Core.Models.Enchantments.Sharp>(attack), "TeachAToFishPower should enchant next Attack");
        AssertEx.True(CombatTestContext.HasEnchant<MegaCrit.Sts2.Core.Models.Enchantments.Nimble>(skill), "TeachAToFishPower should enchant next Skill");
        AssertEx.Equal(0, CombatTestContext.PowerAmount<TeachAToFishPower>(ctx.Player.Creature), "TeachAToFishPower should remove itself");
    }

    private static async Task FoundationalCompassBehavior()
    {
        using CombatTestContext ctx = new();
        FoundationalCompass relic = TestModels.MutableRelic<FoundationalCompass>();
        ctx.Player.AddRelicInternal(relic, silent: true);
        MockAttackCard target = ctx.MockAttackInHand();
        await relic.BeforeCombatStart();
        await relic.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(1, CombatTestContext.EnchantCount(target), "FoundationalCompass should enchant one unenchanted starting-hand card");
    }

    private static async Task BlightAnointingPowerBehavior()
    {
        using CombatTestContext ctx = new();
        await ctx.ApplyPower<BlightAnointingPower>();
        MockAttackCard attack = ctx.MockAttackInHand();
        MockSkillCard skill = ctx.MockSkillInHand();
        BlightAnointingPower.TryEnchant(attack);
        BlightAnointingPower.TryEnchant(skill);
        AssertEx.True(CombatTestContext.HasEnchant<MegaCrit.Sts2.Core.Models.Enchantments.Corrupted>(attack), "BlightAnointingPower should corrupt attacks");
        AssertEx.False(CombatTestContext.HasEnchant<MegaCrit.Sts2.Core.Models.Enchantments.Corrupted>(skill), "BlightAnointingPower should ignore skills");
    }
}
