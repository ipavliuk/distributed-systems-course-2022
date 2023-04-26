using Common.Model;
using Common.Repository;

namespace ReplicatedLog.Master.Services
{
    public interface IReplicatedLogService
    {
        Task AppendMessageToLog(string message, int writeConcern);
        List<Message> GetAllMessages();
    }
}
