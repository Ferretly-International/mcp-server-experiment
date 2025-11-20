using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace mcp_server.Tools;

[McpServerToolType]
public class KreggerTools
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KreggerTools> _logger;
    public KreggerTools(IConfiguration configuration, ILogger<KreggerTools> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    [McpServerTool, Description("Get the current configuration value for a given key.")]
    public string? GetConfigValue(string key)
    {
        if (key.StartsWith("ConnectionStrings", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogWarning("Attempt to access a connection string key '{Key}' was blocked for security reasons.", key);
            throw new UnauthorizedAccessException("Access to connection strings is not allowed.");
            return null; // Prevent access to connection strings for security reasons
        }
        
        var value = _configuration.GetValue<string>(key);
        _logger.LogInformation("Retrieved configuration value for key '{Key}': {Value}", key, value);
        return value;
    }
}