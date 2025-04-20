using System.Collections.Concurrent;

namespace FileServer.Configuration;

internal sealed class Debouncer(
    TimeSpan? waitTime = null)
    : IDebouncer
{
    private readonly TimeSpan _waitTime = waitTime ?? TimeSpan.FromMilliseconds(1000);
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<string, long> _counter = new();
    private readonly ConcurrentDictionary<string, int> _tasksCount = new();
    private readonly object _actionLock = new();
    private readonly object _disposeLock = new();
    private bool _disposed;

    public void Dispose()
    {
        lock (_disposeLock)
        {
            if (_disposed)
                return;
            using IDisposable _ = _cts;
            _disposed = true;
            _cts.Cancel();
            WaitUntilAllTasksFinish();
        }
    }

    public void Debounce(string category, Action action)
    {
        long current;
        lock (_actionLock)
        {
            if (!_counter.TryGetValue(category, out long counter) || counter == long.MaxValue)
                _counter[category] = 0;
            current = ++_counter[category];

            if (!_tasksCount.ContainsKey(category))
                _tasksCount[category] = 0;
            ++_tasksCount[category];
        }

        _ = Task.Delay(_waitTime, _cts.Token).ContinueWith(task =>
        {
            try
            {
                if (current == _counter[category])
                    action();
            }
            finally
            {
                lock (_actionLock)
                    --_tasksCount[category];
            }
        });
    }

    private void WaitUntilAllTasksFinish()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(1010));
        foreach (string category in _tasksCount.Keys)
            WaitUntilZeroTasksInCategory(category, cts.Token);
    }

    private void WaitUntilZeroTasksInCategory(string category, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _tasksCount[category] > 0)
            Task.Delay(50, ct).GetAwaiter().GetResult();
    }
}

#pragma warning disable MA0048 // File name must match type name
internal interface IDebouncer : IDisposable
{
    public void Debounce(string category, Action action);
}
#pragma warning restore MA0048 // File name must match type name
