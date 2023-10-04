using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    public class ChatContext
    {
        public Guid id;
        public string userId;
        public string name;
        public string memory;
        public string authorsNote;
        public string summarizationPrompt;

        internal static async Task<ChatContext> GetChatContext(CosmosClient client, CarpenterUser user, Guid id)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-contexts");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @userId AND c.id = @id")
                .WithParameter("@userId", user.userId)
                .WithParameter("@id", id);

            using var iterator = container.GetItemQueryIterator<ChatContext>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var chatContext = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (chatContext != null)
                {
                    return chatContext;
                }
            }

            return null;
        }

        internal static async Task<IList<ChatContext>> GetChatContexts(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-contexts");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", user.userId);

            List<ChatContext> chatContexts = new();

            using (var iterator = container.GetItemQueryIterator<ChatContext>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var chatContext in readNext)
                    {
                        chatContexts.Add(chatContext);
                    }
                }
            }

            return chatContexts;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-contexts");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-contexts");
            await container.ReplaceItemAsync(this, id.ToString());
        }

        public static async Task Delete(CosmosClient client, string id)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-contexts");
            await container.DeleteItemAsync<ChatMessage>(id, new PartitionKey(id));
        }
    }
}
