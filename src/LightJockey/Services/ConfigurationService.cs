using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

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
            _logger.LogError(ex, "Failed to decrypt value for key: {Key}", key);
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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

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
            _logger.LogDebug("No existing configuration file found");
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
            
            if (data != null)
            {
                // Convert base64 strings back to byte arrays
                _encryptedData = data.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Convert.FromBase64String(kvp.Value));
                
                _logger.LogInformation("Loaded {Count} encrypted configuration values", _encryptedData.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration, starting with empty configuration");
            _encryptedData = new Dictionary<string, byte[]>();
        }
    }

    private async Task SaveConfigurationAsync()
    {
        try
        {
            // Convert byte arrays to base64 strings for JSON serialization
            var data = _encryptedData.ToDictionary(
                kvp => kvp.Key,
                kvp => Convert.ToBase64String(kvp.Value));

            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
            _logger.LogDebug("Saved configuration to disk");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to disk");
            throw;
        }
    }
}
