using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Potions;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
public static class ArchitectPotionSelectionPatch
{
    private static bool Prefix(PotionModel __instance, PlayerChoiceContext choiceContext, Creature? target, ref Task __result)
    {
        if (__instance is not TheArchitectPotion potion || potion.SelectionPrepared) return true;
        __result = potion.UseWithSelection(choiceContext, target);
        return false;
    }
}
