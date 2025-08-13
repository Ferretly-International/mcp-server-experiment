// Top-level statements - code directly in the global namespace
using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

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

