using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace TheArchitect.TheArchitectCode.Helpers;

// Enchantment APIs are synchronous. Keep their triggered effects awaitable at the
// card/potion boundary instead of allowing fire-and-forget failures to escape.
public static class ArchitectEffectQueue
{
    private static readonly ConditionalWeakTable<Player, List<Task>> Pending = new();
    public static void Track(Player player, Task effect) => Pending.GetOrCreateValue(player).Add(effect);
    public static async Task Drain(Player player)
    {
        List<Task> pending = Pending.GetOrCreateValue(player);
        while (pending.Count > 0)
        {
            Task[] batch = pending.ToArray();
            pending.Clear();
            await Task.WhenAll(batch);
        }
    }
}
