namespace ReplicatedLog.Master.Services.Utils
{
    public class IdProvider
    {
        static long Id = 0;

        public static long GenerateId() => Interlocked.Increment(ref Id);

    }
}
