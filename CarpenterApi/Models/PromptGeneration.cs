using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiktoken;

namespace CarpenterApi.Models
{
    internal static class PromptGeneration
    {
        private const string ChatSummarizationPrompt = "### Instruction:Summarize this chat session.\n###Response:";

        public static string ToPrompt(this ChatMessage chatMessage)
        {
            return $"{chatMessage.timestamp:yyyy-MM-dd HH:mm:ss zzz} {chatMessage.sender}: {chatMessage.message}";
        }

        public static string ToPrompt(this ChatSummary chatSummary)
        {
            return $"[Summary:{chatSummary.summary}]";
        }

        public static uint GetHash(string value)
        {
            System.IO.Hashing.Crc32 crc32 = new();
            crc32.Append(System.Text.Encoding.UTF8.GetBytes(value));
            byte[] hashBytes = crc32.GetCurrentHash();
            return BitConverter.ToUInt32(hashBytes);
        }

        public static async Task<MessageGeneration> NextAIChatMessageGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, IList<ChatMessage> chatMessages, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                prompt += Environment.NewLine + chatMessages[i].ToPrompt();
            }

            ChatMessage aiPrompt = new()
            {
                timestamp = DateTime.UtcNow,
                sender = ChatMessage.AISender,
                message = ""
            };

            prompt += Environment.NewLine + aiPrompt.ToPrompt();
            ChatSummary chatSummary = null;

            int tokenCount = encoding.CountTokens(prompt);
            
            while(tokenCount > maxTokens)
            {
                (ChatSummary, int, MessageGeneration) summary = await GetChatSummaryOrMessageGeneration(client, user, chatMemory, chatSummary, chatMessages, MessageGeneration.SummarizationInputLength);

                if (summary.Item3 != null)
                {
                    // we don't already have the next summary we need, so generate it
                    return summary.Item3;
                }
                else
                {
                    chatSummary = summary.Item1;
                    prompt = chatMemory.memory;
                    prompt += Environment.NewLine + summary.Item1.ToPrompt();

                    // remove the chat messages that have been replaced by the summary
                    for (int i = 0; i < summary.Item2; i++)
                    {
                        chatMessages.RemoveAt(0);
                    }

                    for(int i = 0; i < chatMessages.Count; i++)
                    {
                        prompt += Environment.NewLine + chatMessages[i].ToPrompt();
                    }

                    prompt += Environment.NewLine + aiPrompt.ToPrompt();

                    tokenCount = encoding.CountTokens(prompt);
                }
            }

            return new()
            {
                id = Guid.NewGuid(),
                userId = user.userId,
                timestamp = aiPrompt.timestamp,
                prompt = prompt,
                maxInputLength = maxTokens,
                maxOutputLength = MessageGeneration.MaxOutputLength,
                purpose = MessageGeneration.AIChatMessagePurpose,
                status = MessageGeneration.NoneStatus
            };
        }

        private static (string, int) GetChatSummaryPrompt(ChatMemory chatMemory, ChatSummary chatSummary, IList<ChatMessage> chatMessages, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            if (chatSummary != null)
            {
                prompt += Environment.NewLine + chatSummary.ToPrompt();
            }

            string promptInstruction = prompt + Environment.NewLine + ChatSummarizationPrompt;

            int chatMessagesIncluded = 0;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                string tempPrompt = prompt + Environment.NewLine + chatMessages[i].ToPrompt();
                string tempPromptInstruction = tempPrompt + Environment.NewLine + ChatSummarizationPrompt;

                int tokenCount = encoding.CountTokens(tempPromptInstruction);

                if (tokenCount > maxTokens)
                {
                    break;
                }
                else
                {
                    chatMessagesIncluded = i + 1;
                    prompt = tempPrompt;
                    promptInstruction = tempPromptInstruction;
                }
            }

            return (promptInstruction, chatMessagesIncluded);
        }

        public static async Task<(ChatSummary,int,MessageGeneration)> GetChatSummaryOrMessageGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, ChatSummary chatSummary, IList<ChatMessage> chatMessages, int maxTokens)
        {
            (string,int) value = GetChatSummaryPrompt(chatMemory, chatSummary, chatMessages, maxTokens);
            string prompt = value.Item1;
            int chatMessagesIncluded = value.Item2;

            uint promptHash = GetHash(prompt);
            ChatSummary nextChatSummary = await ChatSummary.GetChatSummaryFromHash(client, user, promptHash);

            if(nextChatSummary != null)
            {
                return (nextChatSummary, chatMessagesIncluded, null);
            }
            else
            {
                return (null, 0, new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    maxInputLength = maxTokens,
                    maxOutputLength = MessageGeneration.SummarizationOutputLength,
                    prompt = prompt,
                    purpose = MessageGeneration.ChatSummaryPurpose,
                    status = MessageGeneration.NoneStatus
                });
            }
        }

        public static string GeneratePromptTruncateHistory(ChatMemory chatMemory, IList<ChatMessage> chatMessages, string instruction, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string tempMessages = chatMessages[chatMessages.Count - 1].ToPrompt();
            string prompt = string.Empty;

            for (int i = chatMessages.Count - 2; i >= 0; i--)
            {
                string newTempMessages = chatMessages[i].ToPrompt() + Environment.NewLine + tempMessages;
                string tempPrompt = chatMemory.memory + Environment.NewLine + newTempMessages;

                if(!instruction.IsNullOrWhiteSpace())
                {
                    tempPrompt += Environment.NewLine + "### Instruction:" + instruction + Environment.NewLine + "### Response:";
                }

                int tokenCount = encoding.CountTokens(newTempMessages);

                if (tokenCount > maxTokens)
                {
                    break;
                }
                else
                {
                    tempMessages = newTempMessages;
                    prompt = tempPrompt;
                }
            }

            return prompt;
        }
    }
}
