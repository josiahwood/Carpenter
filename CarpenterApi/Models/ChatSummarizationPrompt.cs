using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ChatSummarizationPrompt
    {
        public Guid id;
        public string userId;
        public string prompt;

        public static async Task<ChatSummarizationPrompt> GetChatSummarizationPrompt(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summarization-prompts");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", user.userId);

            using var iterator = container.GetItemQueryIterator<ChatSummarizationPrompt>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var chatSummarizationPrompt = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (chatSummarizationPrompt != null)
                {
                    return chatSummarizationPrompt;
                }
            }

            return null;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summarization-prompts");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summarization-prompts");
            await container.ReplaceItemAsync(this, id.ToString());
        }
    }
}
