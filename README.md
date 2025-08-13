# MCP Server Experiments

Using [Building MCP Servers for your APIs with .NET and C#](https://anuraj.dev/blog/building-mcp-servers-for-your-apis/) as a reference to create and test an MCP server.

I needed to do the following in order to follow the instructions in this post:

1. Create C:\Users\hokie\AppData\Roaming\npm
2. When connected to the MCP Inspector , prior to clicking Connect you need to copy the “Session token” provided in the console by MCP Inspector into the inspector’s Proxy Session Token property.

In order to get the MCP Server to read its config properly when used in Claude, I had to change the configuration builder thusly:
```csharp
var settingsPath = Environment.GetEnvironmentVariable("MCP_SERVER_SETTINGS_PATH");

// Remove defaults (appsettings.json, env vars, command-line, etc.)
builder.Configuration.Sources.Clear();

// Add only what you want
builder.Configuration
    .AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
```

The path to the config file probably should be an environment variable which would be placed in the MCP server’s config settings.

My experiment is going well now. I have an MCP Server with two tools: echo and get_config_value. I wanted to write this second tool to verify that I could get an MCP Server that had its own config file, because this is where we will (likely, for now) store keys, etc. Clearly better authentication needs to happen somewhere to protect these values.

Here’s an interesting conversation I had with Claude using this MCP Server. Note that it figured out that it needed to chain two tools in order to echo a config setting that it didn’t already have in the conversation. But if I asked it to echo a config value that it had already asked about, it didn’t reuse the get_config_value tool.

https://claude.ai/share/4f1a6d83-7e64-48b3-9a96-839dc2781d11

## MCP Server Config Example
```
    "mcp_server": {
      "command": "D:\\source\\mcp-server\\bin\\Debug\\net8.0\\mcp-server.exe",
	  "cwd": "D:\\source\\mcp-server\\bin\\Debug\\net8.0",
      "args": [],
      "env": {
        "DOTNET_ENVIRONMENT": "Production",
        "WEATHER_CHOICES": "sunny,humid,freezing",
        ""MCP_SERVER_SETTINGS_PATH": "<path to appSettings.json>"
      }
    }
```

## See Also

[7 MCP Server Best Practices for Scalable AI Integrations in 2025 - MarkTechPost](https://www.marktechpost.com/2025/07/23/7-mcp-server-best-practices-for-scalable-ai-integrations-in-2025/)

[modelcontextprotocol/servers: Model Context Protocol Servers](https://github.com/modelcontextprotocol/servers)

[Azure-Samples/mcp: Links to samples, tools, and resources for building and integrating Model Context Protocol (MCP) servers on Azure using multiple languages](https://github.com/Azure-Samples/mcp)

[MCP Catalog: Finding the Right AI Tools for Your Project | Docker](https://www.docker.com/blog/finding-the-right-ai-developer-tools-mcp-catalog/)

https://devblogs.microsoft.com/blog/can-you-build-agent2agent-communication-on-mcp-yes
