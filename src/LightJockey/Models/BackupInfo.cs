namespace LightJockey.Models;

/// <summary>
/// Information about a backup file
/// </summary>
public record BackupInfo
{
    /// <summary>
    /// Unique identifier for the backup
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Filename of the backup
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Full path to the backup file
    /// </summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the backup was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Size of the backup file in bytes
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Description of the backup
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this is an automatic backup
    /// </summary>
    public bool IsAutomatic { get; init; }
}
