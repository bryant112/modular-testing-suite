using CommunityToolkit.Mvvm.ComponentModel;
using Mts.Core;

namespace Mts.App.ViewModels;

public sealed partial class VariableEntryViewModel : ObservableObject
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string Description { get; init; } = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}

public sealed partial class FeatureToggleViewModel : ObservableObject
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string Description { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;
}

public sealed class ActionItemViewModel
{
    public required ModuleActionDefinition Action { get; init; }
    public string Title => Action.Title;
    public string Description => Action.Description;
    public string Method => Action.Method.ToUpperInvariant();
    public string Tags => Action.Tags.Count == 0 ? string.Empty : string.Join(" | ", Action.Tags);
}
