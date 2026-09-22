using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.TheArchitectCode.Audio;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.TheArchitectCode.Character;

public class TheArchitect : CustomCharacterModel
{
    public const string CharacterId = "TheArchitect";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;

    public override CharacterGender Gender => CharacterGender.Neutral;

    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeArchitect>(),
        ModelDb.Card<StrikeArchitect>(),
        ModelDb.Card<StrikeArchitect>(),
        ModelDb.Card<StrikeArchitect>(),
        ModelDb.Card<DefendArchitect>(),
        ModelDb.Card<DefendArchitect>(),
        ModelDb.Card<DefendArchitect>(),
        ModelDb.Card<DefendArchitect>(),
        ModelDb.Card<Tempering>(),
        ModelDb.Card<Sigilbreaker>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<FoundationalCompass>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<TheArchitectCardPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheArchitectRelicPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheArchitectPotionPool>();

    public override string CustomVisualPath => "res://TheArchitect/scenes/creature_visuals/architect_player.tscn";

    // FMOD events from the base game banks (see Audio/ArchitectSfx.cs).
    public override string CharacterTransitionSfx => ArchitectSfx.WipeIronclad;

    public override string CharacterSelectSfx => ArchitectSfx.IroncladSelect;

    public override string CustomIconPath =>
        "res://TheArchitect/scenes/ui/character_icons/architect_icon.tscn";

    public override string CustomEnergyCounterPath =>
        "res://scenes/combat/energy_counters/ironclad_energy_counter.tscn";

    public override string CustomRestSiteAnimPath =>
        "res://TheArchitect/scenes/rest_site/characters/architect_rest_site.tscn";

    public override string CustomMerchantAnimPath =>
        "res://TheArchitect/scenes/merchant/characters/architect_merchant.tscn";

    public override string CustomTrailPath =>
        "res://scenes/vfx/card_trail_ironclad.tscn";

    public override string CustomCharacterSelectBg =>
        "res://TheArchitect/scenes/screens/char_select/char_select_bg_architect.tscn";

    public override string CustomCharacterSelectTransitionPath =>
        "res://TheArchitect/materials/transitions/architect_transition_mat.tres";

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }

    public override string CustomIconTexturePath => "character_icon_architect.png".CharacterUiPath();

    public override string CustomCharacterSelectIconPath => "char_select_architect.png".CharacterUiPath();

    public override string CustomCharacterSelectLockedIconPath => "char_select_architect_locked.png".CharacterUiPath();

    public override string CustomMapMarkerPath => "map_marker_architect.png".CharacterUiPath();

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller)
    {
        AnimState idle = new("idle_loop", isLooping: true);
        AnimState dead = new("hurt");
        AnimState hit = new("hurt");
        AnimState attack = new("attack");
        AnimState cast = new("attack");
        AnimState relaxed = new("idle_loop", isLooping: true);

        dead.NextState = idle;
        hit.NextState = idle;
        attack.NextState = idle;
        cast.NextState = idle;
        relaxed.AddBranch("Idle", idle);

        CreatureAnimator animator = new(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Dead", dead);
        animator.AddAnyState("Hit", hit);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Relaxed", relaxed);
        return animator;
    }
}
