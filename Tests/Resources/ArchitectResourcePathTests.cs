using MegaCrit.Sts2.Core.Models;
using System.Text.RegularExpressions;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Extensions;

namespace TheArchitect.Tests.Resources;

public static class ArchitectResourcePathTests
{
    private static readonly string[] ForbiddenBaseGameCharacterFragments =
    [
        "ironclad",
        "silent",
        "defect",
        "regent",
        "necrobinder"
    ];

    private static readonly string[] ForbiddenBaseGameRuntimePaths =
    [
        "res://scenes/ui/character_icons/ironclad_icon.tscn",
        "res://scenes/merchant/characters/ironclad_merchant.tscn",
        "res://scenes/rest_site/characters/ironclad_rest_site.tscn",
        "res://scenes/screens/char_select/char_select_bg_ironclad.tscn",
        "res://materials/transitions/ironclad_transition_mat.tres",
        "res://animations/characters/ironclad/ironclad.atlas",
        "res://animations/characters/ironclad/ironclad.skel"
    ];

    private static IReadOnlyList<string> GetLocalizationIds(string relativeJsonPath)
    {
        string repoRoot = TestPaths.RepoRoot;
        string jsonPath = Path.Combine(repoRoot, relativeJsonPath);
        string content = File.ReadAllText(jsonPath);
        MatchCollection matches = Regex.Matches(content, "\"THEARCHITECT-([A-Z0-9_]+)\\.title\"");
        return matches.Select(m => m.Groups[1].Value.ToLowerInvariant()).Distinct().OrderBy(x => x).ToArray();
    }

    private static (int Width, int Height) ReadPngDimensions(string file)
    {
        byte[] header = new byte[24];
        using FileStream stream = File.OpenRead(file);
        int bytesRead = stream.Read(header, 0, header.Length);

        AssertEx.True(bytesRead == header.Length, $"PNG header should be readable for {Path.GetFileName(file)}");
        AssertEx.True(header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47, $"{Path.GetFileName(file)} should be a PNG");

        int width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
        int height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];

        return (width, height);
    }

    private static IReadOnlyList<string> SplitCsvLine(string line)
    {
        List<string> columns = [];
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                columns.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        columns.Add(current.ToString());
        return columns;
    }

    private static IReadOnlyList<Dictionary<string, string>> ReadCsv(string file)
    {
        string[] lines = File.ReadAllLines(file);
        string[] headers = SplitCsvLine(lines[0]).ToArray();

        return lines
            .Skip(1)
            .Select(line =>
            {
                IReadOnlyList<string> values = SplitCsvLine(line);
                return headers
                    .Select((header, index) => (header, Value: index < values.Count ? values[index] : string.Empty))
                    .ToDictionary(item => item.header, item => item.Value);
            })
            .ToArray();
    }

    [ArchitectTest]
    public static void CharacterRuntimeResourcePathsShouldBeLocalToTheMod()
    {
        var character = ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>();

        string[] localPaths =
        [
            character.CustomVisualPath,
            character.CustomIconPath ?? string.Empty,
            character.CustomMerchantAnimPath,
            character.CustomRestSiteAnimPath,
            character.CustomCharacterSelectBg,
            character.CustomCharacterSelectTransitionPath,
            character.CustomIconTexturePath,
            character.CustomCharacterSelectIconPath,
            character.CustomCharacterSelectLockedIconPath,
            character.CustomMapMarkerPath
        ];

        foreach (string path in localPaths)
        {
            ResourcePathAssertions.IsLocalModResource(path, "Architect runtime resource path should be local to the mod");
            ResourcePathAssertions.DoesNotContainAny(path, ForbiddenBaseGameCharacterFragments, "Architect runtime resource path should not use temporary base-game character assets");
        }

        AssertEx.Equal(
            "res://scenes/combat/energy_counters/ironclad_energy_counter.tscn",
            character.CustomEnergyCounterPath,
            "Architect should temporarily reuse a base-game energy counter while custom Godot C# scene scripts are unavailable.");
        AssertEx.Equal(
            "res://scenes/vfx/card_trail_ironclad.tscn",
            character.CustomTrailPath,
            "Architect should temporarily reuse a base-game card trail while custom Godot C# scene scripts are unavailable.");
    }

    [ArchitectTest]
    public static void ImageHelpersShouldResolveInsideTheMod()
    {
        string[] paths =
        [
            "strike_architect.png".CardImagePath(),
            "strike_architect.png".BigCardImagePath(),
            "destroyer_power.png".PowerImagePath(),
            "destroyer_power.png".BigPowerImagePath(),
            "foundational_compass.png".RelicImagePath(),
            "foundational_compass.png".BigRelicImagePath(),
            "character_icon_architect.png".CharacterUiPath()
        ];

        foreach (string path in paths)
        {
            ResourcePathAssertions.IsLocalModResource(path, "Helper-generated image path should resolve inside the mod");
            AssertEx.False(path.Contains("res://images/", StringComparison.OrdinalIgnoreCase), "Helper-generated image path should not point at base-game image roots");
        }
    }

    [ArchitectTest]
    public static void RuntimeSourceFilesShouldNotContainTemporaryBaseGameCharacterPaths()
    {
        string repoRoot = TestPaths.RepoRoot;
        string[] directoriesToScan =
        [
            Path.Combine(repoRoot, "TheArchitect"),
            Path.Combine(repoRoot, "TheArchitectCode")
        ];
        string[] filePatterns = ["*.cs", "*.tscn", "*.tres"];

        List<string> hits = [];

        foreach (string directory in directoriesToScan)
        {
            foreach (string pattern in filePatterns)
            {
                foreach (string file in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
                {
                    string contents = File.ReadAllText(file);

                    foreach (string fragment in ForbiddenBaseGameRuntimePaths)
                    {
                        if (contents.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                        {
                            hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains {fragment}");
                        }
                    }
                }
            }
        }

        AssertEx.True(hits.Count == 0, $"Runtime source files should not retain temporary base-game character paths. Hits: {string.Join("; ", hits)}");
    }

    [ArchitectTest]
    public static void LocalAnimationResourcesShouldNotPointBackToBaseGameCharacterAssets()
    {
        string repoRoot = TestPaths.RepoRoot;
        string[] files =
        [
            Path.Combine(repoRoot, "TheArchitect", "animations", "characters", "architect", "architect_skel_data.tres"),
            Path.Combine(repoRoot, "TheArchitect", "animations", "characters", "architect", "battle", "architect_battle_skel_data.tres"),
            Path.Combine(repoRoot, "TheArchitect", "animations", "merchant", "architect", "architect_merchant_skel_data.tres"),
            Path.Combine(repoRoot, "TheArchitect", "animations", "rest_site", "architect", "architect_rest_site_skel_data.tres"),
            Path.Combine(repoRoot, "TheArchitect", "animations", "character_select", "architect", "architect_character_select_skel_data.tres")
        ];

        string[] forbiddenPrefixes =
        [
            "res://animations/characters/",
            "res://animations/merchant/",
            "res://animations/rest_site/",
            "res://animations/character_select/"
        ];

        List<string> hits = [];
        foreach (string file in files)
        {
            string contents = File.ReadAllText(file);
            foreach (string forbiddenPrefix in forbiddenPrefixes)
            {
                if (contents.Contains(forbiddenPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains {forbiddenPrefix}");
                }
            }
        }

        AssertEx.True(hits.Count == 0, $"Local animation resources should not point back to external base-game animation roots. Hits: {string.Join("; ", hits)}");
    }

    [ArchitectTest]
    public static void LocalPlaceholderArtFilesShouldExistForCardsPowersAndRelics()
    {
        string repoRoot = TestPaths.RepoRoot;

        foreach (string id in GetLocalizationIds(Path.Combine("TheArchitect", "localization", "eng", "cards.json")))
        {
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "card_portraits", $"{id}.png")), $"Missing local card portrait for {id}");
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "card_portraits", "big", $"{id}.png")), $"Missing local big card portrait for {id}");
        }

        foreach (string id in GetLocalizationIds(Path.Combine("TheArchitect", "localization", "eng", "powers.json")))
        {
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "powers", $"{id}.png")), $"Missing local power icon for {id}");
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "powers", "big", $"{id}.png")), $"Missing local big power icon for {id}");
        }

        foreach (string id in GetLocalizationIds(Path.Combine("TheArchitect", "localization", "eng", "relics.json")))
        {
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "relics", $"{id}.png")), $"Missing local relic icon for {id}");
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "relics", "big", $"{id}.png")), $"Missing local big relic icon for {id}");
            AssertEx.True(File.Exists(Path.Combine(repoRoot, "TheArchitect", "images", "relics", $"{id}_outline.png")), $"Missing local relic outline for {id}");
        }
    }

    [ArchitectTest]
    public static void LocalCardPortraitFilesShouldMatchBaseGamePortraitDimensions()
    {
        string repoRoot = TestPaths.RepoRoot;

        foreach (string id in GetLocalizationIds(Path.Combine("TheArchitect", "localization", "eng", "cards.json")))
        {
            string smallPortrait = Path.Combine(repoRoot, "TheArchitect", "images", "card_portraits", $"{id}.png");
            string bigPortrait = Path.Combine(repoRoot, "TheArchitect", "images", "card_portraits", "big", $"{id}.png");

            (int smallWidth, int smallHeight) = ReadPngDimensions(smallPortrait);
            (int bigWidth, int bigHeight) = ReadPngDimensions(bigPortrait);

            AssertEx.True(
                smallWidth == 250 && smallHeight == 190,
                $"{id}.png should match base-game card portrait dimensions 250x190, but was {smallWidth}x{smallHeight}");
            AssertEx.True(
                bigWidth == 1000 && bigHeight == 760,
                $"big/{id}.png should use the approved large card portrait dimensions 1000x760, but was {bigWidth}x{bigHeight}");
        }
    }

    [ArchitectTest]
    public static void CardArtManifestShouldCoverEveryLocalizedCard()
    {
        string repoRoot = TestPaths.RepoRoot;
        string manifestPath = Path.Combine(repoRoot, ".artifacts", "generated-assets", "card-art", "card_art_manifest.csv");

        AssertEx.True(File.Exists(manifestPath), "Card art manifest should exist before starting bulk card-image generation");

        HashSet<string> manifestIds = ReadCsv(manifestPath)
            .Select(row => row["card_id"].Trim())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string id in GetLocalizationIds(Path.Combine("TheArchitect", "localization", "eng", "cards.json")))
        {
            AssertEx.True(manifestIds.Contains(id), $"Card art manifest should include {id}");
        }
    }

    [ArchitectTest]
    public static void CardArtManifestShouldSpecifyBaseGameReferencesForGenerationTargets()
    {
        string repoRoot = TestPaths.RepoRoot;
        string manifestPath = Path.Combine(repoRoot, ".artifacts", "generated-assets", "card-art", "card_art_manifest.csv");

        AssertEx.True(File.Exists(manifestPath), "Card art manifest should exist before validating reference selections");

        foreach (Dictionary<string, string> row in ReadCsv(manifestPath))
        {
            string cardId = row["card_id"].Trim();
            string rarity = row["rarity"].Trim();
            string referenceCard = row["base_game_reference_card"].Trim();
            string referenceReason = row["reference_reason"].Trim();

            if (string.Equals(rarity, "Token", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AssertEx.NotEmpty(referenceCard, $"Card art manifest should include a base-game reference card for {cardId}");
            AssertEx.NotEmpty(referenceReason, $"Card art manifest should include a base-game reference reason for {cardId}");
        }
    }

    [ArchitectTest]
    public static void LocalCharacterUiFilesShouldExistForTheApprovedBatch()
    {
        string repoRoot = TestPaths.RepoRoot;

        string[] files =
        [
            Path.Combine(repoRoot, "TheArchitect", "images", "charui", "character_icon_architect.png"),
            Path.Combine(repoRoot, "TheArchitect", "images", "charui", "character_icon_architect_outline.png"),
            Path.Combine(repoRoot, "TheArchitect", "images", "charui", "char_select_architect.png"),
            Path.Combine(repoRoot, "TheArchitect", "images", "charui", "char_select_architect_locked.png"),
            Path.Combine(repoRoot, "TheArchitect", "images", "charui", "map_marker_architect.png")
        ];

        foreach (string file in files)
        {
            AssertEx.True(File.Exists(file), $"Missing approved Architect character UI file: {Path.GetFileName(file)}");
        }
    }

    [ArchitectTest]
    public static void LocalCharacterUiFilesShouldMatchBaseGameDimensions()
    {
        string repoRoot = TestPaths.RepoRoot;

        Dictionary<string, (int Width, int Height)> expectedSizes = new()
        {
            ["character_icon_architect.png"] = (88, 88),
            ["character_icon_architect_outline.png"] = (88, 88),
            ["char_select_architect.png"] = (132, 195),
            ["char_select_architect_locked.png"] = (132, 195),
            ["map_marker_architect.png"] = (49, 64)
        };

        foreach ((string fileName, (int expectedWidth, int expectedHeight)) in expectedSizes)
        {
            string file = Path.Combine(repoRoot, "TheArchitect", "images", "charui", fileName);

            (int width, int height) = ReadPngDimensions(file);

            AssertEx.True(
                width == expectedWidth && height == expectedHeight,
                $"{fileName} should match base-game dimensions {expectedWidth}x{expectedHeight}, but was {width}x{height}");
        }
    }

    [ArchitectTest]
    public static void LocalCharacterScenesShouldNotReferenceExternalProjectScriptsOrAssets()
    {
        string repoRoot = TestPaths.RepoRoot;
        string[] files =
        [
            Path.Combine(repoRoot, "TheArchitect", "scenes", "combat", "energy_counters", "architect_energy_counter.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "merchant", "characters", "architect_merchant.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "rest_site", "characters", "architect_rest_site.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "screens", "char_select", "char_select_bg_architect.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "vfx", "card_trail_architect.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "vfx", "energy", "architect", "architect_energy_vfx_back.tscn"),
            Path.Combine(repoRoot, "TheArchitect", "scenes", "vfx", "energy", "architect", "architect_energy_vfx_front.tscn")
        ];

        string[] forbiddenPrefixes =
        [
            "res://src/Core/Nodes/",
            "res://scenes/ui/selection_reticle.tscn",
            "res://scenes/backgrounds/little_light_script.gd",
            "res://scenes/vfx/",
            "res://images/vfx/",
            "res://images/packed/vfx/",
            "res://images/ui/combat/combat_reticle.png"
        ];

        List<string> hits = [];
        foreach (string file in files)
        {
            string contents = File.ReadAllText(file);
            foreach (string forbiddenPrefix in forbiddenPrefixes)
            {
                if (contents.Contains(forbiddenPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    hits.Add($"{Path.GetRelativePath(repoRoot, file)} contains {forbiddenPrefix}");
                }
            }
        }

        AssertEx.True(hits.Count == 0, $"Local character scenes should not reference external project scripts or shared base-game assets. Hits: {string.Join("; ", hits)}");
    }
}
