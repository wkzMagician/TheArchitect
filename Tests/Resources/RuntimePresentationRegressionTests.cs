using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Cards.Basic;

namespace TheArchitect.Tests.Resources;

public static class RuntimePresentationRegressionTests
{
    [ArchitectTest]
    public static async Task CharacterSelectUsesArchitectArtworkAndRenders()
    {
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Callable.From(async () =>
        {
            SubViewport? viewport = null;
            try
            {
                var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();
                var scene = ResourceLoader.Load<PackedScene>(character.CustomCharacterSelectBg).Instantiate<Control>();
                var portrait = scene.GetNode<TextureRect>("Portrait");
                AssertEx.Equal("res://TheArchitect/images/character_select/architect_workshop_v1.png", portrait.Texture.ResourcePath,
                    "Selection screen uses the original Architect illustration, not the renamed Ironclad atlas");
                AssertEx.True(!scene.HasNode("SpineSprite"), "No warrior skeleton remains in selection scene");
                viewport = new SubViewport { Size = new Vector2I(1920, 1080), RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(viewport);
                // Match the actual character-selection screen's overscan/parallax container.
                var container = new Control { Size = new Vector2(2560, 1200), Position = new Vector2(-388, -80),
                    PivotOffset = new Vector2(1280, 600), Scale = new Vector2(1.1f, 1.1f) };
                viewport.AddChild(container);
                container.AddChild(scene);
                await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                string screenshot = TestPaths.RepoPath(".artifacts", "architect-character-select-preview.png");
                AssertEx.Equal(Error.Ok, viewport.GetTexture().GetImage().SavePng(screenshot), "Selection preview renders");
                AssertEx.True(portrait.Size.X > 0 && portrait.Size.Y > 0, "Portrait fills the selection container");
                finished.SetResult();
            }
            catch (Exception error) { finished.SetException(error); }
            finally { viewport?.QueueFree(); }
        }).CallDeferred();
        await finished.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }

    [ArchitectTest]
    public static async Task BattleSceneInitializesSkeletonAndCombatAnimations()
    {
        // Native Spine loading must run on Godot's main thread, not the test continuation thread.
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Callable.From(() =>
        {
            Node? scene = null;
            string stage = "loading Spine atlas pages";
            try
            {
                EverySpineAtlasPageExistsAndLoadsFromThePack();
                stage = "loading the battle scene";
                var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();
                PackedScene? packedScene = ResourceLoader.Load<PackedScene>(character.CustomVisualPath);
                AssertEx.NotNull(packedScene, $"Battle scene loads: {character.CustomVisualPath}");
                stage = "instantiating the battle scene";
                scene = packedScene!.Instantiate();
                stage = "adding the battle scene to the tree";
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(scene);
                stage = "finding the SpineSprite node";
                Node visuals = scene.GetNode("Visuals");
                AssertEx.Equal("SpineSprite", visuals.GetClass(), "Battle visual must instantiate as a SpineSprite");
                stage = "binding the SpineSprite node";
                var sprite = new MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite(visuals);
                stage = "initializing the Spine skeleton";
                AssertEx.NotNull(sprite.GetSkeleton(), "Battle skeleton initializes with its packaged atlas");
                stage = "initializing the Spine animation state";
                AssertEx.True(sprite.IsAnimationStateReady(), "Battle animation state is ready");
                stage = "configuring combat animations";
                var animator = character.SetupCustomAnimationStates(sprite);
                foreach (string trigger in new[] { "Idle", "Attack", "Hit", "Cast", "Dead" })
                    animator.SetTrigger(trigger);
                finished.SetResult();
            }
            catch (Exception error) { finished.SetException(new InvalidOperationException($"Battle scene failed while {stage}: {error.Message}", error)); }
            finally { scene?.QueueFree(); }
        }).CallDeferred();
        await finished.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }

    [ArchitectTest]
    public static void EveryOwnedCardRendersInTheDeckOutsideCombat()
    {
        TestMode.TurnOnInternal();
        var player = Player.CreateForNewRun(ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>(), UnlockState.all, 1);
        var run = RunState.CreateForTest([player]);
        AssertEx.True(player.PlayerCombatState == null, "Reproduce deck view with an owner but no combat hand");
        foreach (Type type in typeof(TheArchitectCard).Assembly.GetTypes()
                     .Where(type => !type.IsAbstract && typeof(CustomCardModel).IsAssignableFrom(type)))
        {
            var canonical = (CardModel)typeof(ModelDb).GetMethods()
                .Single(method => method.Name == "Card" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                .MakeGenericMethod(type).Invoke(null, null)!;
            var card = run.CreateCard(canonical, player);
            AssertEx.NotEmpty(card.GetDescriptionForPile(PileType.Deck), "Owned deck description: " + type.Name);
            if (card.IsUpgradable)
            {
                card.UpgradeInternal();
                AssertEx.NotEmpty(card.GetDescriptionForPile(PileType.Deck), "Upgraded deck description: " + type.Name);
            }
        }
        var sigilbreaker = run.CreateCard<Sigilbreaker>(player);
        AssertEx.True(sigilbreaker.GetDescriptionForPile(PileType.Deck).Contains("8"), "Sigilbreaker displays base damage without a hand");
    }

    [ArchitectTest]
    public static void EverySpineAtlasPageExistsAndLoadsFromThePack()
    {
        string root = TestPaths.RepoPath("TheArchitect", "animations");
        foreach (string atlas in Directory.EnumerateFiles(root, "*.atlas", SearchOption.AllDirectories))
        {
            // Spine page names follow an empty line (or start of file), unlike region names.
            string[] lines = File.ReadAllLines(atlas);
            for (int i = 0; i < lines.Length; i++)
            {
                string page = lines[i].Trim();
                if (page.Length == 0 || (i > 0 && !string.IsNullOrWhiteSpace(lines[i - 1]))) continue;
                string file = Path.Combine(Path.GetDirectoryName(atlas)!, page);
                AssertEx.True(File.Exists(file), "Spine atlas page must exist before native loading: " + file);
                string path = "res://TheArchitect/animations/" + Path.GetRelativePath(root, file).Replace('\\', '/');
                var texture = ResourceLoader.Load<Texture2D>(path);
                AssertEx.NotNull(texture, "Packaged Spine page: " + path);
                AssertEx.True(texture.GetWidth() > 0 && texture.GetHeight() > 0, "Spine page has pixels: " + path);
            }
        }
    }
}
