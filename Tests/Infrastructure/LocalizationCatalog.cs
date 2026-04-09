using System.Text.Json;

namespace TheArchitect.Tests.Infrastructure;

public static class LocalizationCatalog
{
    private static readonly Lazy<IReadOnlyDictionary<string, JsonElement>> Cards = new(() => Load("cards.json"));
    private static readonly Lazy<IReadOnlyDictionary<string, JsonElement>> Powers = new(() => Load("powers.json"));
    private static readonly Lazy<IReadOnlyDictionary<string, JsonElement>> Relics = new(() => Load("relics.json"));

    public static void AssertCardEntry(string key)
    {
        AssertHas(Cards.Value, key);
    }

    public static string CardEntry(string key)
    {
        return Get(Cards.Value, key);
    }

    public static void AssertPowerEntry(string key)
    {
        AssertHas(Powers.Value, key);
    }

    public static void AssertRelicEntry(string key)
    {
        AssertHas(Relics.Value, key);
    }

    public static string RelicEntry(string key)
    {
        return Get(Relics.Value, key);
    }

    private static IReadOnlyDictionary<string, JsonElement> Load(string fileName)
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitect", "localization", "eng"));
        string path = Path.Combine(root, fileName);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);
    }

    private static void AssertHas(IReadOnlyDictionary<string, JsonElement> entries, string key)
    {
        AssertEx.True(entries.ContainsKey(key), $"Missing localization key '{key}'");
        JsonElement element = entries[key];
        AssertEx.Equal(JsonValueKind.String, element.ValueKind, $"Localization key '{key}' must be a string");
        AssertEx.NotEmpty(element.GetString(), $"Localization key '{key}' must not be empty");
    }

    private static string Get(IReadOnlyDictionary<string, JsonElement> entries, string key)
    {
        AssertHas(entries, key);
        return entries[key].GetString()!;
    }
}
