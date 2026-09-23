namespace TheArchitect.TheArchitectCode.Audio;

/// <summary>
/// Central mapping of FMOD event paths used by TheArchitect.
///
/// The base game ships character and UI sounds as FMOD events inside
/// banks/desktop/sfx.bank + Master.strings.bank; there are no loose audio
/// files to copy. Reuse those events directly here, or point a path at the
/// Architect's own enemy events (banks/desktop/act1_b1.bank,
/// "debuffenemy/enemy_attacks/architect/architect_ending_attack").
///
/// To swap a sound later: change the value here (or use
/// BaseLib.Utils.FmodAudio.RegisterEventReplacement) — no other file involved.
/// </summary>
public static class ArchitectSfx
{
    /// <summary>Architect's own attack event (enemy boss, act1_b1.bank).</summary>
    public const string ArchitectAttack = "event:/sfx/enemy/debuffenemy/enemy_attacks/architect/architect_ending_attack";

    /// <summary>Ironclad cast, reused until Architect-specific audio is authored.</summary>
    public const string IroncladCast = "event:/sfx/characters/ironclad/ironclad_cast";

    /// <summary>Ironclad death, reused until Architect-specific audio is authored.</summary>
    public const string IroncladDie = "event:/sfx/characters/ironclad/ironclad_die";

    /// <summary>Ironclad character select, reused until Architect-specific audio is authored.</summary>
    public const string IroncladSelect = "event:/sfx/characters/ironclad/ironclad_select";

    /// <summary>Character select screen wipe transition.</summary>
    public const string WipeIronclad = "event:/sfx/ui/wipe_ironclad";
}
