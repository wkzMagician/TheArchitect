using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class ArchitectAncientRelicPatch
{
    public static bool Prefix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is not FoundationalCompass)
        {
            return true;
        }
        __result = ModelDb.Relic<GenesisCompass>();
        return false;
    }
}
