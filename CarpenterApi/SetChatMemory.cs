using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                ConnectionStringSetting = "CosmosDbConnectionString"
                )]IAsyncCollector<dynamic> documentsOut,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            Claim nameIdentifierClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            string userId = nameIdentifierClaim.Value;

            string memory = await new StreamReader(req.Body).ReadToEndAsync();
            
            await documentsOut.AddAsync(new
            {
                userId,
                memory
            });

            return new OkResult();
        }
    }
}

