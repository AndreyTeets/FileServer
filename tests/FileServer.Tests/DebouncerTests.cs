using System.Diagnostics;
using FileServer.Configuration;

namespace FileServer.Tests;

internal sealed class DebouncerTests : TestsBase
{
    [Test]
    public async Task Debounce_OnlyPerformsLastAction_And_DisposesCorrectly()
    {
        const int maxAllowedExecTimeMs = 500; // Should be greater than the max possible time for the code to execute
        const int waitTimeMs = 5000; // Should be significantly greater than the max allowed exec time
        const int disposeWaitTasksTimeoutMs = 2000; // Should be significantly greater than the max allowed exec time

        using Debouncer debouncer = new(
            waitTime: TimeSpan.FromMilliseconds(waitTimeMs),
            disposeWaitTasksTimeout: TimeSpan.FromMilliseconds(disposeWaitTasksTimeoutMs));
        bool action1Performed = false;
        bool action2Performed = false;

        Stopwatch sw = new();
        sw.Start();
        debouncer.Debounce("", () => action1Performed = true);
        debouncer.Debounce("", () => action2Performed = true);
        debouncer.Dispose(); // Wait for all tasks to finish
        sw.Stop();

        Assert.That(action1Performed, Is.False);
        Assert.That(action2Performed, Is.True);
        // Ensure dispose forced all queued tasks to finish immediately
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(maxAllowedExecTimeMs));
    }   // Ensure 2nd dispose (from using statement) doesn't throw
}
