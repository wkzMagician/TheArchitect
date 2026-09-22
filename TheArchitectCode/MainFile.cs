using System.Reflection;
using System.Runtime.Loader;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;

namespace TheArchitect.TheArchitectCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "TheArchitect";
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        WriteRunnerStatus("mod-initialize");
        Harmony harmony = new(ModId);
        harmony.PatchAll();
        if (System.Environment.GetEnvironmentVariable("THEARCHITECT_RUN_TESTS") == "1" || File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "TheArchitect.run-tests")) || File.Exists(Path.Combine(System.Environment.GetEnvironmentVariable("THEARCHITECT_GAME_PROJECT") ?? string.Empty, "TheArchitect.run-tests")))
            _ = RunTestsAfterStartup();
    }

    private static async Task RunTestsAfterStartup()
    {
        for (int attempt = 0; attempt < 240 && !ModManager.IsRunningModded(); attempt++)
            await Task.Delay(500);
        // The game fills ModelDb during its own startup sequence, which runs after
        // mods are loaded. Tests build run and combat state out of ModelDb, so wait
        // for the game to finish that step before running them.
        for (int attempt = 0; attempt < 240 && !IsModelDbReady(); attempt++)
            await Task.Delay(500);
        Logger.Info("Combat test runner requested");
        try
        {
            WriteRunnerStatus("runner-started");
            string? configured = System.Environment.GetEnvironmentVariable("THEARCHITECT_TEST_ASSEMBLY");
            string[] candidates = string.IsNullOrWhiteSpace(configured)
                ? [Path.Combine(AppContext.BaseDirectory, "TheArchitect.Tests.dll"), Path.Combine(AppContext.BaseDirectory, "mods", "TheArchitect", "TheArchitect.Tests.dll")]
                : [configured];
            string? path = candidates.FirstOrDefault(File.Exists);
            if (path is null) throw new FileNotFoundException("TheArchitect.Tests.dll was not found.");
            Logger.Info($"Loading combat test assembly: {path}");
            // The mod runs in its own load context, so the tests must be loaded
            // there too; otherwise test types that derive from mod types bind
            // against a second copy of TheArchitect and fail to load.
            AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(typeof(MainFile).Assembly) ?? AssemblyLoadContext.Default;
            context.Resolving -= ResolveTestDependency;
            context.Resolving += ResolveTestDependency;
            Assembly assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path));
            Type runner = assembly.GetType("TheArchitect.Tests.CombatTestRunner", throwOnError: true)!;
            MethodInfo method = runner.GetMethod("RunAllAsync", BindingFlags.Public | BindingFlags.Static)!;
            string reportPath = System.Environment.GetEnvironmentVariable("THEARCHITECT_TEST_REPORT") ?? Path.Combine(Path.GetDirectoryName(path)!, "TheArchitect-combat-tests.log");
            await using StreamWriter report = new(reportPath, append: false);
            object? result = method.Invoke(null, [report]);
            if (result is Task task) await task;
            WriteRunnerStatus("runner-completed");
            Logger.Info("Combat test runner completed");
        }
        catch (Exception ex)
        {
            WriteRunnerStatus("runner-failed: " + ex);
            Logger.Error($"Combat test runner failed: {ex}");
        }
    }

    private static bool IsModelDbReady()
    {
        try
        {
            return ModelDb.Acts.Any();
        }
        catch
        {
            return false;
        }
    }

    private static void WriteRunnerStatus(string message)
    {
        string? report = System.Environment.GetEnvironmentVariable("THEARCHITECT_TEST_REPORT");
        if (string.IsNullOrWhiteSpace(report)) return;
        try { File.AppendAllText(report, $"STATUS {DateTimeOffset.Now:O} {message}{System.Environment.NewLine}"); } catch { }
    }


    private static Assembly? ResolveTestDependency(AssemblyLoadContext context, AssemblyName name)
    {
        if (!string.Equals(name.Name, "BaseLib", StringComparison.OrdinalIgnoreCase) && !string.Equals(name.Name, "GodotSharp", StringComparison.OrdinalIgnoreCase)) return null;
        string? testAssembly = System.Environment.GetEnvironmentVariable("THEARCHITECT_TEST_ASSEMBLY");
        string[] candidates = string.Equals(name.Name, "GodotSharp", StringComparison.OrdinalIgnoreCase)
            ? [@"D:\godot_v4.5.1\GodotSharp\Api\Debug\GodotSharp.dll", @"D:\godot_v4.5.1\GodotSharp\Api\Release\GodotSharp.dll"]
            : [
                string.IsNullOrWhiteSpace(testAssembly) ? string.Empty : Path.Combine(Path.GetDirectoryName(testAssembly) ?? string.Empty, "BaseLib.dll"),
                Path.Combine(AppContext.BaseDirectory, "mods", "BaseLib.3.4.7", "BaseLib.dll"),
                @"D:\godot_v4.5.1\mods\BaseLib.3.4.7\BaseLib.dll"
            ];
        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate)) return context.LoadFromAssemblyPath(candidate);
        }
        return null;
    }

}



