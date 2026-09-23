using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace TheArchitect.TheArchitectCode.Visuals;

public partial class ArchitectVfx : Node
{
    private Node2D? _parent;
    private MegaSprite? _animController;
    private ArchitectTrail? _innerTrail;
    private ArchitectTrail? _outerTrail;

    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
        if (_parent == null)
        {
            return;
        }

        _animController = new MegaSprite(_parent);
        _animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
        _innerTrail = _parent.GetNode<ArchitectTrail>("TrailSlot/TrailInner");
        _outerTrail = _parent.GetNode<ArchitectTrail>("TrailSlot/TrailOuter");
        _animController.GetAnimationState().SetAnimation("idle_loop");
        _animController.GetAnimationState().SetAnimation("_tracks/head_normal", loop: true, 1);
    }

    private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
    {
        string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
        if (eventName == "trail_start")
        {
            StartTrail();
            return;
        }

        if (eventName == "trail_end")
        {
            EndTrail();
        }
    }

    private void StartTrail()
    {
        if (_innerTrail == null || _outerTrail == null)
        {
            return;
        }

        _innerTrail.Visible = true;
        _outerTrail.Visible = true;
        _innerTrail.ClearPoints();
        _outerTrail.ClearPoints();
    }

    private void EndTrail()
    {
        if (_innerTrail == null || _outerTrail == null)
        {
            return;
        }

        _innerTrail.Visible = false;
        _outerTrail.Visible = false;
    }
}
