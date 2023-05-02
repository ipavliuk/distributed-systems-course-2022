using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ReplicatedLog.Common.Repository;


public class BacklogItem
{
    public BacklogItem(Message msg, TaskCompletionSource<bool> tcs)
    {
        Msg = msg;
        Tcs = tcs;
    }
    public Message Msg { get; init; }
    public TaskCompletionSource<bool> Tcs { get; init; }
}

//Replication backlog refers to the set of messages that are not yet replicated to one or more secondary servers.
public class ReplicationBacklog : IReplicationBacklog
{
    private readonly Dictionary<string, Queue<BacklogItem>> _replicationBacklog = new();
   
    public void AddMessageToBacklog(string secondaryUrl, Message msg, TaskCompletionSource<bool> latchCompletion)
    {
        lock (_replicationBacklog)
        {
            if (!_replicationBacklog.ContainsKey(secondaryUrl))
            {
                _replicationBacklog.Add(secondaryUrl, new Queue<BacklogItem>());
            }

            _replicationBacklog[secondaryUrl].Enqueue(new BacklogItem(msg, latchCompletion));
        }
    }

    public bool TryGetMassages(string url, out Queue<BacklogItem> messages)
    {
        messages = null;
        if (!_replicationBacklog.ContainsKey(url))
        {
            return false;
        }

        messages = _replicationBacklog[url];

        return true;
    }
}
