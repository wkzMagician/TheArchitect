using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.Tests.AStartup;

public static class CardLibraryAndUpgradeTests
{
    [ArchitectTest]
    public static void EveryCanonicalArchitectCardRendersItsCompendiumDescription()
    {
        foreach (var card in ModelDb.AllCards.OfType<TheArchitectCard>())
        {
            AssertEx.True(!card.IsMutable, $"{card.Id} must exercise the canonical model");
            AssertEx.True(!string.IsNullOrWhiteSpace(card.GetDescriptionForPile(PileType.None)),
                $"{card.Id} renders without accessing instance-only combat state");
        }
    }

    [ArchitectTest]
    public static async Task ActualLibraryScreenShowsArchitectCardsWhenItsFilterIsSelected()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Callable.From(async () =>
        {
            MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary.NCardLibrary? library = null;
            try
            {
                library = ResourceLoader.Load<PackedScene>("res://scenes/screens/card_library/card_library.tscn")
                    .Instantiate<MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary.NCardLibrary>();
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(library);
                var field = HarmonyLib.AccessTools.Field(library.GetType(), "_cardPoolFilters");
                var filters = (Dictionary<CharacterModel, MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary.NCardPoolFilter>)field.GetValue(library)!;
                var architect = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();
                AssertEx.True(filters.ContainsKey(architect), "Library creates Architect filter");
                library.OnSubmenuOpened();
            await library.ToSignal(library.GetTree().CreateTimer(1.5), SceneTreeTimer.SignalName.Timeout);
            var ironclad = ModelDb.Character<MegaCrit.Sts2.Core.Models.Characters.Ironclad>();
            AssertEx.Equal("res://images/ui/top_panel/character_icon_ironclad.png",
                filters[ironclad].GetNode<TextureRect>("Image").Texture.ResourcePath,
                "Vanilla Ironclad icon must not be redirected by a shared UID");
            AssertEx.True(filters[architect].GetNode<TextureRect>("Image").Texture.ResourcePath.StartsWith("res://TheArchitect/"),
                "Architect filter uses its own artwork");
                AssertEx.True(!filters[architect].IsSelected, "Default library selection is not Architect");
                filters[architect].Call("OnRelease");
            await library.ToSignal(library.GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
                var grid = library.GetNode<MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary.NCardLibraryGrid>("%CardGrid");
            int count = grid.VisibleCards.Count();
            var visibility = HarmonyLib.AccessTools.Method(grid.GetType(), "GetCardVisibility");
            AssertEx.True(grid.VisibleCards.All(card =>
                    (MegaCrit.Sts2.Core.Entities.UI.ModelVisibility)visibility.Invoke(grid, [card])! == MegaCrit.Sts2.Core.Entities.UI.ModelVisibility.Visible),
                "All Architect compendium cards expose their name, description and artwork rather than undiscovered placeholders");
            AssertEx.True(grid.VisibleCards.All(card => card is TheArchitectCard), "Architect filter excludes vanilla cards");
            var rows = (List<List<MegaCrit.Sts2.Core.Nodes.Cards.Holders.NGridCardHolder>>)HarmonyLib.AccessTools.Field(typeof(MegaCrit.Sts2.Core.Nodes.Cards.NCardGrid), "_cardRows").GetValue(grid)!;
            AssertEx.True(rows.SelectMany(r => r).Any(h => h.IsVisibleInTree() && h.Modulate.A > 0.9f && h.CardNode?.Model is TheArchitectCard),
                "Architect cards must render after the filter animation, not just contribute to the count");
                AssertEx.Equal(ModelDb.CardPool<TheArchitectCardPool>().AllCards.Count(), count, "Actual compendium shows the complete Architect pool");
                await library.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                File.WriteAllText(TestPaths.RepoPath(".artifacts", "architect-library-result.txt"), $"Architect library: {count} cards; search empty; no type, rarity or cost filter.\n");
                library.GetViewport().GetTexture().GetImage().SavePng(TestPaths.RepoPath(".artifacts", "architect-library-preview.png"));
                completed.SetResult();
            }
            catch (Exception e) { completed.SetException(e); }
            finally { library?.QueueFree(); }
        }).CallDeferred();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }

    [ArchitectTest]
    public static void UpgradingPreviouslyRenderedCardsUpdatesKeywordsAndDescriptions()
    {
        TestMode.TurnOnInternal();
        var errors = new List<string>();
        foreach (var canonical in ModelDb.AllCards.OfType<TheArchitectCard>())
        {
            var card = (CardModel)canonical.ToMutable();
            if (!card.IsUpgradable || card.Rarity == CardRarity.Token) continue;
            var beforeKeywords = card.Keywords.ToHashSet();
            string beforeDescription = card.GetDescriptionForPile(PileType.None);
            int beforeCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            card.UpgradeInternal();
            var expected = card.CanonicalKeywords.ToHashSet();
            if (!expected.SetEquals(card.Keywords)) errors.Add(card.Id + " cached keywords differ from upgraded keywords");
            if (!expected.SetEquals(beforeKeywords) && beforeDescription == card.GetDescriptionForPile(PileType.None))
                errors.Add(card.Id + " keyword upgrade did not change rendered description");
            if (beforeDescription == card.GetDescriptionForPile(PileType.None) && beforeCost == card.EnergyCost.GetWithModifiers(CostModifiers.All))
                errors.Add(card.Id + " upgrade has no visible description or energy-cost change");
        }
        AssertEx.True(errors.Count == 0, string.Join("; ", errors));
    }

    [ArchitectTest]
    public static void ArchitectCompendiumFilterFindsEveryCharacterCard()
    {
        var pool = ModelDb.CardPool<TheArchitectCardPool>();
        var all = pool.AllCards.ToList();
        var visible = ModelDb.AllCards.Where(card => card.ShouldShowInCardLibrary && pool.AllCardIds.Contains(card.Id)).ToList();
        AssertEx.True(all.Count >= 80, "Architect pool count: " + all.Count);
        AssertEx.Equal(all.Count, visible.Count, "Compendium character filter must show the complete pool");
        var ids = all.Select(card => card.Id).ToHashSet();
        AssertEx.True(ModelDb.AllCards.OfType<TheArchitectCard>().Where(card => card.Rarity != CardRarity.Token)
            .All(card => ids.Contains(card.Id)), "Every main character card belongs to the filter pool");
    }
}
