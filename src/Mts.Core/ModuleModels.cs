namespace Mts.Core;

public sealed class TestModuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TargetHint { get; set; } = string.Empty;
    public List<ModuleVariableDefinition> Variables { get; set; } = new();
    public List<FeatureToggleDefinition> Features { get; set; } = new();
    public List<ModuleActionDefinition> Actions { get; set; } = new();
}

public sealed class ModuleVariableDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}

public sealed class FeatureToggleDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool EnabledByDefault { get; set; }
}

public sealed class ModuleActionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string EndpointTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> CaptureJsonFields { get; set; } = new();
}

public sealed class ActionExecutionResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string ResolvedUrl { get; set; } = string.Empty;
    public string ResolvedBody { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public Dictionary<string, string> CapturedValues { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
}
