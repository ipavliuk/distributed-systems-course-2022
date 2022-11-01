namespace ReplicatedLog.Master.Services
{
    public class IdProvider
    {
        static long Id = 0;

        public static long GenerateId() => Interlocked.Increment(ref Id);

    }
}
