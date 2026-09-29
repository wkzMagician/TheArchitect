using System.Threading;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace TheArchitect.TheArchitectCode.Helpers;

// The engine passes the hovered creature to description formatting, while card
// description arguments have no target parameter of their own.
internal static class ArchitectDamagePreviewTarget
{
    private static readonly AsyncLocal<Creature?> Current = new();

    public static Creature? Target => Current.Value;

    public static Creature? Enter(Creature? target)
    {
        Creature? previous = Current.Value;
        Current.Value = target;
        return previous;
    }

    public static void Exit(Creature? previous) => Current.Value = previous;
}
