using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mts.Core;

public sealed class HttpActionExecutor
{
    private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };
    private readonly HttpClient _httpClient;

    public HttpActionExecutor(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ActionExecutionResult> ExecuteAsync(string baseUrl, ModuleActionDefinition action, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var endpoint = TemplateResolver.Resolve(action.EndpointTemplate, variables).Trim();
        var body = TemplateResolver.Resolve(action.BodyTemplate, variables);
        var url = BuildUrl(baseUrl, endpoint);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(action.Method.ToUpperInvariant()), url);
            if (!string.IsNullOrWhiteSpace(body) && request.Method != HttpMethod.Get)
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            var formatted = PrettyPrintJson(raw);
            var captures = ExtractCaptures(raw, action.CaptureJsonFields);

            stopwatch.Stop();
            return new ActionExecutionResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResolvedUrl = url,
                ResolvedBody = body,
                ResponseBody = formatted,
                DurationMs = stopwatch.ElapsedMilliseconds,
                CapturedValues = captures
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ActionExecutionResult
            {
                Success = false,
                StatusCode = 0,
                ResolvedUrl = url,
                ResolvedBody = body,
                ResponseBody = string.Empty,
                DurationMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string BuildUrl(string baseUrl, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return baseUrl.TrimEnd('/');
        }

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        return $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
    }

    private static string PrettyPrintJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        try
        {
            var node = JsonNode.Parse(raw);
            return node?.ToJsonString(PrettyJson) ?? raw;
        }
        catch
        {
            return raw;
        }
    }

    private static Dictionary<string, string> ExtractCaptures(string rawJson, IReadOnlyDictionary<string, string> mappings)
    {
        var captures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (mappings.Count == 0 || string.IsNullOrWhiteSpace(rawJson))
        {
            return captures;
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(rawJson);
        }
        catch
        {
            return captures;
        }

        if (node is null)
        {
            return captures;
        }

        foreach (var mapping in mappings)
        {
            var value = ResolveJsonPath(node, mapping.Value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                captures[mapping.Key] = value;
            }
        }

        return captures;
    }

    private static string ResolveJsonPath(JsonNode root, string path)
    {
        JsonNode? current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            current = current?[segment];
            if (current is null)
            {
                return string.Empty;
            }
        }

        return current switch
        {
            JsonValue value => value.ToJsonString().Trim('"'),
            _ => current.ToJsonString()
        };
    }
}
