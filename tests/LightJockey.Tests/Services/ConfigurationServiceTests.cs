using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services
{
    public class ConfigurationServiceTests : IDisposable
    {
        private const string TestEncryptionKey = "UnitTestEncryptionKey123!";
        private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
        private readonly ConfigurationService _service;
        private readonly string _testConfigPath;

        public ConfigurationServiceTests()
        {
            Environment.SetEnvironmentVariable("CONFIGURATION_ENCRYPTION_KEY", TestEncryptionKey);
            _mockLogger = new Mock<ILogger<ConfigurationService>>();
            _service = new ConfigurationService(_mockLogger.Object);
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _testConfigPath = Path.Combine(appDataPath, "LightJockey", "config.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
            Environment.SetEnvironmentVariable("CONFIGURATION_ENCRYPTION_KEY", null);
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
            var storedValue = await _service.GetSecureValueAsync(key);

            // Assert
            Assert.True(result);
            Assert.Equal(value, storedValue);

            // Verify the value is actually encrypted on disk
            var fileContent = await File.ReadAllTextAsync(_testConfigPath);
            Assert.DoesNotContain(value, fileContent);
        }

        [Fact]
        public async Task GetSecureValueAsync_ShouldDecryptStoredValue()
        {
            // Arrange
            const string key = "AnotherKey";
            const string value = "AnotherSecret!";
            await _service.SetSecureValueAsync(key, value);

            // Act
            var retrievedValue = await _service.GetSecureValueAsync(key);

            // Assert
            Assert.Equal(value, retrievedValue);
        }
    }
}
