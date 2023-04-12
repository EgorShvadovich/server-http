using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    internal class ServerResponse
    {
        public String Status { get; set; } = null!;
        public List<ChatMessage> Messages { get; set; } = null!;
    }
}
