using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Patches;

// Preserve the engine's card/enchantment glow and its hand UI playability checks.
// Power effects must also apply to generated cards and cards from other pools.
[HarmonyPatch(typeof(CardModel), nameof(CardModel.ShouldGlowGold), MethodType.Getter)]
public static class ArchitectCardHighlightPatch
{
    private static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result || !__instance.IsMutable || __instance.CombatState == null || __instance.Owner == null) return;
        var creature = __instance.Owner.Creature;
        __result = ArchitectCombatState.PendingReplays(__instance) > 0 ||
            (creature.GetPower<RequiemPower>() != null && ArchitectCombatState.TimesPlayed(__instance) >= 2) ||
            (creature.GetPower<RefreshNextCardEnchantmentPower>() != null && ArchitectEnchantmentHelper.HasInactive(__instance)) ||
            (creature.GetPower<FormOfCreationPower>()?.CanTriggerFor(__instance) ?? false) ||
            (creature.GetPower<TeachAToFishPower>()?.CanTriggerFor(__instance) ?? false);
    }
}
