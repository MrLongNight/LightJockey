using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Service for managing automatic backups of preset configurations
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly IPresetService _presetService;
    private readonly string _backupDirectory;
    private readonly string _configFileName = "backup-config.json";
    private BackupConfig _config;
    private System.Timers.Timer? _autoBackupTimer;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Event raised when a backup is created
    /// </summary>
    public event EventHandler<BackupInfo>? BackupCreated;

    /// <summary>
    /// Event raised when a backup is deleted
    /// </summary>
    public event EventHandler<string>? BackupDeleted;

    /// <summary>
    /// Event raised when backup cleanup occurs
    /// </summary>
    public event EventHandler<CleanupCompletedEventArgs>? BackupCleanupCompleted;

    /// <summary>
    /// Gets whether automatic backup is currently running
    /// </summary>
    public bool IsAutoBackupRunning => _autoBackupTimer?.Enabled ?? false;

    /// <summary>
    /// Initializes a new instance of the BackupService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="presetService">Preset service for accessing configurations</param>
    /// <param name="backupDirectory">Optional backup directory path (defaults to AppData)</param>
    public BackupService(ILogger<BackupService> logger, IPresetService presetService, string? backupDirectory = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _presetService = presetService ?? throw new ArgumentNullException(nameof(presetService));

        // Set up backup directory
        if (string.IsNullOrWhiteSpace(backupDirectory))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _backupDirectory = Path.Combine(appDataPath, "LightJockey", "Backups");
        }
        else
        {
            _backupDirectory = backupDirectory;
        }
        
        Directory.CreateDirectory(_backupDirectory);

        _logger.LogDebug("Backup directory: {BackupDirectory}", _backupDirectory);

        // Load configuration
        _config = LoadConfig();

        // Start auto-backup if enabled
        if (_config.AutoBackupEnabled)
        {
            StartAutoBackup();
        }

        _logger.LogInformation("BackupService initialized");
    }

    /// <summary>
    /// Creates a manual backup of the current configuration
    /// </summary>
    /// <param name="description">Optional description for the backup</param>
    /// <returns>Information about the created backup</returns>
    public async Task<BackupInfo> CreateBackupAsync(string? description = null)
    {
        try
        {
            var timestamp = DateTime.UtcNow;
            var fileName = $"backup_{timestamp:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_backupDirectory, fileName);

            // Export all presets to the backup file
            await _presetService.ExportAllPresetsAsync(filePath);

            var fileInfo = new FileInfo(filePath);
            var backupInfo = new BackupInfo
            {
                FileName = fileName,
                FilePath = filePath,
                CreatedAt = timestamp,
                SizeBytes = fileInfo.Length,
                Description = description,
                IsAutomatic = false
            };

            BackupCreated?.Invoke(this, backupInfo);
            _logger.LogInformation("Created backup: {FileName} ({SizeBytes} bytes)", fileName, fileInfo.Length);

            // Trigger cleanup after creating backup
            await CleanupOldBackupsAsync();

            return backupInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup");
            throw;
        }
    }

    /// <summary>
    /// Restores configuration from a backup
    /// </summary>
    /// <param name="backupId">ID of the backup to restore</param>
    /// <returns>True if restore was successful</returns>
    public async Task<bool> RestoreBackupAsync(string backupId)
    {
        if (string.IsNullOrEmpty(backupId))
        {
            return false;
        }

        try
        {
            var backup = await GetBackupAsync(backupId);
            if (backup == null || !File.Exists(backup.FilePath))
            {
                _logger.LogWarning("Backup not found: {BackupId}", backupId);
                return false;
            }

            // Import presets from backup
            var presets = await _presetService.ImportPresetsAsync(backup.FilePath);
            _logger.LogInformation("Restored {Count} presets from backup: {FileName}", presets.Count, backup.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup: {BackupId}", backupId);
            return false;
        }
    }

    /// <summary>
    /// Gets all available backups
    /// </summary>
    /// <returns>List of backup information</returns>
    public async Task<IReadOnlyList<BackupInfo>> GetAllBackupsAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var backups = new List<BackupInfo>();
                var files = Directory.GetFiles(_backupDirectory, "backup_*.json");

                foreach (var file in files.OrderByDescending(f => File.GetCreationTimeUtc(f)))
                {
                    var fileInfo = new FileInfo(file);
                    var backupInfo = new BackupInfo
                    {
                        Id = Path.GetFileNameWithoutExtension(file),
                        FileName = fileInfo.Name,
                        FilePath = file,
                        CreatedAt = fileInfo.CreationTimeUtc,
                        SizeBytes = fileInfo.Length,
                        IsAutomatic = true // Assume automatic for now
                    };
                    backups.Add(backupInfo);
                }

                return (IReadOnlyList<BackupInfo>)backups.AsReadOnly();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backups");
                return Array.Empty<BackupInfo>();
            }
        });
    }

    /// <summary>
    /// Gets a specific backup by ID
    /// </summary>
    /// <param name="backupId">ID of the backup</param>
    /// <returns>Backup information or null if not found</returns>
    public async Task<BackupInfo?> GetBackupAsync(string backupId)
    {
        var backups = await GetAllBackupsAsync();
        return backups.FirstOrDefault(b => b.Id == backupId);
    }

    /// <summary>
    /// Deletes a specific backup
    /// </summary>
    /// <param name="backupId">ID of the backup to delete</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteBackupAsync(string backupId)
    {
        if (string.IsNullOrEmpty(backupId))
        {
            return false;
        }

        try
        {
            var backup = await GetBackupAsync(backupId);
            if (backup == null || !File.Exists(backup.FilePath))
            {
                return false;
            }

            File.Delete(backup.FilePath);
            BackupDeleted?.Invoke(this, backupId);
            _logger.LogInformation("Deleted backup: {FileName}", backup.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting backup: {BackupId}", backupId);
            return false;
        }
    }

    /// <summary>
    /// Performs cleanup of old backups based on retention policy
    /// </summary>
    /// <returns>Number of backups deleted</returns>
    public async Task<int> CleanupOldBackupsAsync()
    {
        try
        {
            var backups = await GetAllBackupsAsync();
            var deletedCount = 0;

            // Delete backups exceeding max count
            if (backups.Count > _config.MaxBackups)
            {
                var backupsToDelete = backups
                    .OrderBy(b => b.CreatedAt)
                    .Take(backups.Count - _config.MaxBackups);

                foreach (var backup in backupsToDelete)
                {
                    if (await DeleteBackupAsync(backup.Id))
                    {
                        deletedCount++;
                    }
                }
            }

            // Delete backups older than max age
            var cutoffDate = DateTime.UtcNow.AddDays(-_config.MaxBackupAgeDays);
            var oldBackups = backups.Where(b => b.CreatedAt < cutoffDate);

            foreach (var backup in oldBackups)
            {
                if (await DeleteBackupAsync(backup.Id))
                {
                    deletedCount++;
                }
            }

            // Get remaining backup count and raise event
            var remainingBackups = await GetAllBackupsAsync();
            BackupCleanupCompleted?.Invoke(this, new CleanupCompletedEventArgs(remainingBackups.Count));
            
            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old backups, {Remaining} remaining", deletedCount, remainingBackups.Count);
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
            return 0;
        }
    }

    /// <summary>
    /// Gets the current backup configuration
    /// </summary>
    /// <returns>Current backup configuration</returns>
    public BackupConfig GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// Updates the backup configuration
    /// </summary>
    /// <param name="config">New backup configuration</param>
    public async Task UpdateConfigAsync(BackupConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        try
        {
            _config = config with { BackupDirectory = _backupDirectory };
            await SaveConfigAsync();

            // Restart auto-backup if settings changed
            if (_config.AutoBackupEnabled && !IsAutoBackupRunning)
            {
                StartAutoBackup();
            }
            else if (!_config.AutoBackupEnabled && IsAutoBackupRunning)
            {
                StopAutoBackup();
            }
            else if (IsAutoBackupRunning)
            {
                // Restart with new interval
                StopAutoBackup();
                StartAutoBackup();
            }

            _logger.LogInformation("Updated backup configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating backup configuration");
            throw;
        }
    }

    /// <summary>
    /// Starts the automatic backup timer
    /// </summary>
    public void StartAutoBackup()
    {
        if (_autoBackupTimer != null)
        {
            StopAutoBackup();
        }

        var intervalMs = _config.AutoBackupIntervalMinutes * 60 * 1000;
        _autoBackupTimer = new System.Timers.Timer(intervalMs);
        _autoBackupTimer.Elapsed += OnAutoBackupTimerElapsed;
        _autoBackupTimer.AutoReset = true;
        _autoBackupTimer.Start();

        _logger.LogInformation("Started automatic backup timer (interval: {Minutes} minutes)", _config.AutoBackupIntervalMinutes);
    }

    /// <summary>
    /// Stops the automatic backup timer
    /// </summary>
    public void StopAutoBackup()
    {
        if (_autoBackupTimer != null)
        {
            _autoBackupTimer.Stop();
            _autoBackupTimer.Elapsed -= OnAutoBackupTimerElapsed;
            _autoBackupTimer.Dispose();
            _autoBackupTimer = null;
            _logger.LogInformation("Stopped automatic backup timer");
        }
    }

    private async void OnAutoBackupTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _logger.LogDebug("Automatic backup triggered");
            var backupInfo = await CreateBackupAsync("Automatic backup");
            // Update to mark as automatic
            backupInfo = backupInfo with { IsAutomatic = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic backup");
        }
    }

    private BackupConfig LoadConfig()
    {
        var configPath = Path.Combine(_backupDirectory, _configFileName);

        if (!File.Exists(configPath))
        {
            _logger.LogDebug("No existing backup config found, using defaults");
            return new BackupConfig { BackupDirectory = _backupDirectory };
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<BackupConfig>(json, JsonOptions);
            _logger.LogInformation("Loaded backup configuration");
            return config ?? new BackupConfig { BackupDirectory = _backupDirectory };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading backup config, using defaults");
            return new BackupConfig { BackupDirectory = _backupDirectory };
        }
    }

    private async Task SaveConfigAsync()
    {
        var configPath = Path.Combine(_backupDirectory, _configFileName);

        try
        {
            var json = JsonSerializer.Serialize(_config, JsonOptions);
            await File.WriteAllTextAsync(configPath, json);
            _logger.LogDebug("Saved backup configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving backup configuration");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAutoBackup();
        _disposed = true;
        _logger.LogDebug("BackupService disposed");
    }
}
