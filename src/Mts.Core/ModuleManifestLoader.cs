using System.Text.Json;

namespace Mts.Core;

public static class ModuleManifestLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IReadOnlyList<TestModuleDefinition> LoadModules(string modulesFolder)
    {
        if (!Directory.Exists(modulesFolder))
        {
            return Array.Empty<TestModuleDefinition>();
        }

        var modules = new List<TestModuleDefinition>();
        foreach (var path in Directory.EnumerateFiles(modulesFolder, "*.json", SearchOption.TopDirectoryOnly).OrderBy(Path.GetFileName))
        {
            var json = File.ReadAllText(path);
            var module = JsonSerializer.Deserialize<TestModuleDefinition>(json, JsonOptions);
            if (module is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(module.Id))
            {
                module.Id = Path.GetFileNameWithoutExtension(path);
            }

            if (string.IsNullOrWhiteSpace(module.Name))
            {
                module.Name = module.Id;
            }

            modules.Add(module);
        }

        return modules;
    }
}
