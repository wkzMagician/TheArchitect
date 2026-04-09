using System.Reflection;
using TheArchitect.Tests.Infrastructure;

List<(string Name, Exception Error)> failures = [];
int passed = 0;

IEnumerable<(Type Type, MethodInfo Method)> tests = typeof(Program).Assembly
    .GetTypes()
    .OrderBy(type => type.FullName, StringComparer.Ordinal)
    .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
    .Where(method => method.GetCustomAttribute<ArchitectTestAttribute>() != null)
    .OrderBy(item => item.DeclaringType!.FullName, StringComparer.Ordinal)
    .ThenBy(item => item.Name, StringComparer.Ordinal)
    .Select(method => (method.DeclaringType!, method));

foreach ((Type type, MethodInfo method) in tests)
{
    string testName = $"{type.FullName}.{method.Name}";
    try
    {
        object? result = method.Invoke(null, null);
        if (result is Task task)
        {
            await task;
        }

        Console.WriteLine($"PASS {testName}");
        passed++;
    }
    catch (TargetInvocationException ex) when (ex.InnerException != null)
    {
        failures.Add((testName, ex.InnerException));
        Console.WriteLine($"FAIL {testName}: {ex.InnerException.Message}");
    }
    catch (Exception ex)
    {
        failures.Add((testName, ex));
        Console.WriteLine($"FAIL {testName}: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"Executed {passed + failures.Count} tests: {passed} passed, {failures.Count} failed.");

if (failures.Count > 0)
{
    Console.WriteLine();
    foreach ((string name, Exception error) in failures)
    {
        Console.WriteLine($"[{name}]");
        Console.WriteLine(error);
        Console.WriteLine();
    }

    Environment.Exit(1);
}
