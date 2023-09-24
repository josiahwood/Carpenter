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
    public class SetChatSummarizationPrompt
    {
        private readonly ILogger<SetChatSummarizationPrompt> _logger;

        public SetChatSummarizationPrompt(ILogger<SetChatSummarizationPrompt> log)
        {
            _logger = log;
        }

        [FunctionName("SetChatSummarizationPrompt")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "The prompt to be appended to request chat summarization.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-summarization-prompts",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            string prompt = await new StreamReader(req.Body).ReadToEndAsync();

            ChatSummarizationPrompt chatSummarizationPrompt = await ChatSummarizationPrompt.GetChatSummarizationPrompt(client, user);

            if (chatSummarizationPrompt != null)
            {
                chatSummarizationPrompt.prompt = prompt;
                await chatSummarizationPrompt.Update(client);
            }
            else
            {
                chatSummarizationPrompt = new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    prompt = prompt
                };

                await chatSummarizationPrompt.Write(client);
            }

            return new OkResult();
        }
    }
}

