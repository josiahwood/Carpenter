using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpenterApi.Models
{
    internal class ChatMessage
    {
        public Guid id;
        public string userId;
        public DateTime dateTime;
        public string sender;
        public string message;
        public ModelParameters modelParameters;
    }
}
