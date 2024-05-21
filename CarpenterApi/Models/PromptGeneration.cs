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
        //private const string ChatSummarizationPrompt = "### Instruction:Summarize this chat session from the perspective of the AI, who is a psychotherapist seeking to perform Mindfulness-Based Cognitive Therapy with the user.\n### Response:";

        public static string ToPrompt(this ChatAuthorsNote chatAuthorsNote)
        {
            if(chatAuthorsNote != null && !string.IsNullOrWhiteSpace(chatAuthorsNote.authorsNote))
            {
                return $"[Author's note:{chatAuthorsNote.authorsNote}]";
            }
            else
            {
                return null;
            }
        }

        public static string ToPrompt(this ChatMessage chatMessage)
        {
            //DateTime utcTime = chatMessage.timestamp.ToUniversalTime();

            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            //DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, cstZone);

            //return $"{cstTime:yyyy-MM-dd HH:mm:ss} {chatMessage.sender}: {chatMessage.message}";
            return $"{chatMessage.sender}: {chatMessage.message}";
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

        public static async Task<MessageGeneration> NextChatMessageGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, ChatAuthorsNote chatAuthorsNote, IList<ChatMessage> chatMessages, string sender, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            string authorsNote = chatAuthorsNote.ToPrompt();

            for (int i = 0; i < chatMessages.Count; i++)
            {
                if(i == chatMessages.Count - 2)
                {
                    if(authorsNote != null)
                    {
                        prompt += Environment.NewLine + authorsNote;
                    }
                }
                
                prompt += Environment.NewLine + chatMessages[i].ToPrompt();
            }

            ChatMessage senderPrompt = new()
            {
                timestamp = DateTime.UtcNow,
                sender = sender,
                message = ""
            };

            prompt += Environment.NewLine + senderPrompt.ToPrompt();
            ChatSummary chatSummary = null;

            int tokenCount = encoding.CountTokens(prompt);

            ChatSummarizationPrompt chatSummarizationPrompt = await ChatSummarizationPrompt.GetChatSummarizationPrompt(client, user);

            while (tokenCount > maxTokens)
            {
                (ChatSummary, int, MessageGeneration) summary = await GetChatSummaryOrMessageGeneration(client, user, chatMemory, chatSummary, chatMessages, chatSummarizationPrompt, MessageGeneration.SummarizationInputLength);

                if (summary.Item3 != null)
                {
                    // we don't already have the next summary we need, so generate it
                    
                    switch(sender)
                    {
                        case ChatMessage.AISender:
                            summary.Item3.nextPurpose = MessageGeneration.AIChatMessagePurpose;
                            break;
                        case ChatMessage.UserSender:
                            summary.Item3.nextPurpose = MessageGeneration.UserChatMessagePurpose;
                            break;
                    }
                    
                    summary.Item3.nextPurposeData = new MessageGeneration.PurposeData
                    {
                        timestamp = senderPrompt.timestamp
                    };
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
                        if(i == chatMessages.Count - 2)
                        {
                            if(authorsNote != null)
                            {
                                prompt += Environment.NewLine + authorsNote;
                            }
                        }
                        
                        prompt += Environment.NewLine + chatMessages[i].ToPrompt();
                    }

                    prompt += Environment.NewLine + senderPrompt.ToPrompt();

                    tokenCount = encoding.CountTokens(prompt);
                }
            }

            return sender switch
            {
                ChatMessage.AISender => new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    prompt = prompt,
                    maxInputLength = maxTokens,
                    maxOutputLength = MessageGeneration.MaxOutputLength,
                    purpose = MessageGeneration.AIChatMessagePurpose,
                    purposeData = new MessageGeneration.PurposeData
                    {
                        timestamp = senderPrompt.timestamp
                    },
                    status = MessageGeneration.NoneStatus
                },
                ChatMessage.UserSender => new()
                {
                    id = Guid.NewGuid(),
                    userId = user.userId,
                    prompt = prompt,
                    maxInputLength = maxTokens,
                    maxOutputLength = MessageGeneration.MaxOutputLength,
                    purpose = MessageGeneration.UserChatMessagePurpose,
                    purposeData = new MessageGeneration.PurposeData
                    {
                        timestamp = senderPrompt.timestamp
                    },
                    status = MessageGeneration.NoneStatus
                },
                _ => throw new InvalidOperationException($"Unknown sender \"{sender}\"."),
            };
        }

        public static async Task<MessageGeneration> NextChatInstructionGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, IList<ChatMessage> chatMessages, string chatInstruction, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                prompt += Environment.NewLine + chatMessages[i].ToPrompt();
            }

            string instructionPrompt = "### Instruction:" + chatInstruction + Environment.NewLine + "### Response:";

            prompt += Environment.NewLine + instructionPrompt;
            ChatSummary chatSummary = null;

            int tokenCount = encoding.CountTokens(prompt);

            ChatSummarizationPrompt chatSummarizationPrompt = await ChatSummarizationPrompt.GetChatSummarizationPrompt(client, user);

            while (tokenCount > maxTokens)
            {
                (ChatSummary, int, MessageGeneration) summary = await GetChatSummaryOrMessageGeneration(client, user, chatMemory, chatSummary, chatMessages, chatSummarizationPrompt, MessageGeneration.SummarizationInputLength);

                if (summary.Item3 != null)
                {
                    // we don't already have the next summary we need, so generate it
                    summary.Item3.nextPurpose = MessageGeneration.ChatInstructionPurpose;
                    summary.Item3.nextPurposeData = new MessageGeneration.PurposeData
                    {
                        instruction = chatInstruction
                    };
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

                    for (int i = 0; i < chatMessages.Count; i++)
                    {
                        prompt += Environment.NewLine + chatMessages[i].ToPrompt();
                    }

                    prompt += Environment.NewLine + instructionPrompt;

                    tokenCount = encoding.CountTokens(prompt);
                }
            }

            return new()
            {
                id = Guid.NewGuid(),
                userId = user.userId,
                prompt = prompt,
                maxInputLength = maxTokens,
                maxOutputLength = MessageGeneration.MaxOutputLength,
                purpose = MessageGeneration.ChatInstructionPurpose,
                status = MessageGeneration.NoneStatus
            };
        }

        private static (string, int) GetChatSummaryPrompt(ChatMemory chatMemory, ChatSummarizationPrompt chatSummarizationPrompt, ChatSummary chatSummary, IList<ChatMessage> chatMessages, int maxTokens)
        {
            var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
            string prompt = chatMemory.memory;

            if (chatSummary != null)
            {
                prompt += Environment.NewLine + chatSummary.ToPrompt();
            }

            string promptInstruction = prompt + Environment.NewLine + chatSummarizationPrompt.prompt;

            int chatMessagesIncluded = 0;

            for (int i = 0; i < chatMessages.Count; i++)
            {
                string tempPrompt = prompt + Environment.NewLine + chatMessages[i].ToPrompt();
                string tempPromptInstruction = tempPrompt + Environment.NewLine + chatSummarizationPrompt.prompt;

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

        public static async Task<(ChatSummary,int,MessageGeneration)> GetChatSummaryOrMessageGeneration(CosmosClient client, CarpenterUser user, ChatMemory chatMemory, ChatSummary chatSummary, IList<ChatMessage> chatMessages, ChatSummarizationPrompt chatSummarizationPrompt, int maxTokens)
        {
            (string,int) value = GetChatSummaryPrompt(chatMemory, chatSummarizationPrompt, chatSummary, chatMessages, maxTokens);
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

        //public static string GeneratePromptTruncateHistory(ChatMemory chatMemory, IList<ChatMessage> chatMessages, string instruction, int maxTokens)
        //{
        //    var encoding = Tiktoken.Encoding.Get(Encodings.Cl100KBase);
        //    string tempMessages = chatMessages[chatMessages.Count - 1].ToPrompt();
        //    string prompt = string.Empty;

        //    for (int i = chatMessages.Count - 2; i >= 0; i--)
        //    {
        //        string newTempMessages = chatMessages[i].ToPrompt() + Environment.NewLine + tempMessages;
        //        string tempPrompt = chatMemory.memory + Environment.NewLine + newTempMessages;

        //        if(!instruction.IsNullOrWhiteSpace())
        //        {
        //            tempPrompt += Environment.NewLine + "### Instruction:" + instruction + Environment.NewLine + "### Response:";
        //        }

        //        int tokenCount = encoding.CountTokens(newTempMessages);

        //        if (tokenCount > maxTokens)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            tempMessages = newTempMessages;
        //            prompt = tempPrompt;
        //        }
        //    }

        //    return prompt;
        //}
    }
}
