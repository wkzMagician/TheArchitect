using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile),
    [typeof(PileType), typeof(Creature)])]
public static class ArchitectDamagePreviewTargetPatch
{
    public static void Prefix(Creature? target, ref Creature? __state)
    {
        __state = ArchitectDamagePreviewTarget.Enter(target);
    }

    public static void Postfix(Creature? __state)
    {
        ArchitectDamagePreviewTarget.Exit(__state);
    }
}
