namespace ReplicatedLog.Master.Services
{
    public interface IMissedMessageReplicator
    {
        Task ReplicateMissedMessagesAsync(string secondaryUrl);
    }
}