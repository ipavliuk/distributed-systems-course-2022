namespace ReplicatedLog.Master.Services;

public class CountDownLatch
{
    private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
    private readonly CountdownEvent _countdownEvent;

    public CountDownLatch(int count)
    {
        _countdownEvent = new CountdownEvent(count);
    }

    public Task WaitAsync()
    {
        return _tcs.Task;
    }

    public TaskCompletionSource<bool> GetTaskCompletionSource()
    {
        return _tcs;
    }

    public void CountDown()
    {
        _countdownEvent.Signal();
        if (_countdownEvent.CurrentCount == 0)
        {
            _tcs.SetResult(true);
        }
    }
}