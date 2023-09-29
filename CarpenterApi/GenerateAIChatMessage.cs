using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
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
using StableHordeApi;

namespace CarpenterApi
{
    public class GenerateAIChatMessage
    {
        private readonly ILogger<GenerateAIChatMessage> _logger;

        public GenerateAIChatMessage(ILogger<GenerateAIChatMessage> log)
        {
            _logger = log;
        }

        [FunctionName("GenerateAIChatMessage")]
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
            var messageGenerations = await MessageGeneration.GenerateAIChatMessageAlternatives(client, user);

            return new OkObjectResult(messageGenerations);
        }
    }
}

