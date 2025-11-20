// Top-level statements - code directly in the global namespace
using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.IO;
using mcp_server;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

var settingsPath = Environment.GetEnvironmentVariable("MCP_SERVER_SETTINGS_PATH");

// Remove defaults (appsettings.json, env vars, command-line, etc.)
builder.Configuration.Sources.Clear();

// Add only what you want
builder.Configuration
    .AddJsonFile(settingsPath,
                  optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);


//Display warnings and above only in console.
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Also log to a file located in the same folder as the settingsPath (if available)
try
{
    var settingsDirectory = !string.IsNullOrWhiteSpace(settingsPath)
        ? Path.GetDirectoryName(settingsPath)
        : AppContext.BaseDirectory;

    if (string.IsNullOrWhiteSpace(settingsDirectory))
    {
        settingsDirectory = AppContext.BaseDirectory;
    }

    Directory.CreateDirectory(settingsDirectory!);

    var logFilePath = Path.Combine(settingsDirectory!, "mcp-server.log");
    builder.Logging.AddProvider(new SimpleFileLoggerProvider(logFilePath));
}
catch (Exception ex)
{
    // If file logging setup fails, keep the app running and rely on console logging.
    Console.Error.WriteLine($"Failed to initialize file logging: {ex.Message}");
}

builder.Services.AddHttpClient();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client, just for fun.")]
    public static string Echo(string message) => $"Echo : {message}";
}

