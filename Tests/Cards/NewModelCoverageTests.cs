using VulnerablePower = MegaCrit.Sts2.Core.Models.Powers.VulnerablePower;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Common;
using TheArchitect.TheArchitectCode.Cards.Rare;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Cards;

public static class AutomatonTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<Automaton>(), "Automaton model should be registered");
}

public static class ExplosiveCoreTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<ExplosiveCore>(), "Explosive Core model should be registered");
}

public static class ExplosiveCorePowerTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(MegaCrit.Sts2.Core.Models.ModelDb.Power<ExplosiveCorePower>(), "Explosive Core power should be registered");

    [ArchitectTest]
    public static async Task DamageIsNotAmplifiedByVulnerable()
    {
        using CombatTestContext ctx = new();
        ExplosiveCorePower power = await ctx.ApplyPower<ExplosiveCorePower>(amount: 3m);
        var card = ctx.MockAttackInHand();
        await ctx.ApplyPower<VulnerablePower>(ctx.Enemy);

        int before = ctx.Enemy.CurrentHp;
        await power.OnEnchantmentsRemoved(card, 1);

        AssertEx.Equal(3, ctx.HpLost(ctx.Enemy, before), "Explosive Core should deal unpowered damage");
    }
}

public static class FallenTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<Fallen>(), "Fallen model should be registered");
}

public static class GenesisPowerTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(MegaCrit.Sts2.Core.Models.ModelDb.Power<GenesisPower>(), "Genesis power should be registered");
}

public static class RefreshNextCardEnchantmentPowerTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(MegaCrit.Sts2.Core.Models.ModelDb.Power<RefreshNextCardEnchantmentPower>(), "Refresh power should be registered");
}

public static class SketchSlashTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<SketchSlash>(), "Sketch Slash model should be registered");
}

public static class SoulTotemTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<SoulTotem>(), "Soul Totem model should be registered");
}

public static class UnyieldingFormTests
{
    [ArchitectTest]
    public static void ModelIsRegistered() => AssertEx.NotNull(TestModels.Card<UnyieldingForm>(), "Unyielding Form model should be registered");
}
