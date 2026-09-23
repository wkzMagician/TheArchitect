namespace TheArchitect.Tests.Infrastructure;

/// <summary>
/// Resolves repository paths while tests run inside the game process, where the
/// current directory and <see cref="AppContext.BaseDirectory"/> point at the game
/// instead of the mod.
/// </summary>
public static class TestPaths
{
    private static readonly Lazy<string> LazyRepoRoot = new(FindRepoRoot);

    public static string RepoRoot => LazyRepoRoot.Value;

    public static string RepoPath(params string[] segments)
    {
        string path = RepoRoot;
        foreach (string segment in segments)
        {
            path = Path.Combine(path, segment);
        }

        return path;
    }

    private static string FindRepoRoot()
    {
        string? configured = Environment.GetEnvironmentVariable("THEARCHITECT_REPO_ROOT");
        if (!string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured))
        {
            return Path.GetFullPath(configured);
        }

        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TheArchitect.csproj")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root above '{AppContext.BaseDirectory}'. Set THEARCHITECT_REPO_ROOT to the mod repository directory.");
    }
}
