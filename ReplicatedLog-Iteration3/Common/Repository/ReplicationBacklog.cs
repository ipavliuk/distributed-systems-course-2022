using Common.Model;
using System;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<long, int> _outstandingMessages = new();

    public void AddMessageToBacklog(string secondaryUrl, Message msg, TaskCompletionSource<bool> latchCompletion)
    {
        lock (_replicationBacklog)
        {
            if (!_replicationBacklog.ContainsKey(secondaryUrl))
            {
                _replicationBacklog.Add(secondaryUrl, new Queue<BacklogItem>());
            }

            _replicationBacklog[secondaryUrl].Enqueue(new BacklogItem(msg, latchCompletion));
            _outstandingMessages.AddOrUpdate(msg.SequenceId, 1, updateValueFactory: (key, currentValue) => currentValue + 1);
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

    public void MessageCompleted(long sequanceId)
    {
        if (!_outstandingMessages.ContainsKey(sequanceId))
        {
            return;
        }

        _outstandingMessages.AddOrUpdate(sequanceId, 0, updateValueFactory: (key, currentValue) => currentValue - 1);
    }

    public bool IsMessageReplicated(long sequenceId)
    {
        return _outstandingMessages.ContainsKey(sequenceId) && _outstandingMessages[sequenceId] == 0;
    }
    public void RemoveOutstandingMessage(long sequenceId)
    {
        _outstandingMessages.TryRemove(sequenceId, out int _);
    }
}
