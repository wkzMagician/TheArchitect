using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.TheArchitectCode.Enchantments.Framework;

public static class MultiEnchantHelper
{
    public static EnchantmentModel? TryAddEnchantment(CardModel card, EnchantmentModel enchantment, decimal amount)
    {
        if (!MultiEnchantRegistry.SupportsMultiEnchant(card))
        {
            if (card.Enchantment != null)
            {
                return null;
            }

            return CardCmd.Enchant(enchantment, card, amount);
        }

        if (card.Enchantment == null)
        {
            return CardCmd.Enchant(enchantment, card, amount);
        }

        if (card.Enchantment is MultiEnchantProxy proxy)
        {
            return TryAppend(proxy, enchantment, amount);
        }

        if (card.Enchantment.GetType() == enchantment.GetType() && !card.Enchantment.IsStackable)
        {
            return null;
        }

        return UpgradeToProxy(card, enchantment, amount);
    }

    public static IReadOnlyList<EnchantmentModel> GetEnchantments(CardModel card)
    {
        if (card.Enchantment is MultiEnchantProxy proxy)
        {
            return proxy.GetChildren();
        }

        return card.Enchantment == null ? [] : [card.Enchantment];
    }

    public static bool HasAnyEnchantments(CardModel card)
    {
        return GetEnchantments(card).Count > 0;
    }

    public static int RemoveAllEnchantments(CardModel card)
    {
        int removed = GetEnchantments(card).Count;
        if (removed == 0)
        {
            return 0;
        }

        card.ClearEnchantmentInternal();
        card.FinalizeUpgradeInternal();
        return removed;
    }

    private static EnchantmentModel? UpgradeToProxy(CardModel card, EnchantmentModel newEnchantment, decimal amount)
    {
        if (card.Enchantment == null)
        {
            return CardCmd.Enchant(newEnchantment, card, amount);
        }

        EnchantmentModel existing = card.Enchantment;
        card.ClearEnchantmentInternal();

        MultiEnchantProxy proxy = (MultiEnchantProxy)ModelDb.Enchantment<MultiEnchantProxy>().ToMutable();
        card.EnchantInternal(proxy, 0m);
        proxy.AddChild(existing, existing.Amount);

        EnchantmentModel? appended = TryAppend(proxy, newEnchantment, amount);
        card.FinalizeUpgradeInternal();
        return appended == null ? null : proxy;
    }

    private static EnchantmentModel? TryAppend(MultiEnchantProxy proxy, EnchantmentModel enchantment, decimal amount)
    {
        EnchantmentModel? matchingType = proxy.GetChildOfType(enchantment.GetType());
        if (matchingType != null)
        {
            if (!matchingType.IsStackable)
            {
                return null;
            }

            matchingType.Amount += (int)amount;
            proxy.SerializedChildren = MultiEnchantSerialization.SerializeChildren(proxy.GetChildren());
            proxy.RecalculateValues();
            proxy.Card.DynamicVars.RecalculateForUpgradeOrEnchant();
            proxy.Card.FinalizeUpgradeInternal();
            return proxy;
        }

        proxy.AddChild(enchantment, amount);
        proxy.Card.FinalizeUpgradeInternal();
        return proxy;
    }
}
