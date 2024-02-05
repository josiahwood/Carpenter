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
    public class ChatAuthorsNote
    {
        public Guid id;
        public string userId;
        public string authorsNote;

        public static async Task<ChatAuthorsNote> GetChatAuthorsNote(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-authors-notes");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @userId")
                .WithParameter("@userId", user.userId);

            using var iterator = container.GetItemQueryIterator<ChatAuthorsNote>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var chatAuthorsNote = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (chatAuthorsNote != null)
                {
                    return chatAuthorsNote;
                }
            }

            return null;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-authors-notes");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-authors-notes");
            await container.ReplaceItemAsync(this, id.ToString());
        }
    }
}
