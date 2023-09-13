using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StableHordeApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ChatMessage
    {
        public const string UserSender = "User";
        public const string AISender = "AI";
        
        public Guid id;
        public string userId;
        public DateTime timestamp;
        public string sender;
        public string message;
        public Guid messageGenerationId;

        public static async Task<IEnumerable<ChatMessage>> GetChatMessages(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-messages");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm ORDER BY c.timestamp ASC").WithParameter("@searchterm", user.userId);

            List<ChatMessage> chatMessages = new();

            using (var iterator = container.GetItemQueryIterator<ChatMessage>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var chatMessage in readNext)
                    {
                        chatMessages.Add(chatMessage);
                    }
                }
            }

            return chatMessages;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-messages");
            await container.CreateItemAsync(this);
        }
    }
}
