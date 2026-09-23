using Godot;

namespace TheArchitect.TheArchitectCode.Visuals;

public partial class ArchitectTrail : Line2D
{
    private Node2D? _target;

    [Export]
    private int _maxSegments = 10;

    public override void _Ready()
    {
        _target = GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        if (_target == null)
        {
            return;
        }

        GlobalPosition = Vector2.Zero;
        GlobalRotation = 0.0f;
        GlobalScale = Vector2.One;
        AddPoint(_target.GlobalPosition);
        if (Points.Length > _maxSegments)
        {
            RemovePoint(0);
        }
    }
}
