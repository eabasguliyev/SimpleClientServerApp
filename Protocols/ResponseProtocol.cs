using System.Collections.Generic;

namespace Protocols
{
    public class ResponseProtocol
    {
        public List<Row> Rows { get; set; }

        public ResponseProtocol()
        {
            Rows = new List<Row>();
        }
    }
}
