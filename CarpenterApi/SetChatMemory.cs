using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using CarpenterApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace CarpenterApi
{
    public class SetChatMemory
    {
        private readonly ILogger<SetChatMemory> _logger;

        public SetChatMemory(ILogger<SetChatMemory> log)
        {
            _logger = log;
        }

        [FunctionName("SetChatMemory")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "The memory to be prepended at the beginning of every prompt.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-memories",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            string memory = await new StreamReader(req.Body).ReadToEndAsync();

            ChatMemory chatMemory = await ChatMemory.GetChatMemory(client, user);

            if (chatMemory != null)
            {
                chatMemory.memory = memory;
                await chatMemory.Update(client);
            }
            else
            {
                chatMemory = new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    memory = memory
                };

                await chatMemory.Write(client);
            }

            return new OkResult();
        }
    }
}

