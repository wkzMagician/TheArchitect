using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using System.IO;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.Tests.Character;

public static class TheArchitectVisualTests
{
    [ArchitectTest]
    public static async Task MerchantSceneResolvesItsScriptWithoutAnExtraFactoryWrapper()
    {
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Callable.From(() =>
        {
            NMerchantCharacter? merchant = null;
            try
            {
                var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();
                var packed = GD.Load<PackedScene>(character.CustomMerchantAnimPath);
                merchant = NodeFactory<NMerchantCharacter>.CreateFromScene(packed);
                AssertEx.True(merchant is TheArchitect.TheArchitectCode.Visuals.ArchitectMerchantCharacter,
                    "Merchant scene should resolve its C# script instead of being wrapped by BaseLib.");
                AssertEx.Equal("SpineSprite", merchant.GetChild(0).GetClass().ToString(),
                    "Merchant initialization and PlayAnimation require the first child to be a SpineSprite.");
                ((SceneTree)Engine.GetMainLoop()).Root.AddChild(merchant);
                merchant.PlayAnimation("relaxed_loop", loop: true);
                finished.SetResult();
            }
            catch (Exception error)
            {
                finished.SetException(error);
            }
            finally
            {
                merchant?.QueueFree();
            }
        }).CallDeferred();
        await finished.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }

    [ArchitectTest]
    public static void UsesCustomCharacterModelAndArchitectVisualScene()
    {
        var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();

        AssertEx.True(character is CustomCharacterModel, "TheArchitect should derive from CustomCharacterModel so it can provide a custom battle visual.");
        AssertEx.Equal("res://TheArchitect/scenes/creature_visuals/architect_player.tscn", character.CustomVisualPath, "TheArchitect should point to the localized Architect player visual scene.");
    }

    [ArchitectTest]
    public static void BattleVisualIsMirroredForPlayerFacing()
    {
        string scenePath = TestPaths.RepoPath("TheArchitect", "scenes", "creature_visuals", "architect_player.tscn");
        string scene = File.ReadAllText(scenePath);

        AssertEx.True(scene.Contains("scale = Vector2(-0.35, 0.35)", StringComparison.Ordinal), "Architect battle visual should mirror the enemy-facing skeleton so the playable character faces right.");
    }

    [ArchitectTest]
    public static void ReusesStableNonCombatCharacterResources()
    {
        var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();

        AssertEx.Equal("res://TheArchitect/scenes/ui/character_icons/architect_icon.tscn", character.CustomIconPath, "TheArchitect should provide a local in-run icon scene.");
        AssertEx.True(character.CustomIcon?.Name == "ArchitectIcon", "TheArchitect should explicitly instantiate its own in-run icon instead of inheriting another character's icon.");
        AssertEx.Equal("res://scenes/combat/energy_counters/ironclad_energy_counter.tscn", character.CustomEnergyCounterPath, "TheArchitect should use the game energy counter with its own orb artwork.");
        AssertEx.Equal("res://TheArchitect/scenes/merchant/characters/architect_merchant.tscn", character.CustomMerchantAnimPath, "TheArchitect should provide a local merchant scene.");
        AssertEx.Equal("res://TheArchitect/scenes/rest_site/characters/architect_rest_site.tscn", character.CustomRestSiteAnimPath, "TheArchitect should provide a local rest-site scene.");
        AssertEx.Equal("res://scenes/vfx/card_trail_ironclad.tscn", character.CustomTrailPath, "TheArchitect should reuse a base-game card trail scene while custom Godot C# scripts are unavailable.");
        AssertEx.Equal("res://TheArchitect/scenes/screens/char_select/char_select_bg_architect.tscn", character.CustomCharacterSelectBg, "TheArchitect should provide a local character-select background.");
        AssertEx.Equal("res://TheArchitect/materials/transitions/architect_transition_mat.tres", character.CustomCharacterSelectTransitionPath, "TheArchitect should provide a local character-select transition material.");
        AssertEx.Equal("event:/sfx/characters/ironclad/ironclad_select", character.CharacterSelectSfx, "TheArchitect should provide a stable character-select sound.");
        AssertEx.Equal("event:/sfx/ui/wipe_ironclad", character.CharacterTransitionSfx, "TheArchitect should provide a stable transition sound.");
    }

    [ArchitectTest]
    public static void ReusesStableBaseGameArchitectAttackVfx()
    {
        var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();

        string[] expected =
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];

        AssertEx.True(
            expected.SequenceEqual(character.GetArchitectAttackVfx()),
            "TheArchitect should reuse known base-game attack VFX instead of a missing character-specific VFX.");
    }
}
