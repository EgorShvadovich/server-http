using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    internal class ChatMessage
    {
        public String Author { get; set; } = null!;
        public String Text { get; set; } = null!;
        public DateTime Moment { get; set; }
    }
}
