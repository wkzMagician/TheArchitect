using System.Reflection;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace TheArchitect.TheArchitectCode.Visuals;

public partial class ArchitectParticlesContainer : NParticlesContainer
{
    private static readonly FieldInfo BaseParticlesField =
        typeof(NParticlesContainer).GetField("_particles", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Export]
    private Array<GpuParticles2D> _architectParticles = [];

    public override void _Ready()
    {
        BaseParticlesField.SetValue(this, _architectParticles);
    }
}
