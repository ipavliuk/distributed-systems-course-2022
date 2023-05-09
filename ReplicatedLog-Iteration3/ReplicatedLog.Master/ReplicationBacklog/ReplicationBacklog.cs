using Common.Model;
using ReplicatedLog.Master.Services.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ReplicatedLog.Common.ReplicationBacklog;


public class BacklogItem
{
    public BacklogItem(Message msg, CountDownLatch countDownLatch)
    {
        Msg = msg;
        CountDownLatch = countDownLatch;
    }
    public Message Msg { get; init; }
    public CountDownLatch CountDownLatch { get; init; }
}

//Replication backlog refers to the set of messages that are not yet replicated to one or more secondary servers.
public class ReplicationBacklog : IReplicationBacklog
{
    private readonly Dictionary<string, Queue<BacklogItem>> _replicationBacklog = new();

    public void AddMessageToBacklog(string secondaryUrl, Message msg, CountDownLatch countDownLatch)
    {
        lock (_replicationBacklog)
        {
            if (!_replicationBacklog.ContainsKey(secondaryUrl))
            {
                _replicationBacklog.Add(secondaryUrl, new Queue<BacklogItem>());
            }

            _replicationBacklog[secondaryUrl].Enqueue(new BacklogItem(msg, countDownLatch));
        }
    }

    public bool TryGetMassages(string url, out Queue<BacklogItem> messages)
    {
        if (!_replicationBacklog.ContainsKey(url))
        {
            messages = null;
            return false;
        }

        messages = _replicationBacklog[url];

        return true;
    }
}
