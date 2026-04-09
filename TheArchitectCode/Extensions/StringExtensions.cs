namespace TheArchitect.TheArchitectCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    private static string ModPath(params string[] parts)
    {
        return string.Join("/", parts);
    }

    public static string ImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "card_portraits", path);
    }

    public static string BigCardImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "card_portraits", "big", path);
    }

    public static string PowerImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "powers", path);
    }

    public static string BigPowerImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "powers", "big", path);
    }

    public static string RelicImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "relics", path);
    }

    public static string BigRelicImagePath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "relics", "big", path);
    }

    public static string CharacterUiPath(this string path)
    {
        return ModPath(MainFile.ModId, "images", "charui", path);
    }
}
