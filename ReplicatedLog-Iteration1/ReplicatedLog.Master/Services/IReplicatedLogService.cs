using Common.Model;
using Common.Repository;

namespace ReplicatedLog.Master.Services
{
    public interface IReplicatedLogService
    {
        void AppendMessageToLog(string message);
        List<Message> GetAllMessages();
    }
}
