using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing automatic backups of preset configurations
/// </summary>
public interface IBackupService : IDisposable
{
    /// <summary>
    /// Event raised when a backup is created
    /// </summary>
    event EventHandler<BackupInfo>? BackupCreated;

    /// <summary>
    /// Event raised when a backup is deleted
    /// </summary>
    event EventHandler<string>? BackupDeleted;

    /// <summary>
    /// Event raised when backup cleanup occurs
    /// </summary>
    event EventHandler<CleanupCompletedEventArgs>? BackupCleanupCompleted;

    /// <summary>
    /// Creates a manual backup of the current configuration
    /// </summary>
    /// <param name="description">Optional description for the backup</param>
    /// <returns>Information about the created backup</returns>
    Task<BackupInfo> CreateBackupAsync(string? description = null);

    /// <summary>
    /// Restores configuration from a backup
    /// </summary>
    /// <param name="backupId">ID of the backup to restore</param>
    /// <returns>True if restore was successful</returns>
    Task<bool> RestoreBackupAsync(string backupId);

    /// <summary>
    /// Gets all available backups
    /// </summary>
    /// <returns>List of backup information</returns>
    Task<IReadOnlyList<BackupInfo>> GetAllBackupsAsync();

    /// <summary>
    /// Gets a specific backup by ID
    /// </summary>
    /// <param name="backupId">ID of the backup</param>
    /// <returns>Backup information or null if not found</returns>
    Task<BackupInfo?> GetBackupAsync(string backupId);

    /// <summary>
    /// Deletes a specific backup
    /// </summary>
    /// <param name="backupId">ID of the backup to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteBackupAsync(string backupId);

    /// <summary>
    /// Performs cleanup of old backups based on retention policy
    /// </summary>
    /// <returns>Number of backups deleted</returns>
    Task<int> CleanupOldBackupsAsync();

    /// <summary>
    /// Gets the current backup configuration
    /// </summary>
    /// <returns>Current backup configuration</returns>
    BackupConfig GetConfig();

    /// <summary>
    /// Updates the backup configuration
    /// </summary>
    /// <param name="config">New backup configuration</param>
    Task UpdateConfigAsync(BackupConfig config);

    /// <summary>
    /// Starts the automatic backup timer
    /// </summary>
    void StartAutoBackup();

    /// <summary>
    /// Stops the automatic backup timer
    /// </summary>
    void StopAutoBackup();

    /// <summary>
    /// Gets whether automatic backup is currently running
    /// </summary>
    bool IsAutoBackupRunning { get; }
}
