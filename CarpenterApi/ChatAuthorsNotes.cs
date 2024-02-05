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
    public class ChatAuthorsNotes
    {
        private readonly ILogger<ChatAuthorsNotes> _logger;

        public ChatAuthorsNotes(ILogger<ChatAuthorsNotes> log)
        {
            _logger = log;
        }

        [FunctionName("GetChatAuthorsNote")]
        [OpenApiOperation(operationId: "GetChatAuthorsNote")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetChatAuthorsNote(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat-authors-note")] HttpRequest req,
#pragma warning restore IDE0060 // Remove unused parameter
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-authors-notes",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            ChatAuthorsNote authorsNote = await ChatAuthorsNote.GetChatAuthorsNote(client, user);

            if (authorsNote != null)
            {
                return new OkObjectResult(authorsNote.authorsNote);
            }
            else
            {
                return new OkObjectResult("");
            }
        }

        [FunctionName("SetChatAuthorsNote")]
        [OpenApiOperation(operationId: "SetChatAuthorsNote")]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "The memory to be prepended at the beginning of every prompt.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> SetChatAuthorsNote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", containerName: "chat-authors-notes",
                Connection = "CosmosDbConnectionString"
                )] CosmosClient client,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            CarpenterUser user = CarpenterUser.GetCurrentUser(claimsPrincipal);

            string authorsNote = await new StreamReader(req.Body).ReadToEndAsync();

            ChatAuthorsNote chatAuthorsNote = await ChatAuthorsNote.GetChatAuthorsNote(client, user);

            if (chatAuthorsNote != null)
            {
                chatAuthorsNote.authorsNote = authorsNote;
                await chatAuthorsNote.Update(client);
            }
            else
            {
                chatAuthorsNote = new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    authorsNote = authorsNote
                };

                await chatAuthorsNote.Write(client);
            }

            return new OkResult();
        }
    }
}

