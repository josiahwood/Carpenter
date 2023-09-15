﻿using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ChatSummary
    {
        public Guid id;
        public string userId;
        public DateTime startTimestamp; // inclusive
        public DateTime endTimestamp;   // exclusive
        public uint originalHash;
        public string original;
        public string summary;
        public Guid messageGenerationId;

        public static async Task<IEnumerable<ChatSummary>> GetChatSummaries(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summaries");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", user.userId);

            List<ChatSummary> chatSummaries = new();

            using (var iterator = container.GetItemQueryIterator<ChatSummary>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var chatSummary in readNext)
                    {
                        chatSummaries.Add(chatSummary);
                    }
                }
            }

            return chatSummaries;
        }

        public static async Task<ChatSummary> GetChatSummaryFromHash(CosmosClient client, CarpenterUser user, uint originalHash)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summaries");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @searchterm AND c.originalHash = @originalHash")
                .WithParameter("@searchterm", user.userId)
                .WithParameter("@originalHash", originalHash);

            using var iterator = container.GetItemQueryIterator<ChatSummary>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var chatSummary = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (chatSummary != null)
                {
                    return chatSummary;
                }
            }

            return null;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-summaries");
            await container.CreateItemAsync(this);
        }
    }
}
