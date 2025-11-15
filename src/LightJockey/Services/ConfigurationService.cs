using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LightJockey.Models;

namespace LightJockey.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private readonly string _appSettingsPath;
        private static readonly byte[] Entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };

        public ConfigurationService()
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LightJockey");
            Directory.CreateDirectory(appDataFolder);
            _configPath = Path.Combine(appDataFolder, "config.json");
            _appSettingsPath = Path.Combine(appDataFolder, "settings.json");
        }

        public async Task<LightJockeyEntertainmentConfig> LoadConfigAsync(string? encryptionKey = null)
        {
            if (!File.Exists(_configPath))
            {
                return new LightJockeyEntertainmentConfig();
            }

            var encryptedJson = await File.ReadAllBytesAsync(_configPath);
            var json = Decrypt(encryptedJson);
            return JsonSerializer.Deserialize<LightJockeyEntertainmentConfig>(json) ?? new LightJockeyEntertainmentConfig();
        }

        public async Task SaveConfigAsync(LightJockeyEntertainmentConfig config, string? encryptionKey = null)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var encryptedJson = Encrypt(json);
            await File.WriteAllBytesAsync(_configPath, encryptedJson);
        }

        public async Task<string?> GetSecureValueAsync(string key)
        {
            var config = await LoadConfigAsync();
            if (config.SecureValues.TryGetValue(key, out var encryptedValue))
            {
                var decryptedValue = Decrypt(Convert.FromBase64String(encryptedValue));
                return decryptedValue;
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

        public AppSettings LoadAppSettings()
        {
            if (!File.Exists(_appSettingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_appSettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void SaveAppSettings(AppSettings appSettings)
        {
            var json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_appSettingsPath, json);
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
    }
}
