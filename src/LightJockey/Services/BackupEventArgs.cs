namespace LightJockey.Services;

/// <summary>
/// Event arguments for backup-related events
/// </summary>
public class BackupEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the backup file
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Initializes a new instance of the BackupEventArgs class
    /// </summary>
    /// <param name="fileName">The name of the backup file</param>
    public BackupEventArgs(string fileName)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }
}
