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
        private static readonly byte[] Entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };

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
                var encryptedJson = await File.ReadAllBytesAsync(_configPath);
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
            await File.WriteAllBytesAsync(_configPath, encryptedJson);
        }

        public async Task<string?> GetSecureValueAsync(string key)
        {
            var config = await LoadConfigAsync();
            if (config.SecureValues.TryGetValue(key, out var encryptedValue))
            {
                try
                {
                    var decryptedValue = Decrypt(Convert.FromBase64String(encryptedValue));
                    return decryptedValue;
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
            var encryptedValue = Convert.ToBase64String(Encrypt(value));
            config.SecureValues[key] = encryptedValue;
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

        private byte[] Encrypt(string plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            return ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
        }

        private string Decrypt(byte[] cipherText)
        {
            var data = ProtectedData.Unprotect(cipherText, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
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

        public void RemoveValue(string key)
        {
            var config = LoadConfigAsync().GetAwaiter().GetResult();
            if (config.SecureValues.Remove(key))
            {
                SaveConfigAsync(config).GetAwaiter().GetResult();
            }
        }

        public bool ContainsKey(string key)
        {
            var config = LoadConfigAsync().GetAwaiter().GetResult();
            return config.SecureValues.ContainsKey(key);
        }

        public void ClearAll()
        {
            var config = LoadConfigAsync().GetAwaiter().GetResult();
            config.SecureValues.Clear();
            SaveConfigAsync(config).GetAwaiter().GetResult();
        }
    }
}
