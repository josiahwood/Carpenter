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
    public class CompareModels
    {
        private readonly ILogger<CompareModels> _logger;

        public CompareModels(ILogger<CompareModels> log)
        {
            _logger = log;
        }

        [FunctionName("CompareModels")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "winnerModel", "loserModel" })]
        [OpenApiParameter(name: "winnerModel", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **id** parameter")]
        [OpenApiParameter(name: "loserModel", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **id** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-memories",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string winnerModel = req.Query["winnerModel"];
            string loserModel = req.Query["loserModel"];
            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);
            await ModelInfo.CompareModels(client, user, winnerModel, loserModel);
            return new OkResult();
        }
    }
}

