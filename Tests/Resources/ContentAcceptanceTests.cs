using System.Reflection;
using System.Text.Json;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Potions;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Resources;

public static class ContentAcceptanceTests
{
    private static IEnumerable<Type> Models<T>() => typeof(TheArchitectCard).Assembly.GetTypes()
        .Where(type => !type.IsAbstract && typeof(T).IsAssignableFrom(type));

    private static T Canonical<T>(string method, Type type) => (T)typeof(ModelDb).GetMethods()
        .Single(info => info.Name == method && info.IsGenericMethodDefinition && info.GetParameters().Length == 0)
        .MakeGenericMethod(type).Invoke(null, null)!;

    private static void Texture(string path, int width, int height)
    {
        AssertEx.True(ResourceLoader.Exists(path), "Packaged resource exists: " + path);
        Texture2D image = ResourceLoader.Load<Texture2D>(path);
        AssertEx.NotNull(image, "Texture loads: " + path);
        AssertEx.Equal(width, image.GetWidth(), "Texture width: " + path);
        AssertEx.Equal(height, image.GetHeight(), "Texture height: " + path);
    }

    [ArchitectTest]
    public static void EveryCardIncludingDrowsyAndChoiceCardsUsesItsOwnPackagedPortraits()
    {
        int count = 0;
        foreach (Type type in Models<CustomCardModel>())
        {
            CustomCardModel card = Canonical<CustomCardModel>("Card", type);
            string id = card.Id.Entry.RemovePrefix().ToLowerInvariant();
            AssertEx.Equal($"TheArchitect/images/card_portraits/{id}.png", card.PortraitPath, "Own small portrait: " + type.Name);
            AssertEx.Equal($"TheArchitect/images/card_portraits/big/{id}.png", card.CustomPortraitPath!, "Own large portrait: " + type.Name);
            Texture(card.PortraitPath, 250, 190);
            Texture(card.CustomPortraitPath!, 1000, 760);
            count++;
        }
        AssertEx.True(count >= 80, "Entire card roster was checked, not only starting cards");
    }

    [ArchitectTest]
    public static void EveryUpgradeableCardRoundTripsItsUpgradeAndPortraitBinding()
    {
        using CombatTestContext ctx = new();
        int checkedCards = 0;
        foreach (Type type in Models<CustomCardModel>())
        {
            var card = (CardModel)Canonical<CustomCardModel>("Card", type).ToMutable();
            card.Owner = ctx.Player;
            if (card.Rarity == MegaCrit.Sts2.Core.Entities.Cards.CardRarity.Token || !card.IsUpgradable) continue;
            card.UpgradeInternal();
            AssertEx.True(card.IsUpgraded, "Upgrade applied: " + type.Name);
            var restored = CardModel.FromSerializable(card.ToSerializable());
            AssertEx.Equal(card.CurrentUpgradeLevel, restored.CurrentUpgradeLevel, "Upgrade survives save: " + type.Name);
            AssertEx.Equal(card.PortraitPath, restored.PortraitPath, "Saved upgrade preserves artwork: " + type.Name);
            checkedCards++;
        }
        AssertEx.True(checkedCards >= 70, "All non-token upgradeable cards were exercised");
    }

    [ArchitectTest]
    public static void AllNineRelicsAndThreePotionsAreRegisteredAndLoadMatchingArt()
    {
        AssertEx.Equal(9, ModelDb.RelicPool<TheArchitectRelicPool>().AllRelics.Count(), "Relic pool includes the ancient compass upgrade");
        AssertEx.Equal(3, ModelDb.PotionPool<TheArchitectPotionPool>().AllPotions.Count(), "Official-sized potion pool");
        foreach (Type type in Models<TheArchitectRelic>())
        {
            var relic = Canonical<TheArchitectRelic>("Relic", type);
            string id = relic is FoundationalCompass ? "foundational_compass_v2" : relic.Id.Entry.RemovePrefix().ToLowerInvariant();
            AssertEx.True(relic.IconPath.Contains(id, StringComparison.Ordinal), "Correct relic icon: " + type.Name);
            AssertEx.Equal(85, relic.Icon.GetWidth(), "Native relic icon size");
            AssertEx.Equal(85, relic.IconOutline.GetWidth(), "Native relic outline size");
            AssertEx.Equal(256, relic.BigIcon.GetWidth(), "Large relic image size");
            AssertEx.True(relic.BigIcon.ResourcePath.Contains(id, StringComparison.Ordinal), "Correct large relic artwork");
        }
        foreach (Type type in Models<TheArchitectPotion>())
        {
            var potion = Canonical<TheArchitectPotion>("Potion", type);
            string id = potion.Id.Entry.RemovePrefix().ToLowerInvariant();
            AssertEx.True(potion.ImagePath.Contains(id, StringComparison.Ordinal), "Potion patch binds correct icon");
            AssertEx.True(potion.OutlinePath!.Contains(id, StringComparison.Ordinal), "Potion patch binds correct outline");
            Texture(potion.ImagePath, 80, 80);
            Texture(potion.OutlinePath!, 80, 80);
            Texture(potion.CustomLargeImagePath!, 256, 256);
        }
    }

    [ArchitectTest]
    public static void EveryPlayableModelHasBothLanguagesAndItemSelectionPrompts()
    {
        foreach (string language in new[] { "eng", "zhs" })
        foreach (var category in new[] { (typeof(CustomCardModel), "Card", "cards"), (typeof(TheArchitectRelic), "Relic", "relics"), (typeof(TheArchitectPotion), "Potion", "potions") })
        {
            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(TestPaths.RepoRoot, "TheArchitect", "localization", language, category.Item3 + ".json")))!;
            foreach (Type type in typeof(TheArchitectCard).Assembly.GetTypes().Where(type => !type.IsAbstract && category.Item1.IsAssignableFrom(type)))
            {
                var model = Canonical<AbstractModel>(category.Item2, type);
                foreach (string field in new[] { "title", "description" })
                    AssertEx.True(entries.TryGetValue(model.Id.Entry + "." + field, out var text) && !string.IsNullOrWhiteSpace(text), $"{language} {model.Id.Entry}.{field}");
                if (model is TheArchitectPotion or FinalizingSeal)
                    AssertEx.True(entries.ContainsKey(model.Id.Entry + ".selectionScreenPrompt"), "Localized item selection prompt");
            }
        }
    }
}
