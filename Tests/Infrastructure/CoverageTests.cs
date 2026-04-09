using System.Reflection;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Powers;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Infrastructure;

public static class CoverageTests
{
    [ArchitectTest]
    public static void EveryArchitectModelHasAMatchingTestClass()
    {
        Assembly productionAssembly = typeof(TheArchitectCard).Assembly;
        HashSet<string> expected = productionAssembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type =>
                typeof(TheArchitectCard).IsAssignableFrom(type) ||
            type.FullName == "TheArchitect.TheArchitectCode.Cards.Tokens.Drowsy" ||
                typeof(TheArchitectPower).IsAssignableFrom(type) ||
                typeof(TheArchitectRelic).IsAssignableFrom(type))
            .Select(type => $"{type.Name}Tests")
            .ToHashSet(StringComparer.Ordinal);

        HashSet<string> actual = typeof(CoverageTests).Assembly
            .GetTypes()
            .Where(type => type.Name.EndsWith("Tests", StringComparison.Ordinal))
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        List<string> missing = expected.Where(name => !actual.Contains(name)).OrderBy(name => name, StringComparer.Ordinal).ToList();
        AssertEx.True(missing.Count == 0, $"Missing matching test classes: {string.Join(", ", missing)}");
    }
}
