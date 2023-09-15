using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using StableHordeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class MessageGeneration
    {
        public const string AIChatMessagePurpose = "aiChatMessage";
        public const string ChatSummaryPurpose = "chatSummary";

        public const string NoneStatus = "none";
        public const string SummarizingStatus = "summarizing";
        public const string PendingStatus = "pending";
        public const string GeneratingStatus = "generating";
        public const string DoneStatus = "done";
        public const string ErrorStatus = "error";

        public const int MaxInputLength = 1024;
        public const int MaxOutputLength = 256;

        // Identifiers
        public Guid id;
        public string userId;

        // Inputs
        public string model;
        public string worker;
        public int maxInputLength;
        public int maxOutputLength;
        public string prompt;
        public DateTime timestamp;
        public string purpose;

        // Outputs
        public string stableHordeId;
        public string status;
        public string generatedOutput;

        public async Task Create(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            await container.CreateItemAsync(this);
        }

        public async Task Update(CosmosClient client)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            await container.ReplaceItemAsync(this, id.ToString());
        }

        public static async Task<MessageGeneration> StartGeneration(CosmosClient client, DateTime timestamp, CarpenterUser user, string prompt, string purpose)
        {
            MessageGeneration messageGeneration = new()
            {
                id = Guid.NewGuid(),
                userId = user.userId,
                maxInputLength = MaxInputLength,
                maxOutputLength = MaxOutputLength,
                prompt = prompt,
                timestamp = timestamp,
                purpose = purpose
            };
            
            HttpClient httpClient = new();
            StableHordeApi.Client apiClient = new(httpClient)
            {
                BaseUrl = "https://stablehorde.net/api"
            };

            GenerationInputKobold payload = new()
            {
                Prompt = messageGeneration.prompt,
                Params = new ModelGenerationInputKobold
                {
                    N = 1,
                    Max_context_length = messageGeneration.maxInputLength,
                    Max_length = messageGeneration.maxOutputLength
                }
            };

            var generateResult = await apiClient.Post_text_async_generateAsync("***REMOVED***", null, payload, null);
            messageGeneration.stableHordeId = generateResult.Id;
            messageGeneration.status = PendingStatus;
            await messageGeneration.Create(client);

            return messageGeneration;
        }

        public async Task UpdateStatus(CosmosClient client)
        {
            if (status != DoneStatus)
            {
                HttpClient httpClient = new();
                StableHordeApi.Client apiClient = new(httpClient)
                {
                    BaseUrl = "https://stablehorde.net/api"
                };

                var statusResult = await apiClient.Get_text_async_statusAsync(null, null, stableHordeId);

                if (statusResult.Done == true)
                {
                    var generation = statusResult.Generations.FirstOrDefault();

                    if (generation != null)
                    {
                        generatedOutput = generation.Text;
                        model = generation.Model;
                        worker = generation.Worker_name;
                        status = DoneStatus;

                        if (purpose == AIChatMessagePurpose)
                        {
                            ChatMessage chatMessage = new()
                            {
                                id = Guid.NewGuid(),
                                userId = userId,
                                timestamp = timestamp,
                                sender = ChatMessage.AISender,
                                message = TrimMessage(generatedOutput),
                                messageGenerationId = id
                            };

                            await chatMessage.Write(client);
                        }
                    }
                    else
                    {
                        status = ErrorStatus;
                    }

                    await Update(client);
                }
            }
        }

        public static async Task<IEnumerable<MessageGeneration>> GetNotDoneMessageGenerations(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm AND c.status != 'done'").WithParameter("@searchterm", user.userId);

            List<MessageGeneration> messageGenerations = new();

            using (var iterator = container.GetItemQueryIterator<MessageGeneration>(queryDefinition))
            {
                while (iterator.HasMoreResults)
                {
                    var readNext = await iterator.ReadNextAsync();

                    foreach (var messageGeneration in readNext)
                    {
                        messageGenerations.Add(messageGeneration);
                    }
                }
            }

            return messageGenerations;
        }

        private static string TrimMessage(string message)
        {
            var lines = message.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            string trimmed = "";

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, @"^\d{4}"))
                {
                    break;
                }
                else
                {
                    if(!trimmed.IsNullOrWhiteSpace())
                    {
                        trimmed += Environment.NewLine;
                    }
                    
                    trimmed += line;
                }
            }

            return trimmed;
        }
    }
}
