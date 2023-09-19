using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
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
    public class ImportChatLog
    {
        private readonly ILogger<ImportChatLog> _logger;

        public ImportChatLog(ILogger<ImportChatLog> log)
        {
            _logger = log;
        }

        [FunctionName("ImportChatLog")]
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
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string[] lines = requestBody.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            Regex regex = new("^(?<timestamp>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2} [+-]\\d{4}) (?<sender>\\w+):(?<message>.*)$");

            List<ChatMessage> chatMessages = new();
            
            foreach (string line in lines)
            {
                Match match = regex.Match(line);

                if (match.Success)
                {
                    ChatMessage chatMessage = new()
                    {
                        id = Guid.NewGuid(),
                        userId = user.userId,
                        timestamp = DateTime.ParseExact(match.Groups["timestamp"].Value, "yyyy-MM-dd HH:mm:ss zzz", null),
                        sender = match.Groups["sender"].Value,
                        message = match.Groups["message"].Value.Trim()
                    };

                    chatMessages.Add(chatMessage);
                }
                else
                {
                    chatMessages[chatMessages.Count - 1].message += Environment.NewLine + line;
                }
            }

            foreach(ChatMessage chatMessage in chatMessages)
            {
                await chatMessage.Write(client);
            }

            return new OkResult();
        }
    }
}

