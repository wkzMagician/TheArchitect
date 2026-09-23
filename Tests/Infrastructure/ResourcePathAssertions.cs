namespace TheArchitect.Tests.Infrastructure;

public static class ResourcePathAssertions
{
    public static void IsLocalModResource(string? path, string message)
    {
        AssertEx.NotEmpty(path, message);
        AssertEx.True(path!.StartsWith("res://TheArchitect/") || path.StartsWith("TheArchitect/"), $"{message}. Path should resolve inside the mod");
    }

    public static void DoesNotContainAny(string? path, IEnumerable<string> forbiddenFragments, string message)
    {
        AssertEx.NotEmpty(path, message);

        foreach (string fragment in forbiddenFragments)
        {
            AssertEx.False(path!.Contains(fragment, StringComparison.OrdinalIgnoreCase), $"{message}. Forbidden fragment: {fragment}");
        }
    }
}
