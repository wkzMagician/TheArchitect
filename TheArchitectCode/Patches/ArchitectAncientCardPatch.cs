using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.TheArchitectCode.Cards.Ancient;
using TheArchitect.TheArchitectCode.Cards.Basic;

namespace TheArchitect.TheArchitectCode.Patches;

// Both ArchaicTooth and DustyTome use this mapping: the tooth transforms the
// starter card, and the tome excludes the mapped Ancient card from its rewards.
[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
public static class ArchitectAncientCardPatch
{
    public static void Postfix(Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<Sigilbreaker>().Id] = ModelDb.Card<AncientVerdict>();
    }
}
