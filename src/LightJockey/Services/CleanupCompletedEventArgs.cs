namespace LightJockey.Services;

/// <summary>
/// Event arguments for backup cleanup completed events
/// </summary>
public class CleanupCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the number of remaining backups after cleanup
    /// </summary>
    public int RemainingBackups { get; }

    /// <summary>
    /// Initializes a new instance of the CleanupCompletedEventArgs class
    /// </summary>
    /// <param name="remainingBackups">The number of remaining backups</param>
    public CleanupCompletedEventArgs(int remainingBackups)
    {
        RemainingBackups = remainingBackups;
    }
}
