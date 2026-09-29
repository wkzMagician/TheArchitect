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
            var iconName = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            var illustratedPath = iconName.Replace(".png", "_illustrated.png").PowerImagePath();
            if (ResourceLoader.Exists(illustratedPath))
            {
                return illustratedPath;
            }

            var path = iconName.PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var iconName = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";
            var illustratedPath = iconName.Replace(".png", "_illustrated.png").PowerImagePath();
            if (ResourceLoader.Exists(illustratedPath))
            {
                return illustratedPath;
            }

            var path = iconName.BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}
