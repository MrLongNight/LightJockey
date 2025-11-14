namespace LightJockey.Models;

/// <summary>
/// Configuration for automatic backup service
/// </summary>
public record BackupConfig
{
    /// <summary>
    /// Maximum number of backups to retain
    /// </summary>
    public int MaxBackups { get; init; } = 10;

    /// <summary>
    /// Maximum age of backups in days before deletion
    /// </summary>
    public int MaxBackupAgeDays { get; init; } = 30;

    /// <summary>
    /// Enable automatic backups
    /// </summary>
    public bool AutoBackupEnabled { get; init; } = false;

    /// <summary>
    /// Interval between automatic backups in minutes
    /// </summary>
    public int AutoBackupIntervalMinutes { get; init; } = 60;

    /// <summary>
    /// Directory where backups are stored
    /// </summary>
    public string? BackupDirectory { get; init; }
}
