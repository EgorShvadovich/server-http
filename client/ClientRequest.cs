using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    internal class ClientRequest
    {
        public String Action { get; set; } = null!;
        public String Author { get; set; } = null!;
        public String Text { get; set; } = null!;
        public DateTime Moment { get; set; }
    }
}
