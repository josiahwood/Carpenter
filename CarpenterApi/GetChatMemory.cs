using System;
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
    public class GetChatMemory
    {
        private readonly ILogger<GetChatMemory> _logger;

        public GetChatMemory(ILogger<GetChatMemory> log)
        {
            _logger = log;
        }

        [FunctionName("GetChatMemory")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-memories",
                Connection = "CosmosDbConnectionString", CreateIfNotExists = true
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

            Container container = client.GetDatabase("carpenter-dev").GetContainer("chat-memories");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", userId);
            
            using (var iterator = container.GetItemQueryIterator<ChatMemory>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var chatMemory = (await iterator.ReadNextAsync()).FirstOrDefault();

                    if (chatMemory != null)
                    {
                        return new OkObjectResult(chatMemory.memory);
                    }
                }
            }

            return new OkObjectResult("");
        }
    }
}

