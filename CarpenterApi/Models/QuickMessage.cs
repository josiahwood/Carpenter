using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class QuickMessage
    {
        public Guid id;
        public string userId;
        public string message;

        public static async Task<IList<QuickMessage>> GetQuickMessages(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("quick-messages");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", user.userId);

            List<QuickMessage> quickMessages = new();

            using (var iterator = container.GetItemQueryIterator<QuickMessage>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var quickMessage in readNext)
                    {
                        quickMessages.Add(quickMessage);
                    }
                }
            }

            return quickMessages;
        }

        public async Task Write(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("quick-messages");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("quick-messages");
            await container.ReplaceItemAsync(this, id.ToString());
        }
    }
}
