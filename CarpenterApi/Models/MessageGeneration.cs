using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using StableHordeApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class MessageGeneration
    {
        internal class PurposeData
        {
            public DateTime timestamp;
            public string instruction;
        }

        public const string ApiKey = "***REMOVED***";

        public const string AIChatMessagePurpose = "aiChatMessage";
        public const string UserChatMessagePurpose = "userChatMessage";
        public const string ChatInstructionPurpose = "chatInstruction";
        public const string ChatSummaryPurpose = "chatSummary";

        public const string NoneStatus = "none";
        public const string SummarizingStatus = "summarizing";
        public const string PendingStatus = "pending";
        public const string GeneratingStatus = "generating";
        public const string DoneStatus = "done";
        public const string ErrorStatus = "error";

        public const string ImEndToken = "<|im_end|>";

        public const int MaxInputLength = 4096;
        public const int MaxOutputLength = 256;
        public const int SummarizationInputLength = 2048;
        public const int SummarizationOutputLength = 256;

        // Identifiers
        public Guid id;
        
        /// <summary>
        /// Used for alternative messages or parent summaries
        /// </summary>
        public Guid? parentId;
        
        public string userId;

        public DateTime? startTime;

        // Inputs
        public string[] models;
        public string worker;
        public int maxInputLength;
        public int maxOutputLength;
        public string prompt;
        public string purpose;
        public PurposeData purposeData;
        public string nextPurpose;
        public PurposeData nextPurposeData;

        // Outputs
        public string stableHordeId;
        public string status;
        public string model;
        public string generatedOutput;
        public RequestStatusKobold koboldStatus;

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

        public static async Task<IEnumerable<MessageGeneration>> GenerateChatMessageAlternatives(CosmosClient client, CarpenterUser user, string sender)
        {
            ChatMemory chatMemory = await ChatMemory.GetChatMemory(client, user);
            var chatMessages = await ChatMessage.GetChatMessages(client, user);

            MessageGeneration messageGeneration = await PromptGeneration.NextChatMessageGeneration(client, user, chatMemory, chatMessages, sender, MaxInputLength);

            List<MessageGeneration> messageGenerations = new();

            switch (messageGeneration.purpose)
            {
                case AIChatMessagePurpose:
                case UserChatMessagePurpose:
                    var models = await ModelInfo.PickModels(client, user, MaxInputLength, MaxOutputLength);

                    MessageGeneration messageGeneration1 = new()
                    {
                        id = Guid.NewGuid(),
                        parentId = messageGeneration.id,
                        maxInputLength = messageGeneration.maxInputLength,
                        maxOutputLength = messageGeneration.maxOutputLength,
                        models = models.Item1,
                        prompt = messageGeneration.prompt,
                        purpose = messageGeneration.purpose,
                        purposeData = messageGeneration.purposeData,
                        status = messageGeneration.status,
                        userId = user.userId,
                    };

                    var newMessageGeneration1 = await StartGeneration(client, messageGeneration1);
                    messageGenerations.Add(newMessageGeneration1);

                    MessageGeneration messageGeneration2 = new()
                    {
                        id = Guid.NewGuid(),
                        parentId = messageGeneration.id,
                        maxInputLength = messageGeneration.maxInputLength,
                        maxOutputLength = messageGeneration.maxOutputLength,
                        models = models.Item2,
                        prompt = messageGeneration.prompt,
                        purpose = messageGeneration.purpose,
                        purposeData = messageGeneration.purposeData,
                        status = messageGeneration.status,
                        userId = user.userId,
                    };

                    var newMessageGeneration2 = await StartGeneration(client, messageGeneration2);
                    messageGenerations.Add(newMessageGeneration2);

                    break;
                case ChatSummaryPurpose:
                    messageGenerations.Add(await StartGeneration(client, messageGeneration));
                    break;
            }

            return messageGenerations;
        }

        public static async Task<MessageGeneration> StartGeneration(CosmosClient client, MessageGeneration messageGeneration)
        {
            HttpClient httpClient = new();
            StableHordeApi.Client apiClient = new(httpClient)
            {
                BaseUrl = "https://stablehorde.net/api"
            };

            GenerationInputKobold payload = new()
            {
                Prompt = messageGeneration.prompt,
                Models = messageGeneration.models,
                Params = new ModelGenerationInputKobold
                {
                    N = 1,
                    Max_context_length = messageGeneration.maxInputLength,
                    Max_length = messageGeneration.maxOutputLength
                }
            };

            var generateResult = await apiClient.Post_text_async_generateAsync(ApiKey, null, payload, null);
            messageGeneration.stableHordeId = generateResult.Id;
            messageGeneration.status = PendingStatus;
            messageGeneration.startTime = DateTime.UtcNow;
            await messageGeneration.Create(client);

            return messageGeneration;
        }

        //public static async Task<MessageGeneration> StartGeneration(CosmosClient client, CarpenterUser user, string prompt, string purpose, PurposeData purposeData, string nextPurpose, object nextPurposeData)
        //{
        //    MessageGeneration messageGeneration = new()
        //    {
        //        id = Guid.NewGuid(),
        //        userId = user.userId,
        //        maxInputLength = MaxInputLength,
        //        maxOutputLength = MaxOutputLength,
        //        prompt = prompt,
        //        purpose = purpose,
        //        nextPurpose = nextPurpose,
        //        nextPurposeData = nextPurposeData
        //    };

        //    return await StartGeneration(client, messageGeneration);
        //}

        public async Task UpdateStatus(CosmosClient client, CarpenterUser user)
        {
            if (status != DoneStatus && status != ErrorStatus)
            {
                HttpClient httpClient = new();
                StableHordeApi.Client apiClient = new(httpClient)
                {
                    BaseUrl = "https://stablehorde.net/api"
                };

                RequestStatusKobold statusResult;

                try
                {
                    statusResult = await apiClient.Get_text_async_statusAsync(null, null, stableHordeId);
                    koboldStatus = statusResult;
                    await Update(client);
                }
                catch (ApiException)
                {
                    status = ErrorStatus;
                    await ModelInfo.DecrementModelInfo(client, user, model);

                    await Update(client);
                    return;
                }

                if (statusResult.Done == true)
                {
                    var generation = statusResult.Generations.FirstOrDefault();

                    if (generation != null)
                    {
                        generatedOutput = TrimGeneratedOutput(generation.Text);
                        model = generation.Model;
                        worker = generation.Worker_name;
                        status = DoneStatus;

                        switch (purpose)
                        {
                            case AIChatMessagePurpose:
                                if (purposeData != null)
                                {
                                    ChatMessage chatMessage = new()
                                    {
                                        id = Guid.NewGuid(),
                                        userId = userId,
                                        timestamp = purposeData.timestamp,
                                        sender = ChatMessage.AISender,
                                        message = TrimMessage(generatedOutput),
                                        messageGenerationId = id,
                                        alternateGroupId = parentId
                                    };

                                    await chatMessage.Write(client);
                                }

                                break;
                            case UserChatMessagePurpose:
                                if (purposeData != null)
                                {
                                    ChatMessage chatMessage = new()
                                    {
                                        id = Guid.NewGuid(),
                                        userId = userId,
                                        timestamp = purposeData.timestamp,
                                        sender = ChatMessage.UserSender,
                                        message = TrimMessage(generatedOutput),
                                        messageGenerationId = id,
                                        alternateGroupId = parentId
                                    };

                                    await chatMessage.Write(client);
                                }

                                break;
                            case ChatInstructionPurpose:
                                // Do nothing, the UI will just get the response directly from the MessageGeneration object
                                break;
                            case ChatSummaryPurpose:
                                ChatSummary chatSummary = new()
                                {
                                    id = Guid.NewGuid(),
                                    userId = userId,
                                    promptHash = PromptGeneration.GetHash(prompt),
                                    summary = generatedOutput,
                                    messageGenerationId = id
                                };

                                await chatSummary.Write(client);
                                break;
                        }

                        ChatMemory chatMemory;
                        IList<ChatMessage> chatMessages;
                        MessageGeneration messageGeneration;

                        switch (nextPurpose)
                        {
                            case AIChatMessagePurpose:
                                await GenerateChatMessageAlternatives(client, user, ChatMessage.AISender);

                                break;
                            case UserChatMessagePurpose:
                                await GenerateChatMessageAlternatives(client, user, ChatMessage.UserSender);

                                break;
                            case ChatInstructionPurpose:
                                chatMemory = await ChatMemory.GetChatMemory(client, user);
                                chatMessages = await ChatMessage.GetChatMessages(client, user);
                                string chatInstruction = nextPurposeData.instruction;

                                messageGeneration = await PromptGeneration.NextChatInstructionGeneration(client, user, chatMemory, chatMessages, chatInstruction, MaxInputLength);

                                await StartGeneration(client, messageGeneration);
                                break;
                        }
                    }
                    else
                    {
                        status = ErrorStatus;
                        await ModelInfo.DecrementModelInfo(client, user, model);
                    }

                    await Update(client);
                }
                else
                {
                    // result is not done

                    if (startTime.HasValue)
                    {
                        if (DateTime.UtcNow - startTime > TimeSpan.FromMinutes(5))
                        {
                            status = ErrorStatus;
                            await ModelInfo.DecrementModelInfo(client, user, model);

                            await Update(client);
                        }
                    }
                }
            }
        }

        public static async Task<IEnumerable<MessageGeneration>> GetMessageGenerations(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm").WithParameter("@searchterm", user.userId);

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

        public static async Task Delete(CosmosClient client, Guid id)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            await container.DeleteItemAsync<MessageGeneration>(id.ToString(), new PartitionKey(id.ToString()));
        }

        public static async Task<IEnumerable<MessageGeneration>> GetNotDoneMessageGenerations(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @searchterm AND c.status != 'done' AND c.status != 'error'").WithParameter("@searchterm", user.userId);

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

        public static async Task<MessageGeneration> GetMessageGeneration(CosmosClient client, CarpenterUser user, Guid id)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId AND c.id = @id")
                .WithParameter("@userId", user.userId)
                .WithParameter("@id", id);

            using var iterator = container.GetItemQueryIterator<MessageGeneration>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var messageGeneration = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (messageGeneration != null)
                {
                    return messageGeneration;
                }
            }

            return null;
        }

        public static async Task<string> GetLatestChatInstructionResponse(CosmosClient client, CarpenterUser user)
        {
            Container container = client.GetDatabase("carpenter-dev").GetContainer("message-generations");
            QueryDefinition queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.userId = @userId AND c.purpose = @purpose ORDER BY c._ts DESC")
                .WithParameter("@userId", user.userId)
                .WithParameter("@purpose", ChatInstructionPurpose);

            using var iterator = container.GetItemQueryIterator<MessageGeneration>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var messageGeneration = (await iterator.ReadNextAsync()).FirstOrDefault();

                if (messageGeneration != null)
                {
                    return messageGeneration.generatedOutput;
                }
            }

            return null;
        }

        private static string TrimGeneratedOutput(string message)
        {
            int imEndTokenIndex = message.IndexOf(ImEndToken);

            if (imEndTokenIndex >= 0)
            {
                message = message[..imEndTokenIndex];
            }

            message = message.Trim();
            message = message.ReplaceLineEndings();

            return message;
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
