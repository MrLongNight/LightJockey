namespace LightJockey.Services;

/// <summary>
/// Service for managing encrypted configuration data
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Stores a secure string value (encrypted)
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Value to encrypt and store</param>
    /// <returns>True if stored successfully</returns>
    Task<bool> SetSecureValueAsync(string key, string value);

    /// <summary>
    /// Retrieves and decrypts a secure string value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>Decrypted value or null if not found</returns>
    Task<string?> GetSecureValueAsync(string key);

    /// <summary>
    /// Removes a configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>True if removed successfully</returns>
    Task<bool> RemoveValueAsync(string key);

    /// <summary>
    /// Checks if a configuration value exists
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <returns>True if the key exists</returns>
    Task<bool> ContainsKeyAsync(string key);

    /// <summary>
    /// Clears all configuration data
    /// </summary>
    /// <returns>True if cleared successfully</returns>
    Task<bool> ClearAllAsync();
}
