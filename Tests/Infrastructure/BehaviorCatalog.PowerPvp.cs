using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Cards.Uncommon;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    private static async Task TeachAManToFish()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        TeachAManToFish card = ctx.CardInHand<TeachAManToFish>();
        await ctx.Play(card, ctx.Ally!.Creature);
        AssertEx.Equal(3, CombatTestContext.PowerAmount<TeachAToFishPower>(ctx.Ally.Creature), "TeachAManToFish should apply power to ally");
    }

    private static async Task HandOff()
    {
        using CombatTestContext ctx = new(includeAlly: true);
        HandOff card = ctx.CardInHand<HandOff>();
        MockAttackCard shared = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(shared, ArchitectEnchantKind.Sharp, 2m);
        ctx.Select(shared);
        await ctx.Play(card, ctx.Ally!.Creature);
        MegaCrit.Sts2.Core.Models.CardModel? copy = ctx.Ally.PlayerCombatState!.Hand.Cards.OfType<MockAttackCard>().FirstOrDefault();
        AssertEx.NotNull(copy, $"HandOff should give the ally a copy of the chosen card. Ally hand: [{string.Join(", ", ctx.Ally.PlayerCombatState.Hand.Cards.Select(c => c.GetType().Name))}]");
        AssertEx.True(CombatTestContext.HasEnchant<Sharp>(copy!), "HandOff should copy enchantments");
        AssertEx.True(ctx.Player.PlayerCombatState!.ExhaustPile.Cards.Contains(shared), "HandOff should exhaust original");
    }

    private static async Task Doctrine()
    {
        using CombatTestContext ctx = new();
        Doctrine card = ctx.CardInHand<Doctrine>();
        MockAttackCard sharp = ctx.MockAttackInHand();
        MockSkillCard nimble = ctx.MockSkillInHand(block: 5);
        ArchitectEnchantmentHelper.Add(sharp, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(nimble, ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.Equal(1, CombatTestContext.PowerAmount<StrengthPower>(ctx.Player.Creature), "Doctrine should gain Strength from Sharp");
        AssertEx.Equal(1, CombatTestContext.PowerAmount<DexterityPower>(ctx.Player.Creature), "Doctrine should gain Dexterity from Nimble");
    }

    private static async Task SanctumOfVigorPowerBehavior()
    {
        using CombatTestContext ctx = new();
        SanctumOfVigorPower power = await ctx.ApplyPower<SanctumOfVigorPower>(amount: 4m);
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(CombatTestContext.HasEnchant<Vigorous>(target), "SanctumOfVigorPower should add Vigorous");
    }

    private static async Task DivineGracePowerBehavior()
    {
        using CombatTestContext ctx = new();
        DivineGracePower power = await ctx.ApplyPower<DivineGracePower>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.True(CombatTestContext.HasEnchant<PerfectFit>(target), "DivineGracePower should add PerfectFit");
    }

    private static async Task ImmovableAsTheMountainPowerBehavior()
    {
        using CombatTestContext ctx = new();
        ImmovableAsTheMountainPower power = await ctx.ApplyPower<ImmovableAsTheMountainPower>();
        MockAttackCard target = ctx.MockAttackInHand();
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Enemy);
        AssertEx.False(CombatTestContext.HasEnchant<Steady>(target), "ImmovableAsTheMountainPower should not trigger at enemy turn end");
        ctx.Select(target);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.True(CombatTestContext.HasEnchant<Steady>(target), "ImmovableAsTheMountainPower should add Steady");
    }

    private static async Task ResonancePowerBehavior()
    {
        using CombatTestContext ctx = new(includeSecondEnemy: true);
        await ctx.ApplyPower<ResonancePower>(amount: 3m);
        MockAttackCard card = ctx.MockAttackInHand();
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(3, ctx.HpLost(ctx.Enemy, before1), "Resonance should damage first enemy on enchant");
        AssertEx.Equal(3, ctx.HpLost(ctx.SecondEnemy, before2), "Resonance should damage second enemy on enchant");
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Refresh(card);
        await ArchitectEffectQueue.Drain(ctx.Player);
        AssertEx.Equal(3, ctx.HpLost(ctx.Enemy, before1), "Failed enchantment and refresh should not trigger Resonance");
        before1 = ctx.Enemy.CurrentHp;
        await ctx.PlayFull(card, ctx.Enemy);
        AssertEx.Equal(7, ctx.HpLost(ctx.Enemy, before1), "Playing enchanted attack should only deal its own damage including Sharp");
    }

    private static async Task TestSubjectPowerBehavior()
    {
        using CombatTestContext ctx = new();
        TestSubjectPower power = await ctx.ApplyPower<TestSubjectPower>();
        MockAttackCard drawn = ctx.CardInDraw<MockAttackCard>();
        ArchitectEnchantmentHelper.Add(drawn, ArchitectEnchantKind.Sharp, 1m);
        ctx.Select(drawn);
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(0, CombatTestContext.EnchantCount(drawn), "TestSubjectPower should strip enchantments");
    }

    private static async Task RebirthPowerBehavior()
    {
        using CombatTestContext ctx = new();
        RebirthPower power = await ctx.ApplyPower<RebirthPower>();
        MockAttackCard card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Get(card)!.Status = EnchantmentStatus.Disabled;
        CardPlay play = new() { Card = card, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.AfterCardPlayedLate(ctx.ChoiceContext, play);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.Get(card)!.Status, "RebirthPower should refresh played enchantments");
    }

    private static async Task FormOfCreationPowerBehavior()
    {
        using CombatTestContext ctx = new();
        FormOfCreationPower power = await ctx.ApplyPower<FormOfCreationPower>(amount: 2m);
        MockAttackCard card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        MockSkillCard draw = ctx.CardInDraw<MockSkillCard>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        await power.AfterEnergyReset(ctx.Player);
        CardPlay play = new() { Card = card, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.AfterCardPlayed(ctx.ChoiceContext, play);
        AssertEx.Equal(before + 1, ctx.Player.PlayerCombatState.Energy, "FormOfCreationPower should gain energy");
        AssertEx.True(ctx.Player.PlayerCombatState.Hand.Cards.Contains(draw), "FormOfCreationPower should draw");
    }

    private static async Task OmnipotencePowerBehavior()
    {
        using CombatTestContext ctx = new();
        OmnipotencePower power = await ctx.ApplyPower<OmnipotencePower>(amount: 2m);
        MockAttackCard a = ctx.MockAttackInHand();
        MockSkillCard b = ctx.MockSkillInHand();
        await power.AfterEnergyReset(ctx.Player);
        AssertEx.Equal(2, CombatTestContext.EnchantCount(a) + CombatTestContext.EnchantCount(b), "OmnipotencePower should enchant two hand cards");
    }

    private static async Task SanctuaryPowerBehavior()
    {
        using CombatTestContext ctx = new();
        await ctx.ApplyPower<SanctuaryPower>(amount: 2m);
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        await Task.Delay(10);
        AssertEx.Equal(2, ctx.Player.Creature.Block, "SanctuaryPower should grant block on enchant");
    }

    private static async Task DestroyerPowerBehavior()
    {
        using CombatTestContext ctx = new();
        await ctx.ApplyPower<DestroyerPower>(amount: 1m);
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Nimble, 1m);
        ArchitectEnchantmentHelper.Remove(target);
        await Task.Delay(10);
        AssertEx.Equal(1, CombatTestContext.PowerAmount<StrengthPower>(ctx.Player.Creature), "DestroyerPower should gain Strength per affected card");
    }

    private static async Task RecuperatePowerBehavior()
    {
        using CombatTestContext ctx = new();
        RecuperatePower power = await ctx.ApplyPower<RecuperatePower>(amount: 10m);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.Equal(10, ctx.Player.Creature.Block, "RecuperatePower should grant block if no enchanted card was played");
    }

    private static async Task DrowsyEnginePowerBehavior()
    {
        using CombatTestContext ctx = new();
        DrowsyEnginePower power = await ctx.ApplyPower<DrowsyEnginePower>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(before + 2, ctx.Player.PlayerCombatState.Energy, "DrowsyEnginePower should gain 2 energy");
        AssertEx.Equal(1, ctx.CountInDiscard<TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy>(), "DrowsyEnginePower should add Drowsy to the discard pile");
    }
}
