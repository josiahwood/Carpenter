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
        //public class Alternate
        //{
        //    public string message;
        //    public Guid messageGenerationId;
        //}
        
        public const string UserSender = "User";
        public const string AISender = "AI";
        
        public Guid id;
        public string userId;
        public DateTime timestamp;
        public string sender;
        public string message;
        public Guid messageGenerationId;
        //public Alternate[] alternates;

        public static async Task<IList<ChatMessage>> GetChatMessages(CosmosClient client, CarpenterUser user)
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

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-messages");
            await container.ReplaceItemAsync(this, id.ToString());
        }

        public static async Task Delete(CosmosClient client, string id)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-messages");
            await container.DeleteItemAsync<ChatMessage>(id, new PartitionKey(id));
        }
    }
}
