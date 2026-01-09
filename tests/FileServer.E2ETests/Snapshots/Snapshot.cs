namespace FileServer.E2ETests.Snapshots;

internal abstract class Snapshot
{
    protected abstract string Value { get; }

    public static implicit operator string(Snapshot snapshot) => snapshot.Value;
}
