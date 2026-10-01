using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace TheArchitect.TheArchitectCode.Patches;

[HarmonyPatch(typeof(SurroundedPower), "FlipScale")]
public static class ArchitectFacingPatch
{
    [HarmonyPostfix]
    public static void Postfix(SurroundedPower __instance, Node2D? body)
    {
        // The boss skeleton faces left at positive X, whereas playable
        // skeletons face right. Surrounded assumes the playable convention.
        // Only correct the owner's body; pets retain their own convention.
        if (body == null || __instance.Owner.Player?.Character is not Character.TheArchitect
            || NCombatRoom.Instance?.GetCreatureNode(__instance.Owner)?.Body != body)
            return;

        body.Scale = ScaleForDirection(body.Scale, __instance.Facing);
    }

    public static Vector2 ScaleForDirection(Vector2 scale, SurroundedPower.Direction direction)
    {
        return new Vector2(
            direction == SurroundedPower.Direction.Right ? -Mathf.Abs(scale.X) : Mathf.Abs(scale.X),
            scale.Y);
    }
}
