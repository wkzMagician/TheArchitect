using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Enchantments.Framework;

public static class MultiEnchantRegistry
{
    private static readonly HashSet<Type> RegisteredTypes = [];
    private static readonly HashSet<CardModel> RegisteredCards = [];

    public static void Register<T>() where T : CardModel
    {
        RegisteredTypes.Add(typeof(T));
    }

    public static void Register(CardModel card)
    {
        RegisteredCards.Add(card);
    }

    public static bool SupportsMultiEnchant(CardModel? card)
    {
        if (card == null)
        {
            return false;
        }

        return card is IMultiEnchantCapable
               || RegisteredTypes.Contains(card.GetType())
               || RegisteredCards.Contains(card)
               || card.Enchantment is MultiEnchantProxy;
    }
}
