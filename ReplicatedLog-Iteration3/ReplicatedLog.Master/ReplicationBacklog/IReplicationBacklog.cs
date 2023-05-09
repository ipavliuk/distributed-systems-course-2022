using Common.Model;
using ReplicatedLog.Master.Services.Utils;

namespace ReplicatedLog.Common.ReplicationBacklog;

public interface IReplicationBacklog
{
    void AddMessageToBacklog(string secondaryUrl, Message msg, CountDownLatch countDownLatch);
    bool TryGetMassages(string url, out Queue<BacklogItem> messages);
}
