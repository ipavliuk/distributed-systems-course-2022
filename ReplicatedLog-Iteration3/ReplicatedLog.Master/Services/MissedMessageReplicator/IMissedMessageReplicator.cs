namespace ReplicatedLog.Master.Services.MissedMessageReplicator
{
    public interface IMissedMessageReplicator
    {
        Task ReplicateMissedMessagesAsync(string secondaryUrl);
    }
}