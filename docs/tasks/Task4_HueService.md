# Task 4 — HueService (HTTPS)

**Status**: ✅ Completed  
**PR**: Task4_HueService  
**Date**: 2025-11-11

## Objective

Implement an HTTPS-based Philips Hue client that discovers bridges on the network, handles authentication/registration, and provides test LED control capabilities for integration with the LightJockey effect engine.

## Implementation

### 1. NuGet Packages Added

The following HueApi packages were added to the main project for Philips Hue bridge communication:

```xml
<PackageReference Include="HueApi" Version="3.0.0" />
<PackageReference Include="HueApi.ColorConverters" Version="3.0.0" />
```

**HueApi** provides:
- Bridge discovery via HTTP and mDNS
- Hue API v2 support with fluent interface
- Application registration and authentication
- Light resource management and control
- HTTPS communication with Philips Hue bridges

**HueApi.ColorConverters** provides:
- RGB color conversion utilities
- Extension methods for color operations
- Support for various color formats (RGB, HSB, etc.)

### 2. Models Created

#### HueBridge (`Models/HueBridge.cs`)

Represents a discovered Philips Hue bridge on the network.

**Properties:**
- `IpAddress` (string, required): IP address of the bridge
- `BridgeId` (string, required): Unique bridge identifier
- `Name` (string?, optional): Friendly name of the bridge
- `ModelId` (string?, optional): Bridge model identifier

#### HueLight (`Models/HueLight.cs`)

Represents a Hue light bulb or fixture.

**Properties:**
- `Id` (string, required): Unique light identifier (GUID)
- `Name` (string, required): Friendly name of the light
- `IsOn` (bool): Current on/off state
- `Brightness` (byte): Brightness level (0-254)
- `SupportsColor` (bool): Whether the light supports color
- `Type` (string?): Light type/archetype
- `IsReachable` (bool): Whether the light is reachable

#### HueAuthResult (`Models/HueAuthResult.cs`)

Represents the result of a bridge authentication attempt.

**Properties:**
- `IsSuccess` (bool): Whether authentication succeeded
- `AppKey` (string?): Application key (username) for API access
- `ErrorMessage` (string?): Error message if authentication failed
- `RequiresLinkButton` (bool): Whether link button needs to be pressed

#### HueColor (`Models/HueColor.cs`)

Represents an RGB color for Hue lights.

**Properties:**
- `Red` (byte): Red component (0-255)
- `Green` (byte): Green component (0-255)
- `Blue` (byte): Blue component (0-255)

**Methods:**
- `ToHexString()`: Converts to hex string (e.g., "FF0000")
- `FromHexString(string)`: Creates color from hex string
- Constructor overloads for flexible initialization

### 3. Hue Service Interface

#### IHueService (`Services/IHueService.cs`)

Service interface for managing Philips Hue bridges and lights via HTTPS.

**Methods:**

```csharp
Task<IReadOnlyList<HueBridge>> DiscoverBridgesAsync(CancellationToken cancellationToken = default);
```
- Discovers Philips Hue bridges on the local network
- Uses both HTTP (discovery.meethue.com) and mDNS for reliability
- Returns list of discovered bridges with IP and ID

```csharp
Task<HueAuthResult> RegisterAsync(
    HueBridge bridge,
    string appName,
    string deviceName,
    CancellationToken cancellationToken = default);
```
- Registers the application with a Hue bridge
- Requires user to press the link button on the bridge
- Returns authentication result with app key on success
- Returns link button requirement if button not pressed

```csharp
Task<bool> ConnectAsync(HueBridge bridge, string appKey);
```
- Connects to a bridge using a previously obtained app key
- Verifies connection by querying bridge configuration
- Updates `IsConnected` and `ConnectedBridge` properties

```csharp
Task<IReadOnlyList<HueLight>> GetLightsAsync(CancellationToken cancellationToken = default);
```
- Retrieves all lights from the connected bridge
- Returns light information including name, state, and capabilities
- Throws `InvalidOperationException` if not connected

```csharp
Task SetLightOnOffAsync(string lightId, bool isOn, CancellationToken cancellationToken = default);
```
- Turns a light on or off
- Uses Hue API v2 UpdateLight request

```csharp
Task SetLightBrightnessAsync(string lightId, byte brightness, CancellationToken cancellationToken = default);
```
- Sets light brightness (0-254 range)
- Converts to percentage for API compatibility

```csharp
Task SetLightColorAsync(string lightId, HueColor color, CancellationToken cancellationToken = default);
```
- Sets light color using RGB values
- Uses RGBColor converter for API compatibility

**Properties:**
- `IsConnected` (bool): Whether connected to a bridge
- `ConnectedBridge` (HueBridge?): Currently connected bridge

### 4. Hue Service Implementation

#### HueService (`Services/HueService.cs`)

Complete implementation of the Hue service using HueApi 3.0.0.

##### Bridge Discovery

**Discovery Methods:**
- **HTTP Discovery**: Queries `https://discovery.meethue.com` for registered bridges
- **mDNS Discovery**: Uses multicast DNS for local network bridge discovery
- **Fallback Strategy**: Tries both methods and combines results
- **Deduplication**: Removes duplicate bridges based on bridge ID

**Implementation Details:**
- 5-second timeout per discovery method
- Graceful error handling for each method
- Comprehensive logging at Debug and Information levels

##### Authentication Flow

**Registration Process:**
1. User calls `RegisterAsync()` with bridge and app details
2. Service calls `LocalHueApi.RegisterAsync()`
3. If link button not pressed: Returns `RequiresLinkButton = true`
4. If successful: Returns app key for future connections
5. App key should be stored securely for reuse

**Connection Process:**
1. User calls `ConnectAsync()` with bridge and stored app key
2. Service creates `LocalHueApi` instance
3. Verifies connection by querying bridge config
4. Updates connection state properties

##### Light Control

**Get Lights:**
- Uses `Light.GetAllAsync()` from HueApi 3.0.0
- Converts API light objects to LightJockey models
- Handles brightness conversion (API: 0-100%, Model: 0-254)

**Update Light State:**
- Uses `UpdateLight` fluent API for all updates
- **On/Off**: `.TurnOn()` or `.TurnOff()`
- **Brightness**: `.SetBrightness(percentage)`
- **Color**: `.SetColor(new RGBColor(hex))`
- All updates use `Light.UpdateAsync()` method

##### Error Handling

**Exception Types:**
- `ArgumentNullException`: For null parameters
- `ArgumentException`: For invalid parameters
- `InvalidOperationException`: For operations requiring connection
- `LinkButtonNotPressedException`: For authentication without link button press

**Logging:**
- Debug: Method entry, parameter details
- Information: Successful operations, counts
- Warning: Non-critical failures (e.g., one discovery method fails)
- Error: Critical failures with exception details

### 5. Testing

#### Unit Tests (`Tests/Services/HueServiceTests.cs`)

Comprehensive test coverage for all service functionality:

**Constructor Tests:**
- Validates null logger handling
- Verifies initialization logging

**Discovery Tests:**
- Tests bridge discovery returns proper list
- Validates bridge model properties

**Authentication Tests:**
- Tests parameter validation
- Validates registration flow

**Connection Tests:**
- Tests parameter validation
- Validates connection state management

**Light Control Tests:**
- Tests all parameter validations
- Validates "not connected" error handling
- Tests brightness, color, and on/off operations

**Model Tests:**
- `HueColor`: Hex conversion, round-trip, validation
- `HueBridge`: Property initialization
- `HueLight`: Property initialization
- `HueAuthResult`: Success and failure scenarios

**Test Statistics:**
- Total Tests: 40+
- Coverage: All public methods and edge cases
- Framework: xUnit with Moq for mocking

### 6. Architecture Integration

#### Dependency Injection

The service follows the established DI pattern:

```csharp
services.AddSingleton<IHueService, HueService>();
```

#### Logging

All operations are logged using `ILogger<HueService>`:
- Bridge discovery and counts
- Authentication attempts and results
- Connection state changes
- Light control operations

#### MVVM Compatibility

The service is designed for MVVM integration:
- Async methods for UI responsiveness
- Simple, testable interface
- Property notifications via `IsConnected` and `ConnectedBridge`

### 7. Usage Examples

#### Discover and Register

```csharp
var hueService = serviceProvider.GetRequiredService<IHueService>();

// Discover bridges
var bridges = await hueService.DiscoverBridgesAsync();
var bridge = bridges.FirstOrDefault();

if (bridge != null)
{
    // Register (user must press link button)
    var authResult = await hueService.RegisterAsync(
        bridge, 
        "LightJockey", 
        "MyComputer");
    
    if (authResult.RequiresLinkButton)
    {
        // Show message to press link button, then try again
    }
    else if (authResult.IsSuccess)
    {
        // Store authResult.AppKey securely for future use
    }
}
```

#### Connect and Control Lights

```csharp
// Connect with stored app key
var success = await hueService.ConnectAsync(bridge, storedAppKey);

if (success)
{
    // Get all lights
    var lights = await hueService.GetLightsAsync();
    var light = lights.First();
    
    // Turn on and set color to red
    await hueService.SetLightOnOffAsync(light.Id, true);
    await hueService.SetLightColorAsync(light.Id, new HueColor(255, 0, 0));
    
    // Set brightness
    await hueService.SetLightBrightnessAsync(light.Id, 200);
}
```

## Future Enhancements

### Planned for Task 5 (Entertainment V2)
- Entertainment area management
- DTLS/UDP streaming for high-speed light updates
- Synchronized multi-light effects
- Low-latency audio-reactive lighting

### Potential Improvements
- Light grouping and scenes
- Color temperature control
- Transition time configuration
- Entertainment mode activation
- State caching for offline resilience

## References

- **HueApi GitHub**: https://github.com/michielpost/Q42.HueApi
- **HueApi NuGet**: https://www.nuget.org/packages/HueApi/3.0.0
- **Philips Hue API v2**: https://developers.meethue.com/develop/hue-api-v2/
- **Discovery Documentation**: https://developers.meethue.com/develop/application-design-guidance/hue-bridge-discovery/

## Notes

- The service uses Hue API v2 exclusively (modern, recommended approach)
- Bridge discovery may take a few seconds depending on network conditions
- App keys should be stored securely (future task: encryption)
- Entertainment mode requires separate Entertainment API (Task 5)
- Testing requires actual Hue bridge hardware or emulator

## Completion Checklist

- [x] HueApi and HueApi.ColorConverters NuGet packages added
- [x] Models created (HueBridge, HueLight, HueAuthResult, HueColor)
- [x] IHueService interface defined
- [x] HueService implementation complete
- [x] Bridge discovery (HTTP + mDNS)
- [x] Authentication/registration flow
- [x] Light enumeration
- [x] Light control (on/off, brightness, color)
- [x] Comprehensive error handling
- [x] Full logging integration
- [x] Unit tests (40+ tests)
- [x] Documentation complete
- [x] Build successful with zero warnings
