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
        using BehaviorTestContext ctx = new(includeAlly: true);
        TeachAManToFish card = ctx.CardInHand<TeachAManToFish>();
        await ctx.Play(card, ctx.Ally!.Creature);
        AssertEx.Equal(3, BehaviorTestContext.PowerAmount<TeachAToFishPower>(ctx.Ally.Creature), "TeachAManToFish should apply power to ally");
    }

    private static async Task HandOff()
    {
        using BehaviorTestContext ctx = new(includeAlly: true);
        HandOff card = ctx.CardInHand<HandOff>();
        MockAttackCard shared = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(shared, ArchitectEnchantKind.Sharp, 2m);
        ctx.Select(shared);
        await ctx.Play(card, ctx.Ally!.Creature);
        MegaCrit.Sts2.Core.Models.CardModel copy = ctx.Ally.PlayerCombatState!.Hand.Cards.OfType<MockAttackCard>().Single();
        AssertEx.True(BehaviorTestContext.HasEnchant<Sharp>(copy), "HandOff should copy enchantments");
        AssertEx.True(ctx.Player.PlayerCombatState!.ExhaustPile.Cards.Contains(shared), "HandOff should exhaust original");
    }

    private static async Task Doctrine()
    {
        using BehaviorTestContext ctx = new();
        Doctrine card = ctx.CardInHand<Doctrine>();
        MockAttackCard sharp = ctx.MockAttackInHand();
        MockSkillCard nimble = ctx.MockSkillInHand();
        ArchitectEnchantmentHelper.Add(sharp, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(nimble, ArchitectEnchantKind.Nimble, 1m);
        await ctx.Play(card);
        AssertEx.Equal(1, BehaviorTestContext.PowerAmount<StrengthPower>(ctx.Player.Creature), "Doctrine should gain Strength from Sharp");
        AssertEx.Equal(1, BehaviorTestContext.PowerAmount<DexterityPower>(ctx.Player.Creature), "Doctrine should gain Dexterity from Nimble");
    }

    private static async Task SanctumOfVigorPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        SanctumOfVigorPower power = await ctx.ApplyPower<SanctumOfVigorPower>(amount: 4m);
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.True(BehaviorTestContext.HasEnchant<Vigorous>(target), "SanctumOfVigorPower should add Vigorous");
    }

    private static async Task DivineGracePowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        DivineGracePower power = await ctx.ApplyPower<DivineGracePower>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.True(BehaviorTestContext.HasEnchant<PerfectFit>(target), "DivineGracePower should add PerfectFit");
    }

    private static async Task ImmovableAsTheMountainPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        ImmovableAsTheMountainPower power = await ctx.ApplyPower<ImmovableAsTheMountainPower>();
        MockAttackCard target = ctx.MockAttackInHand();
        ctx.Select(target);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.True(BehaviorTestContext.HasEnchant<Steady>(target), "ImmovableAsTheMountainPower should add Steady");
    }

    private static async Task ResonancePowerBehavior()
    {
        using BehaviorTestContext ctx = new(includeSecondEnemy: true);
        ResonancePower power = await ctx.ApplyPower<ResonancePower>(amount: 3m);
        MockAttackCard card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        CardPlay play = new() { Card = card, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        int before1 = ctx.Enemy.CurrentHp;
        int before2 = ctx.SecondEnemy!.CurrentHp;
        await power.AfterCardPlayed(ctx.ChoiceContext, play);
        AssertEx.Equal(3, ctx.HpLost(ctx.Enemy, before1), "ResonancePower should damage first enemy");
        AssertEx.Equal(3, ctx.HpLost(ctx.SecondEnemy, before2), "ResonancePower should damage second enemy");
    }

    private static async Task TestSubjectPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        TestSubjectPower power = await ctx.ApplyPower<TestSubjectPower>();
        MockAttackCard drawn = ctx.CardInDraw<MockAttackCard>();
        ArchitectEnchantmentHelper.Add(drawn, ArchitectEnchantKind.Sharp, 1m);
        ctx.Select(drawn);
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(0, BehaviorTestContext.EnchantCount(drawn), "TestSubjectPower should strip enchantments");
    }

    private static async Task RebirthPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        RebirthPower power = await ctx.ApplyPower<RebirthPower>();
        MockAttackCard card = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.GetAll(card)[0].Status = EnchantmentStatus.Disabled;
        CardPlay play = new() { Card = card, Target = ctx.Enemy, ResultPile = PileType.Discard, Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = false, PlayIndex = 0, PlayCount = 1 };
        await power.AfterCardPlayed(ctx.ChoiceContext, play);
        AssertEx.Equal(EnchantmentStatus.Normal, ArchitectEnchantmentHelper.GetAll(card)[0].Status, "RebirthPower should refresh played enchantments");
    }

    private static async Task FormOfCreationPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
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
        using BehaviorTestContext ctx = new();
        OmnipotencePower power = await ctx.ApplyPower<OmnipotencePower>(amount: 2m);
        MockAttackCard a = ctx.MockAttackInHand();
        MockSkillCard b = ctx.MockSkillInHand();
        await power.AfterEnergyReset(ctx.Player);
        AssertEx.Equal(2, BehaviorTestContext.EnchantCount(a) + BehaviorTestContext.EnchantCount(b), "OmnipotencePower should enchant two hand cards");
    }

    private static async Task SanctuaryPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        await ctx.ApplyPower<SanctuaryPower>(amount: 2m);
        ArchitectEnchantmentHelper.Add(ctx.MockAttackInHand(), ArchitectEnchantKind.Sharp, 1m);
        await Task.Delay(10);
        AssertEx.Equal(2, ctx.Player.Creature.Block, "SanctuaryPower should grant block on enchant");
    }

    private static async Task DestroyerPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        await ctx.ApplyPower<DestroyerPower>(amount: 1m);
        MockAttackCard target = ctx.MockAttackInHand();
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Sharp, 1m);
        ArchitectEnchantmentHelper.Add(target, ArchitectEnchantKind.Nimble, 1m);
        ArchitectEnchantmentHelper.RemoveAll(target);
        await Task.Delay(10);
        AssertEx.Equal(2, BehaviorTestContext.PowerAmount<StrengthPower>(ctx.Player.Creature), "DestroyerPower should gain Strength per removed enchant");
    }

    private static async Task RecuperatePowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        RecuperatePower power = await ctx.ApplyPower<RecuperatePower>(amount: 10m);
        await power.BeforeTurnEnd(ctx.ChoiceContext, MegaCrit.Sts2.Core.Combat.CombatSide.Player);
        AssertEx.Equal(10, ctx.Player.Creature.Block, "RecuperatePower should grant block if no enchanted card was played");
    }

    private static async Task FateVortexPowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        FateVortexPower power = await ctx.ApplyPower<FateVortexPower>();
        MockAttackCard target = ctx.MockAttackInHand();
        await power.AfterEnergyReset(ctx.Player);
        AssertEx.Equal(1, BehaviorTestContext.EnchantCount(target), "FateVortexPower should enchant one hand card");
    }

    private static async Task DrowsyEnginePowerBehavior()
    {
        using BehaviorTestContext ctx = new();
        DrowsyEnginePower power = await ctx.ApplyPower<DrowsyEnginePower>();
        int before = ctx.Player.PlayerCombatState!.Energy;
        await power.AfterPlayerTurnStart(ctx.ChoiceContext, ctx.Player);
        AssertEx.Equal(before + 1, ctx.Player.PlayerCombatState.Energy, "DrowsyEnginePower should gain energy");
        AssertEx.Equal(1, ctx.CountInHand<TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy>(), "DrowsyEnginePower should add Drowsy");
    }
}
