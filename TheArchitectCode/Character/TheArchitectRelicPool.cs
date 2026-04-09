using BaseLib.Abstracts;
using TheArchitect.TheArchitectCode.Extensions;
using Godot;

namespace TheArchitect.TheArchitectCode.Character;

public class TheArchitectRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheArchitect.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}