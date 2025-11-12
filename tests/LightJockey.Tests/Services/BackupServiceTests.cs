using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LightJockey.Tests.Services;

public class BackupServiceTests : IDisposable
{
    private readonly Mock<ILogger<BackupService>> _mockLogger;
    private readonly Mock<IPresetService> _mockPresetService;
    private readonly string _testBackupDirectory;
    private readonly BackupService _backupService;

    public BackupServiceTests()
    {
        _mockLogger = new Mock<ILogger<BackupService>>();
        _mockPresetService = new Mock<IPresetService>();

        // Create temporary test directory
        _testBackupDirectory = Path.Combine(Path.GetTempPath(), $"LightJockey_BackupTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testBackupDirectory);

        _backupService = new BackupService(_mockLogger.Object, _mockPresetService.Object);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var service = new BackupService(_mockLogger.Object, _mockPresetService.Object);

        // Assert
        Assert.NotNull(service);
        Assert.False(service.IsAutoBackupRunning);
    }

    [Fact]
    public async Task CreateBackupAsync_CreatesBackupFile()
    {
        // Arrange
        var testPresetCollection = new PresetCollection();
        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var backupInfo = await _backupService.CreateBackupAsync("Test backup");

        // Assert
        Assert.NotNull(backupInfo);
        Assert.False(string.IsNullOrEmpty(backupInfo.FileName));
        Assert.Contains("backup_", backupInfo.FileName);
        Assert.Equal("Test backup", backupInfo.Description);
        Assert.False(backupInfo.IsAutomatic);
        _mockPresetService.Verify(x => x.ExportAllPresetsAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateBackupAsync_RaisesBackupCreatedEvent()
    {
        // Arrange
        BackupInfo? raisedBackupInfo = null;
        _backupService.BackupCreated += (sender, info) => raisedBackupInfo = info;

        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var backupInfo = await _backupService.CreateBackupAsync("Test");

        // Assert
        Assert.NotNull(raisedBackupInfo);
        Assert.Equal(backupInfo.FileName, raisedBackupInfo.FileName);
    }

    [Fact]
    public async Task GetAllBackupsAsync_ReturnsEmptyListInitially()
    {
        // Act
        var backups = await _backupService.GetAllBackupsAsync();

        // Assert
        Assert.NotNull(backups);
        Assert.Empty(backups);
    }

    [Fact]
    public async Task GetBackupAsync_ReturnsNullForNonexistentBackup()
    {
        // Act
        var backup = await _backupService.GetBackupAsync("nonexistent");

        // Assert
        Assert.Null(backup);
    }

    [Fact]
    public async Task DeleteBackupAsync_ReturnsFalseForNonexistentBackup()
    {
        // Act
        var result = await _backupService.DeleteBackupAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteBackupAsync_RaisesBackupDeletedEvent()
    {
        // Arrange
        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var backupInfo = await _backupService.CreateBackupAsync();
        
        string? deletedBackupId = null;
        _backupService.BackupDeleted += (sender, id) => deletedBackupId = id;

        // Act
        var result = await _backupService.DeleteBackupAsync(backupInfo.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(backupInfo.Id, deletedBackupId);
    }

    [Fact]
    public async Task CleanupOldBackupsAsync_DeletesBackupsExceedingMaxCount()
    {
        // Arrange
        var config = new BackupConfig { MaxBackups = 3, MaxBackupAgeDays = 365 };
        await _backupService.UpdateConfigAsync(config);

        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Create 5 backups
        for (int i = 0; i < 5; i++)
        {
            await _backupService.CreateBackupAsync($"Backup {i}");
            await Task.Delay(100); // Ensure different timestamps
        }

        // Act
        var deletedCount = await _backupService.CleanupOldBackupsAsync();

        // Assert
        Assert.Equal(2, deletedCount); // Should delete 2 oldest backups
        var remainingBackups = await _backupService.GetAllBackupsAsync();
        Assert.Equal(3, remainingBackups.Count);
    }

    [Fact]
    public async Task CleanupOldBackupsAsync_RaisesCleanupCompletedEvent()
    {
        // Arrange
        var config = new BackupConfig { MaxBackups = 1, MaxBackupAgeDays = 365 };
        await _backupService.UpdateConfigAsync(config);

        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _backupService.CreateBackupAsync("Backup 1");
        await Task.Delay(100);
        await _backupService.CreateBackupAsync("Backup 2");

        int? cleanedCount = null;
        _backupService.BackupCleanupCompleted += (sender, count) => cleanedCount = count;

        // Act
        await _backupService.CleanupOldBackupsAsync();

        // Assert
        Assert.NotNull(cleanedCount);
        Assert.Equal(1, cleanedCount.Value);
    }

    [Fact]
    public async Task RestoreBackupAsync_ReturnsFalseForNonexistentBackup()
    {
        // Act
        var result = await _backupService.RestoreBackupAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreBackupAsync_CallsImportPresetsAsync()
    {
        // Arrange
        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockPresetService
            .Setup(x => x.ImportPresetsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Preset> { new Preset { Name = "Test" } });

        var backupInfo = await _backupService.CreateBackupAsync("Test");

        // Act
        var result = await _backupService.RestoreBackupAsync(backupInfo.Id);

        // Assert
        Assert.True(result);
        _mockPresetService.Verify(x => x.ImportPresetsAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetConfig_ReturnsCurrentConfiguration()
    {
        // Act
        var config = _backupService.GetConfig();

        // Assert
        Assert.NotNull(config);
        Assert.True(config.AutoBackupEnabled);
        Assert.Equal(10, config.MaxBackups);
        Assert.Equal(30, config.MaxBackupAgeDays);
        Assert.Equal(60, config.AutoBackupIntervalMinutes);
    }

    [Fact]
    public async Task UpdateConfigAsync_UpdatesConfiguration()
    {
        // Arrange
        var newConfig = new BackupConfig
        {
            MaxBackups = 5,
            MaxBackupAgeDays = 15,
            AutoBackupEnabled = false,
            AutoBackupIntervalMinutes = 30
        };

        // Act
        await _backupService.UpdateConfigAsync(newConfig);
        var config = _backupService.GetConfig();

        // Assert
        Assert.Equal(5, config.MaxBackups);
        Assert.Equal(15, config.MaxBackupAgeDays);
        Assert.False(config.AutoBackupEnabled);
        Assert.Equal(30, config.AutoBackupIntervalMinutes);
    }

    [Fact]
    public void StartAutoBackup_StartsTimer()
    {
        // Arrange
        var config = new BackupConfig { AutoBackupEnabled = true };
        _backupService.UpdateConfigAsync(config).Wait();

        // Act
        _backupService.StartAutoBackup();

        // Assert
        Assert.True(_backupService.IsAutoBackupRunning);
    }

    [Fact]
    public void StopAutoBackup_StopsTimer()
    {
        // Arrange
        _backupService.StartAutoBackup();

        // Act
        _backupService.StopAutoBackup();

        // Assert
        Assert.False(_backupService.IsAutoBackupRunning);
    }

    [Fact]
    public async Task UpdateConfigAsync_StartsAutoBackupWhenEnabled()
    {
        // Arrange
        var config = new BackupConfig { AutoBackupEnabled = true };

        // Act
        await _backupService.UpdateConfigAsync(config);

        // Assert
        Assert.True(_backupService.IsAutoBackupRunning);
    }

    [Fact]
    public async Task UpdateConfigAsync_StopsAutoBackupWhenDisabled()
    {
        // Arrange
        _backupService.StartAutoBackup();
        var config = new BackupConfig { AutoBackupEnabled = false };

        // Act
        await _backupService.UpdateConfigAsync(config);

        // Assert
        Assert.False(_backupService.IsAutoBackupRunning);
    }

    [Fact]
    public async Task CreateBackupAsync_TriggersCleanup()
    {
        // Arrange
        var config = new BackupConfig { MaxBackups = 2 };
        await _backupService.UpdateConfigAsync(config);

        _mockPresetService
            .Setup(x => x.ExportAllPresetsAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Create 3 backups
        await _backupService.CreateBackupAsync("Backup 1");
        await Task.Delay(100);
        await _backupService.CreateBackupAsync("Backup 2");
        await Task.Delay(100);
        await _backupService.CreateBackupAsync("Backup 3");

        // Act
        var backups = await _backupService.GetAllBackupsAsync();

        // Assert
        Assert.Equal(2, backups.Count); // Cleanup should have run automatically
    }

    public void Dispose()
    {
        _backupService?.Dispose();
        
        // Cleanup test directory
        if (Directory.Exists(_testBackupDirectory))
        {
            try
            {
                Directory.Delete(_testBackupDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
