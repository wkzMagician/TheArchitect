using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheArchitect.TheArchitectCode.Extensions;
using Godot;

namespace TheArchitect.TheArchitectCode.Powers;

public abstract class TheArchitectPower : CustomPowerModel
{
    //Loads from TheArchitect/images/powers/your_power.png
    public override string CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}