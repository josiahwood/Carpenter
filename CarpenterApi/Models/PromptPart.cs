using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class PromptPart
    {
        public enum Type
        {
            Memory,
            ChatSummary,
            ChatMessage,
            Instruction
        }

        public string Prompt;
    }
}
