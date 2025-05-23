# Sample using agents with OpenAPI tool in Azure.AI.Agents

In this example we will demonstrate the possibility to use services with [OpenAPI Specification](https://en.wikipedia.org/wiki/OpenAPI_Specification) with the agent. We will use [wttr.in](https://wttr.in) service to get weather. The agent will use an OpenAPI specification file for this service (e.g., `weather_openapi.json`), which is configured in `appsettings.json` and loaded by the C# code.
The C# samples below demonstrate how to configure an agent with an OpenAPI tool, create a conversation thread, send a message, run the agent, and retrieve the response.

## Initialize

First, we set up the application configuration to read settings like the project endpoint, model deployment name, and the path to the OpenAPI specification file. We then create a `PersistentAgentsClient`, define the `openApiSpec` variable, and create a common `OpenApiToolDefinition`. This common setup includes all necessary `using` directives. Finally, synchronous and asynchronous samples show how a `PersistentAgent` is created using this tool definition.

```csharp
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var projectEndpoint = configuration["ProjectEndpoint"];
var modelDeploymentName = configuration["ModelDeploymentName"];
var openApiSpec = configuration["OpenApiSpec"];
PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());

OpenApiToolDefinition openApiToolDef = new(
    name: "get_weather",
    description: "Retrieve weather information for a location",
    spec: BinaryData.FromBytes(File.ReadAllBytes(openApiSpec)),
    openApiAuthentication: new OpenApiAnonymousAuthDetails(),
    defaultParams: ["format"]
);
```

Synchronous sample:

```csharp
PersistentAgent agent = client.Administration.CreateAgent(
    model: modelDeploymentName,
    name: "Open API Tool Calling Agent",
    instructions: "You are a helpful agent.",
    tools: [openApiToolDef]
);
```

Asynchronous sample:

```csharp
PersistentAgent agent = await client.Administration.CreateAgentAsync(
    model: modelDeploymentName,
    name: "Open API Tool Calling Agent",
    instructions: "You are a helpful agent.",
    tools: [openApiToolDef]
);
```

## Threads and Messages

With the agent configured, we create a `PersistentAgentThread` to manage the conversation. We then add an initial user message to this thread, for example, asking for the weather in a specific location.

Synchronous sample:

```csharp
PersistentAgentThread thread = client.Threads.CreateThread();

client.Messages.CreateMessage(
    thread.Id,
    MessageRole.User,
    "What's the weather in Seattle?");
```

Asynchronous sample:

```csharp
PersistentAgentThread thread = await client.Threads.CreateThreadAsync();

await client.Messages.CreateMessageAsync(
    thread.Id,
    MessageRole.User,
    "What's the weather in Seattle?");
```

## Start Run and Polling

To process the message(s) in the thread, we create a `ThreadRun` associated with the thread and the agent. The status of this run is then polled until it reaches a terminal state (e.g., completed, failed), indicating the agent has finished processing.

Synchronous sample:

```csharp
ThreadRun run = client.Runs.CreateRun(
    thread.Id,
    agent.Id);

do
{
    Thread.Sleep(TimeSpan.FromMilliseconds(500));
    run = client.Runs.GetRun(thread.Id, run.Id);
}
while (run.Status == RunStatus.Queued
    || run.Status == RunStatus.InProgress);
```

Asynchronous sample:

```csharp
ThreadRun run = await client.Runs.CreateRunAsync(
    thread.Id,
    agent.Id
);

do
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    run = await client.Runs.GetRunAsync(thread.Id, run.Id);
}
while (run.Status == RunStatus.Queued
    || run.Status == RunStatus.InProgress);
```

## View Messages

Once the agent run has completed, we retrieve all messages from the thread. These messages, which now include the agent's responses, are iterated through, and their content is printed to the console.

Synchronous sample:

```csharp
Pageable<PersistentThreadMessage> messages = client.Messages.GetMessages(
    thread.Id,
    order: ListSortOrder.Ascending);

foreach (PersistentThreadMessage threadMessage in messages)
{
    foreach (MessageContent content in threadMessage.ContentItems)
    {
        switch (content)
        {
            case MessageTextContent textItem:
                Console.WriteLine($"[{threadMessage.Role}]: {textItem.Text}");
                break;
        }
    }
}
```

Asynchronous sample:

```csharp
AsyncPageable<PersistentThreadMessage> messages = client.Messages.GetMessagesAsync(
    thread.Id,
    order: ListSortOrder.Ascending
);

await foreach (PersistentThreadMessage threadMessage in messages)
{
    foreach (MessageContent content in threadMessage.ContentItems)
    {
        switch (content)
        {
            case MessageTextContent textItem:
                Console.WriteLine($"[{threadMessage.Role}]: {textItem.Text}");
                break;
        }
    }
}
```

## Cleanup Resources

Finally, to clean up resources, we delete the `PersistentAgentThread` and the `PersistentAgent` that were created during this example.

Synchronous sample:

```csharp
client.Threads.DeleteThread(thread.Id);
client.Administration.DeleteAgent(agent.Id);
```

Asynchronous sample:

```csharp
await client.Threads.DeleteThreadAsync(thread.Id);
await client.Administration.DeleteAgentAsync(agent.Id);
```
