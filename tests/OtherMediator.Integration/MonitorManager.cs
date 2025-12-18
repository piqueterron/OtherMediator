namespace OtherMediator.Integration;

using System.Threading.Tasks;

public class MonitorManager
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static int _expectedCount;
    private static int _currentCount;
    private static TaskCompletionSource<bool> _tcs;

    public static async Task InitializeAsync(int expectedCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedCount);

        await _semaphore.WaitAsync();

        try
        {
            _expectedCount = expectedCount;
            _currentCount = 0;
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public static async Task<bool> WaitForCompletionAsync(int timeoutSeconds = 10)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using (cts.Token.Register(() => _tcs?.TrySetResult(false)))
        {
            return await _tcs.Task;
        }
    }

    public static async Task SignalAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_tcs is null)
            {
                return;
            }

            _currentCount++;

            if (_currentCount >= _expectedCount)
            {
                _tcs.TrySetResult(true);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
