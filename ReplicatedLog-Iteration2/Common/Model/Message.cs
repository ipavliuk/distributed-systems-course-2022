using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public record Message
    {
        public long SequenceId { get; init; }
        public string Msg { get; init; }

        public Message(long sequenceId, string msg) 
        {
            SequenceId = sequenceId; 
            Msg = msg;
        }
        
    }
}
