using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiRequestBody(contentType: "text/plain", bodyType: typeof(string), Description = "The memory to be prepended at the beginning of every prompt.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "carpenter-dev", collectionName: "chat-memories",
                ConnectionStringSetting = "CosmosDbConnectionString", CreateIfNotExists = true
                )] DocumentClient documentClient,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string userId = "00000000000000000000000000000000";

            if (claimsPrincipal != null)
            {
                Claim nameIdentifierClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

                if (nameIdentifierClaim != null)
                {
                    userId = nameIdentifierClaim.Value;
                }
            }

            string memory = await new StreamReader(req.Body).ReadToEndAsync();

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("carpenter-dev", "chat-memories");

            string query = $"SELECT * FROM c WHERE c.userId = \"{userId}\"";
            var queryObj = documentClient.CreateDocumentQuery(collectionUri, query);

            dynamic? result = queryObj.FirstOrDefault();

            if (result != null)
            {
                dynamic document = new
                {
                    id = result.id,
                    userId,
                    memory
                };

                Uri documentUri = UriFactory.CreateDocumentUri("carpenter-dev", "chat-memories", result.id);
                await documentClient.UpsertDocumentAsync(documentUri, document);
            }
            else
            {
                dynamic document = new
                {
                    id = Guid.NewGuid(),
                    userId,
                    memory
                };

                await documentClient.CreateDocumentAsync(collectionUri, document);
            }

            return new OkResult();
        }
    }
}

