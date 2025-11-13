using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LightJockey.Tests.Services
{
    /// <summary>
    /// Unit tests for ConfigurationService
    /// </summary>
    public class ConfigurationServiceTests : IDisposable
    {
        private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
        private readonly ConfigurationService _service;
        private readonly string _testConfigPath;
        private readonly string _testConfigDir;

        public ConfigurationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ConfigurationService>>();

            // Isolate test file path
            _testConfigDir = Path.Combine(Path.GetTempPath(), "LightJockeyTests", Guid.NewGuid().ToString());
            _testConfigPath = Path.Combine(_testConfigDir, "secure-config.dat");

            // Use reflection to set the private field for the config path to isolate tests
            var service = new ConfigurationService(_mockLogger.Object);
            var configPathField = typeof(ConfigurationService).GetField("_configFilePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configPathField?.SetValue(service, _testConfigPath);
            var configDirField = typeof(ConfigurationService).GetField("_configDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configDirField?.SetValue(service, _testConfigDir);

            _service = service;
        }

        public void Dispose()
        {
            // Clean up test configuration directory and file
            if (Directory.Exists(_testConfigDir))
            {
                try
                {
                    Directory.Delete(_testConfigDir, true);
                }
                catch
                {
                    // Ignore cleanup errors in case of file locks
                }
            }
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task SetSecureValueAsync_ShouldStoreAndEncryptValue()
        {
            // Arrange
            const string key = "TestKey";
            const string value = "TestSecretValue123!";

            // Act
            var result = await _service.SetSecureValueAsync(key, value);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(_testConfigPath));
            var fileContent = await File.ReadAllTextAsync(_testConfigPath);
            Assert.DoesNotContain(value, fileContent);
        }

        [Fact]
        public async Task GetSecureValueAsync_ShouldDecryptStoredValue()
        {
            // Arrange
            const string key = "TestKey";
            const string value = "MySecretAppKey456!";
            await _service.SetSecureValueAsync(key, value);

            // Act
            var retrievedValue = await _service.GetSecureValueAsync(key);

            // Assert
            Assert.NotNull(retrievedValue);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public async Task GetSecureValueAsync_WithNonExistentKey_ShouldReturnNull()
        {
            // Act
            var result = await _service.GetSecureValueAsync("NonExistentKey");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ConfigurationService_ShouldPersistAcrossInstances()
        {
            // Arrange
            const string key = "PersistentKey";
            const string value = "PersistentValue";
            await _service.SetSecureValueAsync(key, value);

            // Act - Create a new service instance that loads from the same file
            var newService = new ConfigurationService(_mockLogger.Object);
            var configPathField = typeof(ConfigurationService).GetField("_configFilePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configPathField?.SetValue(newService, _testConfigPath);
            var configDirField = typeof(ConfigurationService).GetField("_configDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configDirField?.SetValue(newService, _testConfigDir);

            // We need to manually trigger loading for the test instance
            var loadMethod = typeof(ConfigurationService).GetMethod("LoadConfigurationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)loadMethod?.Invoke(newService, null);

            var retrievedValue = await newService.GetSecureValueAsync(key);

            // Assert
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public async Task RemoveValueAsync_ShouldRemoveStoredValue()
        {
            // Arrange
            const string key = "KeyToRemove";
            const string value = "ValueToRemove";
            await _service.SetSecureValueAsync(key, value);
            Assert.True(await _service.ContainsKeyAsync(key));

            // Act
            var removed = await _service.RemoveValueAsync(key);

            // Assert
            Assert.True(removed);
            Assert.False(await _service.ContainsKeyAsync(key));
            var retrieved = await _service.GetSecureValueAsync(key);
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task ClearAllAsync_ShouldRemoveAllValues()
        {
            // Arrange
            await _service.SetSecureValueAsync("Key1", "Value1");
            await _service.SetSecureValueAsync("Key2", "Value2");

            // Act
            await _service.ClearAllAsync();

            // Assert
            Assert.False(await _service.ContainsKeyAsync("Key1"));
            Assert.False(await _service.ContainsKeyAsync("Key2"));
            Assert.True(File.Exists(_testConfigPath)); // File should still exist but be empty of these keys
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task SetSecureValueAsync_WithInvalidKey_ShouldThrowArgumentException(string key)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.SetSecureValueAsync(key, "some-value"));
        }

        [Fact]
        public async Task SetSecureValueAsync_WithNullValue_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SetSecureValueAsync("some-key", null!));
        }
    }
}
