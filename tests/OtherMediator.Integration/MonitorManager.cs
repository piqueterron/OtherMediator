namespace OtherMediator.Integration;

using System.Threading.Tasks;

public static class MonitorManager
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static int _count;
    private static TaskCompletionSource _tcs;

    public static async Task<bool> WaitAsync(int counter, int timeoutSeconds = 10)
    {
        if (counter <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(counter), "The counter must be greater than zero");
        }

        await _semaphore.WaitAsync();

        try
        {
            _count = counter;
            _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        finally
        {
            _semaphore.Release();
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using (cts.Token.Register(() => _tcs.TrySetResult()))
        {
            await _tcs.Task;
        }

        return _count == 0;
    }

    public static async Task SignalAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_count <= 0)
            {
                return;
            }

            _count--;

            if (_count == 0)
            {
                _tcs?.TrySetResult();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
