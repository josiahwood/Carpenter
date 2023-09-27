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
    public class ExecuteChatInstruction
    {
        private readonly ILogger<ExecuteChatInstruction> _logger;

        public ExecuteChatInstruction(ILogger<ExecuteChatInstruction> log)
        {
            _logger = log;
        }

        [FunctionName("ExecuteChatInstruction")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-messages",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            ChatMemory chatMemory = await ChatMemory.GetChatMemory(client, user);
            var chatMessages = await ChatMessage.GetChatMessages(client, user);
            string chatInstruction = await new StreamReader(req.Body).ReadToEndAsync();

            MessageGeneration messageGeneration = await PromptGeneration.NextChatInstructionGeneration(client, user, chatMemory, chatMessages, chatInstruction, MessageGeneration.MaxInputLength);

            messageGeneration = await MessageGeneration.StartGeneration(client, messageGeneration);

            return new OkObjectResult(messageGeneration);
        }
    }
}

