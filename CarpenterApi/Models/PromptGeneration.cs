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
        private const string ChatSummarizationInstruction = "Summarize this chat session.";

        public static string ToPrompt(this ChatMessage chatMessage)
        {
            return $"{chatMessage.timestamp:yyyy-MM-dd HH:mm:ss zzz} {chatMessage.sender}: {chatMessage.message}";
        }

        public static string ToPrompt(this ChatSummary chatSummary)
        {
            return $"[Summary from {chatSummary.startTimestamp:yyyy-MM-dd HH:mm:ss zzz} to {chatSummary.endTimestamp:yyyy-MM-dd HH:mm:ss zzz}:{chatSummary.summary}]";
        }

        public static uint GetHash(string value)
        {
            System.IO.Hashing.Crc32 crc32 = new();
            crc32.Append(System.Text.Encoding.UTF8.GetBytes(value));
            byte[] hashBytes = crc32.GetCurrentHash();
            return BitConverter.ToUInt32(hashBytes);
        }

        public static async Task<string> GenerateNextChatPrompt(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, ChatSummary chatSummary, IList<ChatMessage> chatMessages, string instruction, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            if(chatSummary != null)
            {
                if (!prompt.IsNullOrWhiteSpace())
                {
                    prompt += Environment.NewLine;
                }

                prompt += chatSummary.ToPrompt();
            }

            string promptInstruction = prompt;

            if (!instruction.IsNullOrWhiteSpace())
            {
                promptInstruction += Environment.NewLine + "### Instruction:" + instruction + Environment.NewLine + "### Response:";
            }

            int lastIndexIncluded = 0;
            bool summarizationNeeded = false;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                string tempPrompt = prompt + Environment.NewLine + chatMessages[i].ToPrompt();
                string tempPromptInstruction = tempPrompt;
                
                if (!instruction.IsNullOrWhiteSpace())
                {
                    tempPromptInstruction += Environment.NewLine + "### Instruction:" + instruction + Environment.NewLine + "### Response:";
                }

                int tokenCount = encoding.CountTokens(tempPromptInstruction);

                if (tokenCount > maxTokens)
                {
                    summarizationNeeded = true;
                    break;
                }
                else
                {
                    lastIndexIncluded = i;
                    prompt = tempPrompt;
                    promptInstruction = tempPromptInstruction;
                }
            }

            if (summarizationNeeded)
            {
                if (instruction == ChatSummarizationInstruction && maxTokens == 512)
                {
                    // this is a summarization prompt already, see if this has already been done

                    uint hash = GetHash(promptInstruction);

                    ChatSummary nextChatSummary = await ChatSummary.GetChatSummaryFromHash(client, user, hash);

                    if (nextChatSummary != null)
                    {
                        // this summary has already been done before, use it and recurse
                        List<ChatMessage> nextChatMessages = new();

                        for (int i = lastIndexIncluded + 1; i < chatMessages.Count; i++)
                        {
                            nextChatMessages.Add(chatMessages[i]);
                        }

                        return await GenerateNextChatPrompt(client, user, chatMemory, nextChatSummary, nextChatMessages, instruction, maxTokens);
                    }
                    else
                    {
                        // this is already a summary, but it's not in the database, so this is the next prompt needed
                        return promptInstruction;
                    }
                }
                else
                {
                    // summarization is needed, but this isn't a summarization prompt
                    
                    return await GenerateNextChatPrompt(client, user, chatMemory, chatSummary, chatMessages, ChatSummarizationInstruction, 512);
                }
            }
            else
            {
                // no summarization needed
                return promptInstruction;
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
