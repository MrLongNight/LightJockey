using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _configPath;
        private readonly string _appSettingsPath;

        private LightJockeyEntertainmentConfig? _cachedConfig;
        private AppSettings? _cachedAppSettings;

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightJockey");
            Directory.CreateDirectory(appDataFolder);
            _configPath = Path.Combine(appDataFolder, "config.json");
            _appSettingsPath = Path.Combine(appDataFolder, "settings.json");
        }

        public async Task<LightJockeyEntertainmentConfig> LoadConfigAsync()
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            if (!File.Exists(_configPath))
            {
                _cachedConfig = new LightJockeyEntertainmentConfig();
                return _cachedConfig;
            }

            try
            {
                var encryptedJson = await File.ReadAllTextAsync(_configPath);
                if (string.IsNullOrEmpty(encryptedJson))
                {
                    _cachedConfig = new LightJockeyEntertainmentConfig();
                    return _cachedConfig;
                }
                var json = Decrypt(encryptedJson);
                _cachedConfig = JsonSerializer.Deserialize<LightJockeyEntertainmentConfig>(json) ?? new LightJockeyEntertainmentConfig();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize config.json. A new configuration will be created. Old file will be backed up.");
                BackupCorruptedFile(_configPath);
                _cachedConfig = new LightJockeyEntertainmentConfig();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while loading the configuration.");
                _cachedConfig = new LightJockeyEntertainmentConfig();
            }

            return _cachedConfig;
        }

        public async Task SaveConfigAsync(LightJockeyEntertainmentConfig config)
        {
            _cachedConfig = config;
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var encryptedJson = Encrypt(json);
            await File.WriteAllTextAsync(_configPath, encryptedJson);
        }

        public async Task<string?> GetSecureValueAsync(string key)
        {
            var config = await LoadConfigAsync();
            if (config.SecureValues.TryGetValue(key, out var encryptedValue))
            {
                try
                {
                    return Decrypt(encryptedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt secure value for key: {Key}", key);
                    return null;
                }
            }
            return null;
        }

        public async Task SetSecureValueAsync(string key, string value)
        {
            var config = await LoadConfigAsync();
            config.SecureValues[key] = Encrypt(value);
            await SaveConfigAsync(config);
        }

        public async Task<AppSettings> LoadAppSettingsAsync()
        {
            if (_cachedAppSettings != null)
            {
                return _cachedAppSettings;
            }

            if (!File.Exists(_appSettingsPath))
            {
                _cachedAppSettings = new AppSettings();
                return _cachedAppSettings;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_appSettingsPath);
                _cachedAppSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize settings.json. New settings will be created. Old file will be backed up.");
                BackupCorruptedFile(_appSettingsPath);
                _cachedAppSettings = new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while loading app settings.");
                _cachedAppSettings = new AppSettings();
            }

            return _cachedAppSettings;
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
        {
            _cachedAppSettings = appSettings;
            var json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_appSettingsPath, json);
        }

        private string Encrypt(string plainText)
        {
            return Convert.ToBase64String(
                ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser)
            );
        }

        private string Decrypt(string cipherText)
        {
            return Encoding.UTF8.GetString(
                ProtectedData.Unprotect(Convert.FromBase64String(cipherText), null, DataProtectionScope.CurrentUser)
            );
        }

        private void BackupCorruptedFile(string filePath)
        {
            try
            {
                var backupPath = $"{filePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
                File.Move(filePath, backupPath);
                _logger.LogInformation("Backed up corrupted file to: {BackupPath}", backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to back up corrupted file: {FilePath}", filePath);
            }
        }
    }
}
