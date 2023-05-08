using Common.Model;

namespace ReplicatedLog.Common.Repository;

public interface IReplicationBacklog
{
    void AddMessageToBacklog(string secondaryUrl, Message msg, TaskCompletionSource<bool> latchCompletion);
    bool TryGetMassages(string url, out Queue<BacklogItem> messages);
    void MessageCompleted(long sequanceId);
    bool IsMessageReplicated(long sequenceId);
    void RemoveOutstandingMessage(long sequenceId);
}
