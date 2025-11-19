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
            if (_cachedConfig != null) return _cachedConfig;
            if (!File.Exists(_configPath))
            {
                _cachedConfig = new LightJockeyEntertainmentConfig();
                return _cachedConfig;
            }

            try
            {
                var fileContent = await File.ReadAllTextAsync(_configPath);
                _cachedConfig = JsonSerializer.Deserialize<LightJockeyEntertainmentConfig>(fileContent) ?? new LightJockeyEntertainmentConfig();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load or deserialize config.json.");
                _cachedConfig = new LightJockeyEntertainmentConfig();
            }
            return _cachedConfig;
        }

        public async Task SaveConfigAsync(LightJockeyEntertainmentConfig config)
        {
            _cachedConfig = config;
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configPath, json);
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

        public async Task<bool> SetSecureValueAsync(string key, string value)
        {
            var config = await LoadConfigAsync();
            config.SecureValues[key] = Encrypt(value);
            await SaveConfigAsync(config);
            return true;
        }

        public async Task<AppSettings> LoadAppSettingsAsync()
        {
            if (_cachedAppSettings != null) return _cachedAppSettings;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load or deserialize settings.json.");
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
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            try 
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                // Verschlüsselung mit Windows DPAPI (User Scope)
                byte[] cipherBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(cipherBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption failed.");
                throw;
            }
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                // Entschlüsselung mit Windows DPAPI (User Scope)
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failed.");
                throw;
            }
        }

        private void BackupCorruptedFile(string filePath)
        {
            var backupPath = $"{filePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
            try
            {
                File.Move(filePath, backupPath);
                _logger.LogInformation("Backed up corrupted file to {BackupPath}", backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to back up corrupted file: {FilePath}", filePath);
            }
        }

        public async Task<bool> RemoveValueAsync(string key)
        {
            var config = await LoadConfigAsync();
            if (config.SecureValues.Remove(key))
            {
                await SaveConfigAsync(config);
                return true;
            }
            return false;
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            var config = await LoadConfigAsync();
            return config.SecureValues.ContainsKey(key);
        }

        public async Task<bool> ClearAllAsync()
        {
            var config = await LoadConfigAsync();
            config.SecureValues.Clear();
            await SaveConfigAsync(config);
            return true;
        }
    }
}
