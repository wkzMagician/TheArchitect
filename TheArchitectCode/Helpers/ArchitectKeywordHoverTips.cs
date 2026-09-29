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
    public static IHoverTip Enchant => Create("THEARCHITECT-ENCHANT", "enchant.png");

    public static IHoverTip RemoveEnchantments => Create("THEARCHITECT-REMOVE_ENCHANTMENTS", "remove.png");

    public static IHoverTip RefreshEnchantments => Create("THEARCHITECT-REFRESH_ENCHANTMENTS", "refresh.png");

    public static IEnumerable<IHoverTip> IncludeEnchant(IEnumerable<IHoverTip> hoverTips)
    {
        foreach (IHoverTip hoverTip in hoverTips)
        {
            yield return hoverTip;
        }

        yield return Enchant;
    }

    private static IHoverTip Create(string key, string iconName)
    {
        Texture2D icon = ResourceLoader.Load<Texture2D>($"res://TheArchitect/images/keywords/{iconName}");
        return new HoverTip(
            new LocString("static_hover_tips", $"{key}.title"),
            new LocString("static_hover_tips", $"{key}.description"),
            icon);
    }
}
