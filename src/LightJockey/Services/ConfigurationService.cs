using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services
{
    /// <summary>
    /// Configuration service that encrypts sensitive data using Windows DPAPI
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private Dictionary<string, byte[]> _encryptedData;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Initializes a new instance of the ConfigurationService class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set up configuration directory
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _configDirectory = Path.Combine(appDataPath, "LightJockey");
            _configFilePath = Path.Combine(_configDirectory, "secure-config.dat");

            Directory.CreateDirectory(_configDirectory);
            _encryptedData = new Dictionary<string, byte[]>();

            // Load existing configuration
            LoadConfigurationAsync().GetAwaiter().GetResult();

            _logger.LogDebug("ConfigurationService initialized with path: {ConfigPath}", _configFilePath);
        }

        /// <inheritdoc/>
        public async Task<bool> SetSecureValueAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            await _fileLock.WaitAsync();
            try
            {
                // Encrypt the value using DPAPI
                var plainTextBytes = Encoding.UTF8.GetBytes(value);
                var encryptedBytes = ProtectedData.Protect(
                    plainTextBytes,
                    optionalEntropy: null,
                    scope: DataProtectionScope.CurrentUser);

                // Store encrypted data
                _encryptedData[key] = encryptedBytes;

                // Persist to disk
                await SaveConfigurationAsync();

                _logger.LogDebug("Securely stored value for key: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing secure value for key: {Key}", key);
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<string?> GetSecureValueAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            await _fileLock.WaitAsync();
            try
            {
                if (!_encryptedData.TryGetValue(key, out var encryptedBytes))
                {
                    _logger.LogDebug("Key not found: {Key}", key);
                    return null;
                }

                // Decrypt using DPAPI
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    optionalEntropy: null,
                    scope: DataProtectionScope.CurrentUser);

                var decryptedValue = Encoding.UTF8.GetString(decryptedBytes);
                _logger.LogDebug("Successfully retrieved and decrypted value for key: {Key}", key);
                return decryptedValue;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "Failed to decrypt value for key: {Key}. The data may be corrupt or inaccessible.", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secure value for key: {Key}", key);
                return null;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveValueAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            await _fileLock.WaitAsync();
            try
            {
                if (!_encryptedData.Remove(key))
                {
                    _logger.LogDebug("Key not found for removal: {Key}", key);
                    return false;
                }

                await SaveConfigurationAsync();
                _logger.LogDebug("Removed value for key: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value for key: {Key}", key);
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ContainsKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            await _fileLock.WaitAsync();
            try
            {
                return _encryptedData.ContainsKey(key);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ClearAllAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                _encryptedData.Clear();
                await SaveConfigurationAsync();
                _logger.LogInformation("Cleared all secure configuration data");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all configuration data");
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task LoadConfigurationAsync()
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogDebug("No existing configuration file found at {ConfigPath}", _configFilePath);
                return;
            }

            await _fileLock.WaitAsync();
            try
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Configuration file is empty, initializing new configuration.");
                    _encryptedData = new Dictionary<string, byte[]>();
                    return;
                }

                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                
                if (data != null)
                {
                    _encryptedData = data.ToDictionary(
                        kvp => kvp.Key,
                        kvp => Convert.FromBase64String(kvp.Value));

                    _logger.LogInformation("Loaded {Count} encrypted configuration values from {ConfigPath}", _encryptedData.Count, _configFilePath);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing configuration file at {ConfigPath}. Starting with an empty configuration.", _configFilePath);
                _encryptedData = new Dictionary<string, byte[]>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from {ConfigPath}, starting with empty configuration", _configFilePath);
                _encryptedData = new Dictionary<string, byte[]>();
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task SaveConfigurationAsync()
        {
            // This method assumes the lock is already held
            try
            {
                var data = _encryptedData.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Convert.ToBase64String(kvp.Value));

                var json = JsonSerializer.Serialize(data, JsonOptions);
                await File.WriteAllTextAsync(_configFilePath, json);
                _logger.LogDebug("Saved configuration to {ConfigPath}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration to disk at {ConfigPath}", _configFilePath);
                throw; // Re-throw to indicate the save operation failed
            }
        }
    }
}
