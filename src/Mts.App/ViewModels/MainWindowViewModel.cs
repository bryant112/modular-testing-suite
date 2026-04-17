using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mts.Core;

namespace Mts.App.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly HttpActionExecutor _executor = new();
    private readonly string _workspaceRoot;
    private readonly string _modulesFolder;

    [ObservableProperty]
    private string _baseUrl = "http://localhost:5080";

    [ObservableProperty]
    private string _statusMessage = "Load a module and execute an action.";

    [ObservableProperty]
    private string _requestPreview = string.Empty;

    [ObservableProperty]
    private string _responsePreview = string.Empty;

    [ObservableProperty]
    private string _resultSummary = "No action executed yet.";

    [ObservableProperty]
    private string _moduleSummary = "No modules loaded yet.";

    [ObservableProperty]
    private string _featureSummary = "No feature matrix selected.";

    [ObservableProperty]
    private TestModuleDefinition? _selectedModule;

    [ObservableProperty]
    private ActionItemViewModel? _selectedAction;

    public ObservableCollection<TestModuleDefinition> Modules { get; } = new();
    public ObservableCollection<VariableEntryViewModel> Variables { get; } = new();
    public ObservableCollection<FeatureToggleViewModel> Features { get; } = new();
    public ObservableCollection<ActionItemViewModel> Actions { get; } = new();

    public IAsyncRelayCommand ReloadModulesCommand { get; }
    public IAsyncRelayCommand ExecuteSelectedActionCommand { get; }
    public IRelayCommand OpenModulesFolderCommand { get; }

    public MainWindowViewModel()
    {
        _workspaceRoot = ResolveWorkspaceRoot();
        _modulesFolder = Path.Combine(_workspaceRoot, "modules");

        ReloadModulesCommand = new AsyncRelayCommand(ReloadModulesAsync);
        ExecuteSelectedActionCommand = new AsyncRelayCommand(ExecuteSelectedActionAsync);
        OpenModulesFolderCommand = new RelayCommand(OpenModulesFolder);

        ReloadModulesAsync().GetAwaiter().GetResult();
    }

    partial void OnSelectedModuleChanged(TestModuleDefinition? value)
    {
        Variables.Clear();
        Features.Clear();
        Actions.Clear();

        if (value is null)
        {
            ModuleSummary = "No module selected.";
            return;
        }

        foreach (var variable in value.Variables)
        {
            var entry = new VariableEntryViewModel
            {
                Key = variable.Key,
                Label = string.IsNullOrWhiteSpace(variable.Label) ? variable.Key : variable.Label,
                Description = variable.Description,
                Value = variable.DefaultValue
            };
            entry.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(VariableEntryViewModel.Value))
                {
                    UpdateRequestPreview();
                }
            };
            Variables.Add(entry);
        }

        foreach (var feature in value.Features)
        {
            var toggle = new FeatureToggleViewModel
            {
                Key = feature.Key,
                Label = string.IsNullOrWhiteSpace(feature.Label) ? feature.Key : feature.Label,
                Description = feature.Description,
                IsEnabled = feature.EnabledByDefault
            };
            toggle.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(FeatureToggleViewModel.IsEnabled))
                {
                    UpdateFeatureSummary();
                    UpdateRequestPreview();
                }
            };
            Features.Add(toggle);
        }

        foreach (var action in value.Actions)
        {
            Actions.Add(new ActionItemViewModel { Action = action });
        }

        SelectedAction = Actions.FirstOrDefault();
        ModuleSummary = $"{value.Name} | {value.Description} | target {value.TargetHint}";
        UpdateFeatureSummary();
        UpdateRequestPreview();
    }

    partial void OnSelectedActionChanged(ActionItemViewModel? value)
    {
        UpdateRequestPreview();
    }

    private async Task ReloadModulesAsync()
    {
        Modules.Clear();
        foreach (var module in ModuleManifestLoader.LoadModules(_modulesFolder))
        {
            Modules.Add(module);
        }

        SelectedModule = Modules.FirstOrDefault();
        StatusMessage = Modules.Count == 0
            ? $"No modules found in {_modulesFolder}."
            : $"Loaded {Modules.Count} module(s) from {_modulesFolder}.";
        await Task.CompletedTask;
    }

    private async Task ExecuteSelectedActionAsync()
    {
        if (SelectedAction is null)
        {
            StatusMessage = "Pick an action first.";
            return;
        }

        var variables = BuildExecutionVariables();
        var result = await _executor.ExecuteAsync(BaseUrl, SelectedAction.Action, variables);

        RequestPreview = BuildRequestPreview(SelectedAction.Action, variables, result.ResolvedUrl, result.ResolvedBody);
        ResponsePreview = string.IsNullOrWhiteSpace(result.ErrorMessage) ? result.ResponseBody : result.ErrorMessage;
        ResultSummary = result.Success
            ? $"HTTP {result.StatusCode} in {result.DurationMs} ms"
            : $"Request failed after {result.DurationMs} ms";

        foreach (var capture in result.CapturedValues)
        {
            var target = Variables.FirstOrDefault(v => string.Equals(v.Key, capture.Key, StringComparison.OrdinalIgnoreCase));
            if (target is not null)
            {
                target.Value = capture.Value;
            }
        }

        UpdateFeatureSummary();

        StatusMessage = result.CapturedValues.Count == 0
            ? ResultSummary
            : $"{ResultSummary} | captured {string.Join(", ", result.CapturedValues.Select(kvp => kvp.Key + "=" + kvp.Value))}";
    }

    private void OpenModulesFolder()
    {
        Directory.CreateDirectory(_modulesFolder);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = _modulesFolder,
            UseShellExecute = true
        });
    }

    private void UpdateRequestPreview()
    {
        if (SelectedAction is null)
        {
            RequestPreview = string.Empty;
            return;
        }

        var variables = BuildExecutionVariables();
        var resolvedUrl = ResolveUrl(BaseUrl, SelectedAction.Action.EndpointTemplate, variables);
        var resolvedBody = TemplateResolver.Resolve(SelectedAction.Action.BodyTemplate, variables);
        RequestPreview = BuildRequestPreview(SelectedAction.Action, variables, resolvedUrl, resolvedBody);
    }

    private Dictionary<string, string> BuildExecutionVariables()
    {
        var variables = Variables.ToDictionary(v => v.Key, v => v.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        foreach (var feature in Features)
        {
            variables[feature.Key] = feature.IsEnabled ? "true" : "false";
        }

        return variables;
    }

    private void UpdateFeatureSummary()
    {
        if (Features.Count == 0)
        {
            FeatureSummary = "No feature matrix defined for this module.";
            return;
        }

        var enabled = Features.Where(feature => feature.IsEnabled).Select(feature => feature.Label).ToList();
        FeatureSummary = enabled.Count == 0
            ? "Feature set // baseline only"
            : $"Feature set // {string.Join(", ", enabled)}";
    }

    private static string BuildRequestPreview(ModuleActionDefinition action, IReadOnlyDictionary<string, string> variables, string url, string body)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{action.Method.ToUpperInvariant()} {url}");
        if (variables.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Variables");
            foreach (var variable in variables.OrderBy(kvp => kvp.Key))
            {
                builder.AppendLine($"- {variable.Key} = {variable.Value}");
            }
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.AppendLine();
            builder.AppendLine("Body");
            builder.AppendLine(body);
        }

        return builder.ToString().TrimEnd();
    }

    private static string ResolveUrl(string baseUrl, string endpointTemplate, IReadOnlyDictionary<string, string> variables)
    {
        var endpoint = TemplateResolver.Resolve(endpointTemplate, variables);
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        return $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
    }

    private static string ResolveWorkspaceRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MTS.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return "C:\\dev\\MTS";
    }
}
