using System;
using System.IO;
using System.Net;
using System.Net.Mime;
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
    public class ChatContexts
    {
        private readonly ILogger<ChatContexts> _logger;

        public ChatContexts(ILogger<ChatContexts> log)
        {
            _logger = log;
        }

        [FunctionName("GetChatContext")]
        [OpenApiOperation(operationId: "GetChatContext", tags: new[] { "id" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **id** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatContext), Description = "The OK response")]
        public async Task<IActionResult> GetChatContext(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat-contexts/{id:guid}")] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-contexts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal,
            Guid id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            return new OkObjectResult(await ChatContext.GetChatContext(client, user, id));
        }

        [FunctionName("GetChatContexts")]
        [OpenApiOperation(operationId: "GetChatContexts")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatContext[]), Description = "The OK response")]
        public async Task<IActionResult> GetChatContexts(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat-contexts")] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-contexts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            return new OkObjectResult(await ChatContext.GetChatContexts(client, user));
        }

        [FunctionName("CreateChatContext")]
        [OpenApiOperation(operationId: "CreateChatContext")]
        [OpenApiRequestBody("application/json", typeof(ChatContext))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatContext), Description = "The OK response")]
        public async Task<IActionResult> CreateChatContext(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat-contexts")] ChatContext chatContext,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-contexts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            chatContext.id = Guid.NewGuid();
            chatContext.userId = user.userId;

            await chatContext.Write(client);

            return new OkObjectResult(chatContext);
        }

        [FunctionName("UpdateChatContext")]
        [OpenApiOperation(operationId: "UpdateChatContext")]
        [OpenApiRequestBody("application/json", typeof(ChatContext))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatContext), Description = "The OK response")]
        public async Task<IActionResult> UpdateChatContext(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "chat-contexts")] ChatContext chatContext,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-contexts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            if (chatContext.userId != user.userId)
            {
                return new BadRequestResult();
            }

            await chatContext.Write(client);

            return new OkObjectResult(chatContext);
        }

        [FunctionName("DeleteChatContext")]
        [OpenApiOperation(operationId: "DeleteChatContext", tags: new[] { "id" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Description = "The **id** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public async Task<IActionResult> DeleteChatContext(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "chat-contexts/{id:guid}")] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-contexts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            Guid id)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            await ChatContext.Delete(client, id);

            return new OkResult();
        }
    }
}

