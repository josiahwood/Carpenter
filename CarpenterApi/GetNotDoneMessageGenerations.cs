using System.Collections.Generic;
using System.IO;
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
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CarpenterApi
{
    public class GetNotDoneMessageGenerations
    {
        private readonly ILogger<GetMessageGenerations> _logger;

        public GetNotDoneMessageGenerations(ILogger<GetMessageGenerations> log)
        {
            _logger = log;
        }

        [FunctionName("GetNotDoneMessageGenerations")]
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

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);
            var messageGenerations = await MessageGeneration.GetNotDoneMessageGenerations(client, user);
            List<MessageGeneration> updatedMessageGenerations = new();

            foreach(var messageGeneration in messageGenerations)
            {
                await messageGeneration.UpdateStatus(client);
                updatedMessageGenerations.Add(messageGeneration);
            }

            return new OkObjectResult(updatedMessageGenerations);
        }
    }
}

