using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace mcp_server.Tools;

[McpServerToolType]
public class ExternalApiTools
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalApiTools> _logger;
    private readonly HttpClient _httpClient;

    public ExternalApiTools(
        IConfiguration configuration,
        ILogger<ExternalApiTools> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(nameof(ExternalApiTools));
    }

    [McpServerTool, Description("Gets the external api client configuration for the current user.")]
    public async Task<string> GetExternalApiClientInfo()
    {
        var endpoint = _configuration.GetValue<string>("ExternalApi:InfoEndpoint", "/info");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("External API info endpoint is not configured.");
        }

        _logger.LogInformation("Fetching external API client info from {Url}", endpoint);
        
        using var request = CreateHttpRequestMessage(endpoint, _configuration.GetValue<string>("ExternalApi:ApiKey"));

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }

    private HttpRequestMessage CreateHttpRequestMessage(string endpoint, string? apiKey)
    {
        var apiUrl = _configuration.GetValue<string>("ExternalApi:BaseUrl");
        var url = apiUrl + endpoint;
        HttpRequestMessage? request = null;
        try
        {
            request = new HttpRequestMessage(HttpMethod.Get, url);
            // add the api key header if configured
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add("X-Api-Key", $"{apiKey}");
            }

            return request;
        }
        catch
        {
            request?.Dispose();
            throw;
        }
    }
}