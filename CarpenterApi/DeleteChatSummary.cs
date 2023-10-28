using System;
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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CarpenterApi
{
    public class DeleteChatSummary
    {
        private readonly ILogger<DeleteChatSummary> _logger;

        public DeleteChatSummary(ILogger<DeleteChatSummary> log)
        {
            _logger = log;
        }

        [FunctionName("DeleteChatSummary")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "id" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid), Description = "The **id** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Guid), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-messages",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            Guid id = Guid.Parse(req.Query["id"]);

            await ChatSummary.Delete(client, id);
            return new OkResult();
        }
    }
}

