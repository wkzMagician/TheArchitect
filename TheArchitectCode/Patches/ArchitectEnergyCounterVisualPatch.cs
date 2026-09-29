using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using ArchitectCharacter = TheArchitect.TheArchitectCode.Character.TheArchitect;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter.Create))]
public static class ArchitectEnergyCounterVisualPatch
{
    public static void Postfix(Player player, NEnergyCounter? __result)
    {
        if (player.Character is ArchitectCharacter && __result is not null)
            ApplyArchitectVisuals(__result);
    }

    public static void ApplyArchitectVisuals(NEnergyCounter counter)
    {
        const string imageRoot = "res://TheArchitect/images/ui/combat/energy_counters/architect/";
        const string materialPath = "res://TheArchitect/materials/ui/architect_energy_orb_dark.tres";
        Material? material = ResourceLoader.Exists(materialPath) ? GD.Load<Material>(materialPath) : null;
        string[] paths =
        [
            "Layers/Layer1",
            "Layers/RotationLayers/Layer2",
            "Layers/RotationLayers/Layer3",
            "Layers/Layer4",
            "Layers/Layer5"
        ];

        for (int i = 0; i < paths.Length; i++)
        {
            TextureRect layer = counter.GetNode<TextureRect>(paths[i]);
            string imagePath = $"{imageRoot}architect_orb_layer_{i + 1}.png";
            if (ResourceLoader.Exists(imagePath))
                layer.Texture = GD.Load<Texture2D>(imagePath);
            if (material is not null)
                layer.Material = material;
        }
    }
}
