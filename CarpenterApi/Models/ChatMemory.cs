using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using StableHordeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ChatMemory
    {
        public Guid id;
        public string userId;
        public string memory;

        public static async Task<ChatMemory> GetChatMemory(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-memories");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", user.userId);

            using (var iterator = container.GetItemQueryIterator<ChatMemory>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var chatMemory = (await iterator.ReadNextAsync()).FirstOrDefault();

                    if (chatMemory != null)
                    {
                        return chatMemory;
                    }
                }
            }

            return null;
        }
    }
}
