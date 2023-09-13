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
    public class SendUserChatMessage
    {
        private readonly ILogger<SendUserChatMessage> _logger;

        public SendUserChatMessage(ILogger<SendUserChatMessage> log)
        {
            _logger = log;
        }

        [FunctionName("SendUserChatMessage")]
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

            string userChatMessage = await new StreamReader(req.Body).ReadToEndAsync();
            
            ChatMessage chatMessage = new()
            {
                id = Guid.NewGuid(),
                userId = user.userId,
                timestamp = DateTime.UtcNow,
                sender = ChatMessage.UserSender,
                message = userChatMessage
            };

            await chatMessage.Write(client);

            return new OkResult();
        }
    }
}

