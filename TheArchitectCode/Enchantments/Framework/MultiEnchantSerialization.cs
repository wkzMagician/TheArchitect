using System.Text.Json;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace TheArchitect.TheArchitectCode.Enchantments.Framework;

public static class MultiEnchantSerialization
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string SerializeChildren(IEnumerable<EnchantmentModel> children)
    {
        List<SerializableEnchantment> payload = children.Select(ToSerializable).ToList();
        return JsonSerializer.Serialize(payload, Options);
    }

    public static IReadOnlyList<SerializableEnchantment> DeserializeChildren(string? serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<SerializableEnchantment>>(serialized, Options) ?? [];
    }

    public static SerializableEnchantment ToSerializable(EnchantmentModel enchantment)
    {
        return enchantment.ToSerializable();
    }

    public static EnchantmentModel ToMutableChild(SerializableEnchantment child)
    {
        return EnchantmentModel.FromSerializable(child);
    }
}
