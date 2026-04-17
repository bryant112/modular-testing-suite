using System.Text.RegularExpressions;

namespace Mts.Core;

public static class TemplateResolver
{
    private static readonly Regex TokenRegex = new(@"\{(?<key>[A-Za-z0-9_-]+)\}", RegexOptions.Compiled);

    public static string Resolve(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return TokenRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return variables.TryGetValue(key, out var value) ? value : string.Empty;
        });
    }
}
