using Godot;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Patches;

namespace TheArchitect.Tests.Character;

public static class ArchitectFacingTests
{
    [ArchitectTest]
    public static void SurroundedFacingFollowsTargetWithoutTogglingOnRepeatedAttacks()
    {
        Vector2 scale = new(-0.35f, 0.35f);
        foreach (var direction in new[]
                 {
                     SurroundedPower.Direction.Left, SurroundedPower.Direction.Left,
                     SurroundedPower.Direction.Right, SurroundedPower.Direction.Right,
                     SurroundedPower.Direction.Left, SurroundedPower.Direction.Right
                 })
        {
            // Simulate the base game's normalization before the postfix runs.
            scale.X = direction == SurroundedPower.Direction.Right ? Mathf.Abs(scale.X) : -Mathf.Abs(scale.X);
            scale = ArchitectFacingPatch.ScaleForDirection(scale, direction);
            AssertEx.Equal(direction == SurroundedPower.Direction.Right ? -0.35f : 0.35f,
                scale.X, "Architect should visually face the selected side on every attack");
            AssertEx.Equal(0.35f, scale.Y, "Turning should preserve the visual's vertical scale");
        }
    }
}
