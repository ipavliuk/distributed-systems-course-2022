using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplicatedLog.Common.Exceptions;

public class ConnectionFailureException : Exception
{
    public ConnectionFailureException() : base("Failed to connect to Secondary server.")
    {
    }

    public ConnectionFailureException(string message) : base(message)
    {
    }

    public ConnectionFailureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
