using System.Text.RegularExpressions;
using TheArchitect.Tests.Infrastructure;

namespace TheArchitect.Tests.Resources;

public static class ResourceUidIsolationTests
{
    [ArchitectTest]
    public static void ModResourcesDoNotReuseVanillaOrOtherModResourceUids()
    {
        string game = Environment.GetEnvironmentVariable("THEARCHITECT_GAME_PROJECT")
            ?? throw new InvalidOperationException("THEARCHITECT_GAME_PROJECT is required for UID isolation validation");
        var vanilla = Definitions(game).Select(x => x.Uid).ToHashSet();
        AssertEx.True(vanilla.Count > 100, "UID audit must inspect the base-game resource definitions");
        var seen = new Dictionary<string, string>();
        var errors = new List<string>();
        foreach (var (uid, path) in Definitions(TestPaths.RepoPath("TheArchitect"))
                     .Concat(Definitions(TestPaths.RepoPath("TheArchitectCode"))))
        {
            if (vanilla.Contains(uid)) errors.Add($"{path}: reuses vanilla UID {uid}");
            if (!seen.TryAdd(uid, path)) errors.Add($"{path}: shares {uid} with {seen[uid]}");
        }
        AssertEx.True(errors.Count == 0, string.Join("\n", errors));
    }

    private static IEnumerable<(string Uid, string Path)> Definitions(string root)
    {
        foreach (string file in Directory.EnumerateFiles(root))
        {
            string extension = Path.GetExtension(file);
            if (extension is not (".import" or ".uid" or ".tscn" or ".tres")) continue;
            string text = extension == ".import" || extension == ".uid"
                ? File.ReadAllText(file) : File.ReadLines(file).FirstOrDefault() ?? "";
            string uid = extension == ".uid" ? text.Trim()
                : Regex.Match(text, "(?:^|[\\s])uid=\"(uid://[^\"]+)\"").Groups[1].Value;
            if (uid.StartsWith("uid://")) yield return (uid, file);
        }
        foreach (string directory in Directory.EnumerateDirectories(root))
        {
            if (Path.GetFileName(directory) is ".godot" or ".git" or ".artifacts" or "bin" or "obj" or "packages" or "mods" or "node_modules") continue;
            foreach (var definition in Definitions(directory)) yield return definition;
        }
    }
}
