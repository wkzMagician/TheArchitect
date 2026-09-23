using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace TheArchitect.TheArchitectCode.Helpers;

/// <summary>
/// Hover tips for Architect-specific rules that are terminology rather than
/// engine CardKeywords. Keeping these separate from CardKeyword prevents the
/// rules from being rendered as automatic card behaviors.
/// </summary>
public static class ArchitectKeywordHoverTips
{
    private const string IconPath = "res://TheArchitect/images/powers/power.png";

    public static IHoverTip Enchant => Create("THEARCHITECT-ENCHANT");

    public static IHoverTip RemoveEnchantments => Create("THEARCHITECT-REMOVE_ENCHANTMENTS");

    public static IHoverTip RefreshEnchantments => Create("THEARCHITECT-REFRESH_ENCHANTMENTS");

    private static IHoverTip Create(string key)
    {
        Texture2D icon = ResourceLoader.Load<Texture2D>(IconPath);
        return new HoverTip(
            new LocString("static_hover_tips", $"{key}.title"),
            new LocString("static_hover_tips", $"{key}.description"),
            icon);
    }
}
