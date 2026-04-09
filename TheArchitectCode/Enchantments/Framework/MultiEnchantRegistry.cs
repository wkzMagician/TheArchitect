using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Enchantments.Framework;

public static class MultiEnchantRegistry
{
    private static readonly HashSet<Type> RegisteredTypes = [];

    public static void Register<T>() where T : CardModel
    {
        RegisteredTypes.Add(typeof(T));
    }

    public static bool SupportsMultiEnchant(CardModel? card)
    {
        if (card == null)
        {
            return false;
        }

        return card is IMultiEnchantCapable
               || RegisteredTypes.Contains(card.GetType())
               || card.Enchantment is MultiEnchantProxy;
    }
}
