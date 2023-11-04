using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiktoken;

namespace CarpenterApi.Models
{
    internal class ChatSummaryLog
    {
        public const string MemoryType = "memory";
        public const string ChatMessagesType = "chatMessages";
        public const string InstructionType = "instruction";
        public const string ChatSummaryType = "chatSummary";

        public string type;
        public Guid? chatSummaryId;
        public string text;

        public static async Task<IList<ChatSummaryLog>> GetChatSummaryLogs(CosmosClient client, CarpenterUser user)
        {
            List<ChatSummaryLog> logs = new();
            
            ChatMemory chatMemory = await ChatMemory.GetChatMemory(client, user);
            var chatMessages = await ChatMessage.GetChatMessages(client, user);
            int maxTokens = MessageGeneration.MaxInputLength;

            logs.Add(new()
            {
                type = MemoryType,
                text = chatMemory.memory
            });

            logs.AddRange(await NextChatMessageGeneration(client, user, chatMemory, chatMessages, ChatMessage.AISender, maxTokens));

            return logs;
        }

        public static async Task<IList<ChatSummaryLog>> NextChatMessageGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, IList<ChatMessage> chatMessages, string sender, int maxTokens)
        {
            List<ChatSummaryLog> logs = new();
            
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                prompt += Environment.NewLine + chatMessages[i].ToPrompt();
            }

            ChatMessage aiPrompt = new()
            {
                timestamp = DateTime.UtcNow,
                sender = sender,
                message = ""
            };

            prompt += Environment.NewLine + aiPrompt.ToPrompt();
            ChatSummary chatSummary = null;

            int tokenCount = encoding.CountTokens(prompt);

            ChatSummarizationPrompt chatSummarizationPrompt = await ChatSummarizationPrompt.GetChatSummarizationPrompt(client, user);

            while (tokenCount > maxTokens)
            {
                (ChatSummary, int, MessageGeneration) summary = await PromptGeneration.GetChatSummaryOrMessageGeneration(client, user, chatMemory, chatSummary, chatMessages, chatSummarizationPrompt, MessageGeneration.SummarizationInputLength);

                if (summary.Item3 != null)
                {
                    // we don't have the next summary we need, so return what we've got
                    return logs;
                }
                else
                {
                    chatSummary = summary.Item1;
                    prompt = chatMemory.memory;
                    prompt += Environment.NewLine + summary.Item1.ToPrompt();

                    string previousChatMessages = "";

                    // remove the chat messages that have been replaced by the summary
                    for (int i = 0; i < summary.Item2; i++)
                    {
                        if(i > 0)
                        {
                            previousChatMessages += Environment.NewLine;
                        }

                        previousChatMessages += chatMessages[0].ToPrompt();
                        chatMessages.RemoveAt(0);
                    }

                    logs.Add(new()
                    {
                        type = ChatMessagesType,
                        text = previousChatMessages
                    });

                    logs.Add(new()
                    {
                        type = InstructionType,
                        text = chatSummarizationPrompt.prompt
                    });

                    logs.Add(new()
                    {
                        type = ChatSummaryType,
                        chatSummaryId = chatSummary.id,
                        text = chatSummary.summary
                    });

                    for (int i = 0; i < chatMessages.Count; i++)
                    {
                        prompt += Environment.NewLine + chatMessages[i].ToPrompt();
                    }

                    prompt += Environment.NewLine + aiPrompt.ToPrompt();

                    tokenCount = encoding.CountTokens(prompt);
                }
            }

            return logs;
        }
    }
}
