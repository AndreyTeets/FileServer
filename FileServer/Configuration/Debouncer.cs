namespace FileServer.Configuration;

public class Debouncer : IDebouncer, IDisposable
{
    private long _counter;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _waitTime;

    public Debouncer(TimeSpan? waitTime = null)
    {
        _waitTime = waitTime ?? TimeSpan.FromSeconds(1);
    }

    public void Debounce(Action action)
    {
        long current = Interlocked.Increment(ref _counter);
        Task.Delay(_waitTime).ContinueWith(task =>
        {
            using IDisposable _ = task;
            if (current == _counter && !_cts.IsCancellationRequested)
                action();
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}

public interface IDebouncer
{
    public void Debounce(Action action);
}
