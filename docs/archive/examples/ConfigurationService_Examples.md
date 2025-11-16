# ConfigurationService Usage Examples

This document provides practical examples of how to use the `ConfigurationService` for encrypting and storing sensitive data in LightJockey.

## Basic Usage

### Storing a Hue Bridge AppKey

```csharp
public class HueBridgeManager
{
    private readonly IConfigurationService _configService;
    private readonly IHueService _hueService;
    
    public HueBridgeManager(
        IConfigurationService configService,
        IHueService hueService)
    {
        _configService = configService;
        _hueService = hueService;
    }
    
    public async Task RegisterAndStoreAppKeyAsync(HueBridge bridge)
    {
        // Register with the Hue Bridge
        var result = await _hueService.RegisterAsync(
            bridge,
            "LightJockey",
            "Desktop");
        
        if (result.IsSuccess && !string.IsNullOrEmpty(result.AppKey))
        {
            // Store the AppKey securely
            var key = $"HueBridge_{bridge.BridgeId}_AppKey";
            await _configService.SetSecureValueAsync(key, result.AppKey);
            
            // Connect using the stored AppKey
            await _hueService.ConnectAsync(bridge, result.AppKey);
        }
    }
    
    public async Task<bool> ConnectToStoredBridgeAsync(HueBridge bridge)
    {
        // Retrieve the stored AppKey
        var key = $"HueBridge_{bridge.BridgeId}_AppKey";
        var appKey = await _configService.GetSecureValueAsync(key);
        
        if (string.IsNullOrEmpty(appKey))
        {
            // No stored AppKey, need to register
            return false;
        }
        
        // Connect using the stored AppKey
        return await _hueService.ConnectAsync(bridge, appKey);
    }
}
```

## ViewModel Integration

### MainWindowViewModel Example

```csharp
public class MainWindowViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IHueService _hueService;
    
    public MainWindowViewModel(
        IConfigurationService configService,
        IHueService hueService,
        // ... other services
    )
    {
        _configService = configService;
        _hueService = hueService;
    }
    
    private async Task LoadSavedBridgeConnectionAsync()
    {
        // Check if we have a saved bridge configuration
        var bridgeId = await _configService.GetSecureValueAsync("LastUsedBridgeId");
        var bridgeIp = await _configService.GetSecureValueAsync("LastUsedBridgeIp");
        var appKey = await _configService.GetSecureValueAsync($"HueBridge_{bridgeId}_AppKey");
        
        if (!string.IsNullOrEmpty(bridgeId) && 
            !string.IsNullOrEmpty(bridgeIp) && 
            !string.IsNullOrEmpty(appKey))
        {
            var bridge = new HueBridge
            {
                BridgeId = bridgeId,
                IpAddress = bridgeIp
            };
            
            var connected = await _hueService.ConnectAsync(bridge, appKey);
            
            if (connected)
            {
                IsHueConnected = true;
                // Update UI...
            }
        }
    }
    
    private async Task SaveBridgeConnectionAsync(HueBridge bridge, string appKey)
    {
        // Store bridge information securely
        await _configService.SetSecureValueAsync("LastUsedBridgeId", bridge.BridgeId);
        await _configService.SetSecureValueAsync("LastUsedBridgeIp", bridge.IpAddress);
        await _configService.SetSecureValueAsync($"HueBridge_{bridge.BridgeId}_AppKey", appKey);
    }
}
```

## Key Naming Convention

To ensure consistency and avoid conflicts, use these naming conventions:

```csharp
// Hue Bridge AppKeys
$"HueBridge_{bridgeId}_AppKey"

// Last used bridge
"LastUsedBridgeId"
"LastUsedBridgeIp"

// Future cloud integration
"CloudSync_ApiToken"
"CloudSync_Username"

// Other services
$"{ServiceName}_{InstanceId}_ApiKey"
```

## Testing with ConfigurationService

### Unit Test Example

```csharp
[Fact]
public async Task HueBridgeManager_ShouldStoreAndRetrieveAppKey()
{
    // Arrange
    var mockConfig = new Mock<IConfigurationService>();
    var storedKey = string.Empty;
    var storedValue = string.Empty;
    
    mockConfig
        .Setup(x => x.SetSecureValueAsync(It.IsAny<string>(), It.IsAny<string>()))
        .Callback<string, string>((k, v) => { storedKey = k; storedValue = v; })
        .ReturnsAsync(true);
    
    mockConfig
        .Setup(x => x.GetSecureValueAsync(It.IsAny<string>()))
        .ReturnsAsync(() => storedValue);
    
    var manager = new HueBridgeManager(mockConfig.Object, mockHueService.Object);
    
    // Act
    var bridge = new HueBridge { BridgeId = "abc123", IpAddress = "192.168.1.100" };
    var testAppKey = "test-app-key-12345";
    
    await mockConfig.Object.SetSecureValueAsync($"HueBridge_{bridge.BridgeId}_AppKey", testAppKey);
    var retrieved = await mockConfig.Object.GetSecureValueAsync($"HueBridge_{bridge.BridgeId}_AppKey");
    
    // Assert
    Assert.Equal(testAppKey, retrieved);
    Assert.Equal($"HueBridge_{bridge.BridgeId}_AppKey", storedKey);
}
```

## Error Handling

```csharp
public async Task<bool> TryGetAppKeyAsync(string bridgeId, out string? appKey)
{
    try
    {
        appKey = await _configService.GetSecureValueAsync($"HueBridge_{bridgeId}_AppKey");
        return !string.IsNullOrEmpty(appKey);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve AppKey for bridge {BridgeId}", bridgeId);
        appKey = null;
        return false;
    }
}
```

## Migration from Plain Text

If you have existing plain text configuration:

```csharp
public async Task MigrateToEncryptedStorageAsync()
{
    // Example: Migrate from old settings file
    var oldConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LightJockey",
        "settings.json");
    
    if (File.Exists(oldConfigPath))
    {
        var oldConfig = JsonSerializer.Deserialize<OldConfig>(
            await File.ReadAllTextAsync(oldConfigPath));
        
        if (oldConfig?.AppKey != null)
        {
            // Migrate to encrypted storage
            await _configService.SetSecureValueAsync(
                $"HueBridge_{oldConfig.BridgeId}_AppKey",
                oldConfig.AppKey);
            
            // Delete old plain text file
            File.Delete(oldConfigPath);
            
            _logger.LogInformation("Migrated configuration to encrypted storage");
        }
    }
}
```

## Best Practices

### 1. Always Use Secure Storage for Credentials

```csharp
// ❌ DON'T: Store in plain text
var appKey = "my-secret-key";
File.WriteAllText("config.txt", appKey);

// ✅ DO: Use ConfigurationService
await _configService.SetSecureValueAsync("AppKey", appKey);
```

### 2. Handle Missing Keys Gracefully

```csharp
var appKey = await _configService.GetSecureValueAsync("AppKey");
if (string.IsNullOrEmpty(appKey))
{
    // Prompt user to register/authenticate
    await ShowRegistrationDialogAsync();
}
```

### 3. Clean Up on Logout/Disconnect

```csharp
public async Task DisconnectAndForgetBridgeAsync(string bridgeId)
{
    await _configService.RemoveValueAsync($"HueBridge_{bridgeId}_AppKey");
    await _hueService.DisconnectAsync();
}
```

### 4. Use Dependency Injection

```csharp
// In App.xaml.cs or Startup
services.AddSingleton<IConfigurationService, ConfigurationService>();

// In your service/viewmodel constructor
public MyService(IConfigurationService configService)
{
    _configService = configService;
}
```

## Troubleshooting

### Issue: CryptographicException when retrieving values

**Cause**: Values encrypted by one Windows user cannot be decrypted by another.

**Solution**: Each user needs to register their own AppKey.

### Issue: Values not persisting after restart

**Cause**: Application might not have write permissions to AppData folder.

**Solution**: Ensure the application has proper permissions or check logs for errors.

### Issue: Multiple bridge support

**Solution**: Use unique keys for each bridge:

```csharp
var bridges = await _hueService.DiscoverBridgesAsync();

foreach (var bridge in bridges)
{
    var key = $"HueBridge_{bridge.BridgeId}_AppKey";
    var appKey = await _configService.GetSecureValueAsync(key);
    
    if (appKey != null)
    {
        // Bridge is already registered
        await _hueService.ConnectAsync(bridge, appKey);
    }
}
```

## See Also

- [Sicherheitskonzept.md](../Sicherheitskonzept.md) - Security architecture documentation
- [HueService_Examples.md](HueService_Examples.md) - Hue service usage examples
- Task 14 Documentation
