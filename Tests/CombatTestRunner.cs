using System.Reflection;
using TheArchitect.Tests.Infrastructure;

namespace TheArchitect.Tests;

/// Executes the in-game combat test suite.
public static class CombatTestRunner
{
    public static async Task<int> RunAllAsync(TextWriter output)
    {
        List<(string Name, Exception Error)> failures = [];
        int passed = 0;
        IEnumerable<(Type Type, MethodInfo Method)> tests = typeof(CombatTestRunner).Assembly
            .GetTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(method => method.GetCustomAttribute<ArchitectTestAttribute>() != null)
            .OrderBy(item => item.DeclaringType!.FullName, StringComparer.Ordinal)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .Select(method => (method.DeclaringType!, method));

        foreach ((Type type, MethodInfo method) in tests)
        {
            string name = $"{type.FullName}.{method.Name}";
            try
            {
                object? result = method.Invoke(null, null);
                if (result is Task task) await task.ConfigureAwait(false);
                await output.WriteLineAsync($"PASS {name}");
                passed++;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                failures.Add((name, ex.InnerException));
                await output.WriteLineAsync($"FAIL {name}: {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                failures.Add((name, ex));
                await output.WriteLineAsync($"FAIL {name}: {ex.Message}");
            }
        }

        await output.WriteLineAsync($"Executed {passed + failures.Count} tests: {passed} passed, {failures.Count} failed.");
        foreach ((string name, Exception error) in failures)
        {
            await output.WriteLineAsync($"[{name}] {error}");
        }
        return failures.Count == 0 ? 0 : 1;
    }
}
