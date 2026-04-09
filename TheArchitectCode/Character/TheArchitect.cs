using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.TheArchitectCode.Cards.Basic;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.TheArchitectCode.Character;

public class TheArchitect : PlaceholderCharacterModel
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

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();

    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();

    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();

    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
