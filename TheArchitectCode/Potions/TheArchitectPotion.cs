using BaseLib.Abstracts;
using BaseLib.Utils;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.TheArchitectCode.Potions;

[Pool(typeof(TheArchitectPotionPool))]
public abstract class TheArchitectPotion : CustomPotionModel;