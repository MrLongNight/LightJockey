# HueService Usage Examples

This document provides practical examples of how to use the HueService in the LightJockey application.

## Setup and Dependency Injection

First, register the HueService in your dependency injection container:

```csharp
// In App.xaml.cs or your DI configuration
services.AddSingleton<IHueService, HueService>();
```

## Example 1: Discover and Register a Bridge

This example shows the complete flow of discovering a bridge and registering your application.

```csharp
using LightJockey.Services;
using LightJockey.Models;

public class HueSetupViewModel
{
    private readonly IHueService _hueService;
    private readonly ILogger<HueSetupViewModel> _logger;
    
    public HueSetupViewModel(IHueService hueService, ILogger<HueSetupViewModel> logger)
    {
        _hueService = hueService;
        _logger = logger;
    }
    
    public async Task DiscoverAndRegisterAsync()
    {
        try
        {
            // Step 1: Discover bridges
            _logger.LogInformation("Discovering Hue bridges...");
            var bridges = await _hueService.DiscoverBridgesAsync();
            
            if (bridges.Count == 0)
            {
                _logger.LogWarning("No Hue bridges found on network");
                // Show message to user: "No bridges found. Check network connection."
                return;
            }
            
            // Step 2: Select first bridge (or let user choose)
            var bridge = bridges.First();
            _logger.LogInformation("Found bridge at {IpAddress}", bridge.IpAddress);
            
            // Step 3: Attempt registration
            _logger.LogInformation("Attempting to register with bridge...");
            var authResult = await _hueService.RegisterAsync(
                bridge,
                "LightJockey",
                Environment.MachineName);
            
            if (authResult.RequiresLinkButton)
            {
                // Show message to user: "Please press the link button on your Hue bridge and try again"
                _logger.LogInformation("Link button needs to be pressed");
                
                // In a real UI, you would:
                // 1. Show a dialog asking user to press the button
                // 2. Wait for user confirmation
                // 3. Retry registration
                
                // Retry after user presses button
                await Task.Delay(2000); // Wait for user
                authResult = await _hueService.RegisterAsync(
                    bridge,
                    "LightJockey",
                    Environment.MachineName);
            }
            
            if (authResult.IsSuccess && authResult.AppKey != null)
            {
                _logger.LogInformation("Successfully registered with bridge");
                
                // Step 4: Store the app key securely
                // TODO: In production, use secure storage (DPAPI, etc.)
                Properties.Settings.Default.HueBridgeIp = bridge.IpAddress;
                Properties.Settings.Default.HueBridgeId = bridge.BridgeId;
                Properties.Settings.Default.HueAppKey = authResult.AppKey;
                Properties.Settings.Default.Save();
                
                // Step 5: Connect using the new app key
                var connected = await _hueService.ConnectAsync(bridge, authResult.AppKey);
                
                if (connected)
                {
                    _logger.LogInformation("Connected to bridge successfully");
                    // Proceed to light selection
                }
            }
            else
            {
                _logger.LogError("Registration failed: {Error}", authResult.ErrorMessage);
                // Show error to user
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bridge discovery and registration");
            // Show error to user
        }
    }
}
```

## Example 2: Connect with Stored Credentials

Once you have registered and stored the app key, subsequent connections are simpler:

```csharp
public async Task ConnectToStoredBridgeAsync()
{
    try
    {
        // Load stored credentials
        var bridgeIp = Properties.Settings.Default.HueBridgeIp;
        var bridgeId = Properties.Settings.Default.HueBridgeId;
        var appKey = Properties.Settings.Default.HueAppKey;
        
        if (string.IsNullOrEmpty(bridgeIp) || string.IsNullOrEmpty(appKey))
        {
            _logger.LogWarning("No stored bridge credentials found");
            // Redirect to setup
            return;
        }
        
        var bridge = new HueBridge
        {
            IpAddress = bridgeIp,
            BridgeId = bridgeId
        };
        
        var connected = await _hueService.ConnectAsync(bridge, appKey);
        
        if (connected)
        {
            _logger.LogInformation("Connected to bridge at {IpAddress}", bridgeIp);
        }
        else
        {
            _logger.LogWarning("Failed to connect to bridge. Credentials may be invalid.");
            // Clear stored credentials and redirect to setup
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error connecting to stored bridge");
    }
}
```

## Example 3: Get and Display Lights

```csharp
public async Task LoadLightsAsync()
{
    try
    {
        if (!_hueService.IsConnected)
        {
            _logger.LogWarning("Cannot load lights: not connected to bridge");
            return;
        }
        
        var lights = await _hueService.GetLightsAsync();
        
        _logger.LogInformation("Retrieved {Count} lights", lights.Count);
        
        // Update UI with lights
        foreach (var light in lights)
        {
            _logger.LogDebug("Light: {Name} (ID: {Id}) - On: {IsOn}, Brightness: {Brightness}",
                light.Name, light.Id, light.IsOn, light.Brightness);
        }
        
        // In MVVM, update an ObservableCollection
        // Lights.Clear();
        // foreach (var light in lights)
        // {
        //     Lights.Add(new LightViewModel(light, _hueService));
        // }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading lights");
    }
}
```

## Example 4: Control a Light

### Turn Light On/Off

```csharp
public async Task ToggleLightAsync(string lightId, bool isOn)
{
    try
    {
        await _hueService.SetLightOnOffAsync(lightId, isOn);
        _logger.LogInformation("Set light {LightId} to {State}", lightId, isOn ? "ON" : "OFF");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error toggling light {LightId}", lightId);
    }
}
```

### Set Brightness

```csharp
public async Task SetBrightnessAsync(string lightId, byte brightness)
{
    try
    {
        // Brightness range: 0-254
        await _hueService.SetLightBrightnessAsync(lightId, brightness);
        _logger.LogInformation("Set light {LightId} brightness to {Brightness}", lightId, brightness);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error setting brightness for light {LightId}", lightId);
    }
}
```

### Set Color

```csharp
public async Task SetColorAsync(string lightId, byte red, byte green, byte blue)
{
    try
    {
        var color = new HueColor(red, green, blue);
        await _hueService.SetLightColorAsync(lightId, color);
        _logger.LogInformation("Set light {LightId} to color RGB({R},{G},{B})", 
            lightId, red, green, blue);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error setting color for light {LightId}", lightId);
    }
}

// Using hex color
public async Task SetColorFromHexAsync(string lightId, string hexColor)
{
    try
    {
        var color = HueColor.FromHexString(hexColor);
        await _hueService.SetLightColorAsync(lightId, color);
        _logger.LogInformation("Set light {LightId} to color {Hex}", lightId, hexColor);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error setting color for light {LightId}", lightId);
    }
}
```

## Example 5: Audio-Reactive Lighting

This example shows how to integrate HueService with AudioService for audio-reactive effects:

```csharp
public class AudioReactiveLightingService
{
    private readonly IHueService _hueService;
    private readonly ISpectralAnalyzer _spectralAnalyzer;
    private readonly ILogger<AudioReactiveLightingService> _logger;
    private string? _selectedLightId;
    
    public AudioReactiveLightingService(
        IHueService hueService,
        ISpectralAnalyzer spectralAnalyzer,
        ILogger<AudioReactiveLightingService> logger)
    {
        _hueService = hueService;
        _spectralAnalyzer = spectralAnalyzer;
        _logger = logger;
    }
    
    public void Start(string lightId)
    {
        _selectedLightId = lightId;
        _spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;
    }
    
    public void Stop()
    {
        _spectralAnalyzer.SpectralDataAvailable -= OnSpectralDataAvailable;
    }
    
    private async void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e)
    {
        if (_selectedLightId == null || !_hueService.IsConnected)
            return;
        
        try
        {
            // Map frequency bands to RGB colors
            // Low frequencies -> Red
            // Mid frequencies -> Green  
            // High frequencies -> Blue
            
            byte red = (byte)(e.LowEnergy * 255);
            byte green = (byte)(e.MidEnergy * 255);
            byte blue = (byte)(e.HighEnergy * 255);
            
            var color = new HueColor(red, green, blue);
            
            // Update light color based on audio
            await _hueService.SetLightColorAsync(_selectedLightId, color);
            
            // Adjust brightness based on overall energy
            var overallEnergy = (e.LowEnergy + e.MidEnergy + e.HighEnergy) / 3.0;
            byte brightness = (byte)(overallEnergy * 254);
            await _hueService.SetLightBrightnessAsync(_selectedLightId, brightness);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating light in audio-reactive mode");
        }
    }
}
```

## Example 6: MVVM ViewModel Integration

```csharp
public class LightControlViewModel : ViewModelBase
{
    private readonly IHueService _hueService;
    private ObservableCollection<HueLight> _lights;
    private HueLight? _selectedLight;
    private byte _brightness;
    
    public ObservableCollection<HueLight> Lights
    {
        get => _lights;
        set => SetProperty(ref _lights, value);
    }
    
    public HueLight? SelectedLight
    {
        get => _selectedLight;
        set
        {
            if (SetProperty(ref _selectedLight, value))
            {
                OnPropertyChanged(nameof(IsLightSelected));
                if (value != null)
                {
                    Brightness = value.Brightness;
                }
            }
        }
    }
    
    public byte Brightness
    {
        get => _brightness;
        set
        {
            if (SetProperty(ref _brightness, value) && SelectedLight != null)
            {
                _ = UpdateBrightnessAsync();
            }
        }
    }
    
    public bool IsLightSelected => SelectedLight != null;
    
    public ICommand RefreshLightsCommand { get; }
    public ICommand ToggleLightCommand { get; }
    
    public LightControlViewModel(IHueService hueService)
    {
        _hueService = hueService;
        _lights = new ObservableCollection<HueLight>();
        
        RefreshLightsCommand = new AsyncRelayCommand(RefreshLightsAsync);
        ToggleLightCommand = new AsyncRelayCommand(ToggleLightAsync, () => IsLightSelected);
    }
    
    private async Task RefreshLightsAsync()
    {
        try
        {
            var lights = await _hueService.GetLightsAsync();
            Lights.Clear();
            foreach (var light in lights)
            {
                Lights.Add(light);
            }
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
    
    private async Task ToggleLightAsync()
    {
        if (SelectedLight == null) return;
        
        try
        {
            bool newState = !SelectedLight.IsOn;
            await _hueService.SetLightOnOffAsync(SelectedLight.Id, newState);
            SelectedLight.IsOn = newState;
            OnPropertyChanged(nameof(SelectedLight));
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }
    
    private async Task UpdateBrightnessAsync()
    {
        if (SelectedLight == null) return;
        
        try
        {
            await _hueService.SetLightBrightnessAsync(SelectedLight.Id, Brightness);
        }
        catch (Exception ex)
        {
            // Handle error (but don't spam on slider changes)
        }
    }
}
```

## Best Practices

### 1. Store Credentials Securely

```csharp
// DO NOT store in plain text in production!
// Use Windows DPAPI or other secure storage:
var protectedKey = ProtectedData.Protect(
    Encoding.UTF8.GetBytes(appKey),
    null,
    DataProtectionScope.CurrentUser);
```

### 2. Handle Connection Loss

```csharp
// Periodically check connection
private async Task<bool> EnsureConnectedAsync()
{
    if (!_hueService.IsConnected)
    {
        await ConnectToStoredBridgeAsync();
    }
    return _hueService.IsConnected;
}
```

### 3. Rate Limiting for Animations

```csharp
// Don't update lights too frequently (HTTPS has latency)
// Recommended: 10-20 updates per second maximum
private DateTime _lastUpdate = DateTime.MinValue;

private async Task UpdateLightIfNeededAsync(string lightId, HueColor color)
{
    var now = DateTime.UtcNow;
    if ((now - _lastUpdate).TotalMilliseconds < 50) // 20 Hz max
        return;
        
    _lastUpdate = now;
    await _hueService.SetLightColorAsync(lightId, color);
}
```

### 4. Error Handling

```csharp
// Always handle network errors gracefully
try
{
    await _hueService.SetLightColorAsync(lightId, color);
}
catch (HttpRequestException ex)
{
    _logger.LogWarning("Network error updating light: {Message}", ex.Message);
    // Don't crash the app - just log and continue
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error updating light");
}
```

## Integration with Task 5 (Entertainment Mode)

The current HueService provides HTTPS-based control suitable for:
- Setup and configuration
- Slow/ambient effects
- Testing and debugging

For high-speed, low-latency effects (Task 5), you'll use Entertainment API v2:
- DTLS/UDP streaming
- 25+ updates per second
- Synchronized multi-light effects
- Direct entertainment area control

The HueService authentication (app key) will be used to initialize Entertainment mode.
