using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class CarpenterUser
    {
        /// <summary>
        /// Hexadecimal string
        /// </summary>
        public string userId;

        public static CarpenterUser GetCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            string userId = "00000000000000000000000000000000";

            if (claimsPrincipal != null)
            {
                Claim nameIdentifierClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

                if (nameIdentifierClaim != null)
                {
                    userId = nameIdentifierClaim.Value;
                }
            }

            return new() { userId = userId };
        }

        public async Task DeleteAllUserData(CosmosClient client)
        {
            var chatMemory = await ChatMemory.GetChatMemory(client, this);
            chatMemory.memory = string.Empty;
            await chatMemory.Update(client);

            var chatSummarizationPrompt = await ChatSummarizationPrompt.GetChatSummarizationPrompt(client, this);
            chatSummarizationPrompt.prompt = string.Empty;
            await chatSummarizationPrompt.Update(client);

            var chatMessages = await ChatMessage.GetChatMessages(client, this);

            foreach(var chatMessage in chatMessages)
            {
                await ChatMessage.Delete(client, chatMessage.id);
            }

            var chatSummaries = await ChatSummary.GetChatSummaries(client, this);

            foreach(var chatSummary in chatSummaries)
            {
                await ChatSummary.Delete(client, chatSummary.id);
            }

            var messageGenerations = await MessageGeneration.GetMessageGenerations(client, this);

            foreach(var messageGeneration in messageGenerations)
            {
                await MessageGeneration.Delete(client, messageGeneration.id);
            }
        }
    }
}
