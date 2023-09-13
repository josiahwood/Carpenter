using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CarpenterApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CarpenterApi
{
    public class GetChatMessages
    {
        private readonly ILogger<GetChatMessages> _logger;

        public GetChatMessages(ILogger<GetChatMessages> log)
        {
            _logger = log;
        }

        [FunctionName("GetChatMessages")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-messages",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string userId = "00000000000000000000000000000000";

            if (claimsPrincipal != null)
            {
                Claim nameIdentifierClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

                if (nameIdentifierClaim != null)
                {
                    userId = nameIdentifierClaim.Value;
                }
            }

            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-messages");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm ORDER BY c.timestamp ASC").WithParameter("@searchterm", userId);

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

            return new OkObjectResult(chatMessages);
        }
    }
}

