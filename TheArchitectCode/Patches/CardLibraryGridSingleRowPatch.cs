using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace TheArchitect.TheArchitectCode.Patches;

// NCardGrid's reallocation methods index a second row. The card library can
// briefly have only one row while its filters or layout are changing.
public static class CardLibraryGridSingleRowPatch
{
    private static readonly FieldInfo CardRowsField = AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static bool HasAtLeastTwoRows(NCardGrid grid) =>
        grid is not NCardLibraryGrid ||
        ((List<List<NGridCardHolder>>)CardRowsField.GetValue(grid)!).Count >= 2;

    [HarmonyPatch(typeof(NCardGrid), "ReallocateAbove")]
    private static class ReallocateAbovePatch
    {
        private static bool Prefix(NCardGrid __instance) => HasAtLeastTwoRows(__instance);
    }

    [HarmonyPatch(typeof(NCardGrid), "ReallocateBelow")]
    private static class ReallocateBelowPatch
    {
        private static bool Prefix(NCardGrid __instance) => HasAtLeastTwoRows(__instance);
    }
}
