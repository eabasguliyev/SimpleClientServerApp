using System;
using System.Collections.Generic;

namespace Protocols
{
    public class Row
    {
        public List<Object> Fields { get; set; }

        public Row()
        {
            Fields = new List<object>();
        }
    }
}