using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TheArchitect.Tests.Infrastructure;

public static class BehaviorGoldenAssert
{
    private static readonly string SourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TheArchitectCode"));
    private static readonly Dictionary<string, string> ClassCache = [];

    public static void AssertType(Type type)
    {
        if (!BehaviorGoldenHashes.ByTypeName.TryGetValue(type.Name, out string? expectedHash))
        {
            throw new InvalidOperationException($"No behavior golden hash registered for {type.FullName}.");
        }

        string source = GetNormalizedClassSource(type.Name);
        string actualHash = ComputeHash(source);
        AssertEx.Equal(expectedHash, actualHash, $"{type.Name} source hash should match the recorded behavior golden.");
    }

    private static string GetNormalizedClassSource(string typeName)
    {
        if (!ClassCache.TryGetValue(typeName, out string? source))
        {
            source = NormalizeWhitespace(ExtractClassSource(typeName));
            ClassCache[typeName] = source;
        }

        return source;
    }

    private static string ExtractClassSource(string typeName)
    {
        foreach (string path in Directory.EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(path);
            int classIndex = text.IndexOf($"class {typeName}", StringComparison.Ordinal);
            if (classIndex < 0)
            {
                continue;
            }

            int braceStart = text.IndexOf('{', classIndex);
            if (braceStart < 0)
            {
                continue;
            }

            int depth = 0;
            for (int i = braceStart; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    depth++;
                }
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        int start = text.LastIndexOf("public", classIndex, StringComparison.Ordinal);
                        if (start < 0)
                        {
                            start = classIndex;
                        }

                        return text[start..(i + 1)];
                    }
                }
            }
        }

        throw new FileNotFoundException($"Could not find source for class {typeName} under {SourceRoot}.");
    }

    private static string NormalizeWhitespace(string source)
    {
        string withoutComments = Regex.Replace(source, @"//.*?$|/\*.*?\*/", string.Empty, RegexOptions.Multiline | RegexOptions.Singleline);
        return Regex.Replace(withoutComments, @"\s+", " ").Trim();
    }

    private static string ComputeHash(string source)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes);
    }
}
