using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public record Message
    {
        public long Id { get; init; }

        public string Msg { get; init; }
        public Message(long id, string msg) 
        {
            Id = id; 
            Msg = msg;
        }
        
    }
}
