using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// Load configuration from appsettings.json
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var projectEndpoint = configuration["ProjectEndpoint"];
var modelDeploymentName = configuration["ModelDeploymentName"];
// Create a PersistentAgentsClient
PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());

// Create a PersistentAgent
PersistentAgent agent = client.Administration.CreateAgent(
    model: modelDeploymentName,
    name: "Image Understanding Agent",
    instructions: "You are an image-understanding agent. Analyze images and provide textual descriptions."
);

// Create a thread.
PersistentAgentThread thread = client.Threads.CreateThread();

// Url to image for analysis.
MessageImageUriParam imageUrl = new("https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg") { Detail = ImageDetailLevel.High };

var contentBlocks = new List<MessageInputContentBlock>
{
    new MessageInputTextBlock("Could you describe this image?"),
    new MessageInputImageUriBlock(imageUrl)
};

// Create a message.
client.Messages.CreateMessage(
    thread.Id,
    MessageRole.User,
    contentBlocks: contentBlocks
);

// Start run.
ThreadRun run = client.Runs.CreateRun(
    thread.Id,
    agent.Id
);

// Poll until finished.
do
{
    Thread.Sleep(TimeSpan.FromMilliseconds(500));
    run = client.Runs.GetRun(thread.Id, run.Id);
}
while (run.Status == RunStatus.Queued
    || run.Status == RunStatus.InProgress);

// Get messages.
Pageable<PersistentThreadMessage> messages = client.Messages.GetMessages(
    thread.Id,
    order: ListSortOrder.Ascending);

// Print messages.
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

// Clean up resources
client.Threads.DeleteThread(thread.Id);
client.Administration.DeleteAgent(agent.Id);