using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheArchitect.Tests.Infrastructure;

public static class ModelTestHelper
{
    private static string LocalizationBase(string entry)
    {
        return entry.Contains('-', StringComparison.Ordinal) ? entry : $"THEARCHITECT-{entry}";
    }

    public static void AssertCardMetadata<T>(CardType type, CardRarity rarity, TargetType targetType) where T : CardModel, new()
    {
        T card = new();
        AssertEx.NotNull(card.Id, $"{typeof(T).Name} should expose an id");
        AssertEx.NotEmpty(card.Id.Entry, $"{typeof(T).Name} should expose a non-empty id");
        AssertEx.Equal(type, card.Type, $"{typeof(T).Name} card type mismatch");
        AssertEx.Equal(rarity, card.Rarity, $"{typeof(T).Name} card rarity mismatch");
        AssertEx.Equal(targetType, card.TargetType, $"{typeof(T).Name} target type mismatch");
        string keyBase = LocalizationBase(card.Id.Entry);
        LocalizationCatalog.AssertCardEntry($"{keyBase}.title");
        LocalizationCatalog.AssertCardEntry($"{keyBase}.description");
    }

    public static void AssertPowerMetadata<T>(PowerType type, PowerStackType stackType) where T : PowerModel, new()
    {
        T power = new();
        AssertEx.NotNull(power.Id, $"{typeof(T).Name} should expose an id");
        AssertEx.NotEmpty(power.Id.Entry, $"{typeof(T).Name} should expose a non-empty id");
        AssertEx.Equal(type, power.Type, $"{typeof(T).Name} power type mismatch");
        AssertEx.Equal(stackType, power.StackType, $"{typeof(T).Name} power stack type mismatch");
        string keyBase = LocalizationBase(power.Id.Entry);
        LocalizationCatalog.AssertPowerEntry($"{keyBase}.title");
        LocalizationCatalog.AssertPowerEntry($"{keyBase}.description");
        LocalizationCatalog.AssertPowerEntry($"{keyBase}.smartDescription");
    }

    public static void AssertRelicMetadata<T>(RelicRarity rarity) where T : RelicModel, new()
    {
        T relic = new();
        AssertEx.NotNull(relic.Id, $"{typeof(T).Name} should expose an id");
        AssertEx.NotEmpty(relic.Id.Entry, $"{typeof(T).Name} should expose a non-empty id");
        AssertEx.Equal(rarity, relic.Rarity, $"{typeof(T).Name} relic rarity mismatch");
        string keyBase = LocalizationBase(relic.Id.Entry);
        LocalizationCatalog.AssertRelicEntry($"{keyBase}.title");
        LocalizationCatalog.AssertRelicEntry($"{keyBase}.description");
        LocalizationCatalog.AssertRelicEntry($"{keyBase}.flavor");
    }
}
