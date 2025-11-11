using HueApi;
using HueApi.BridgeLocator;
using HueApi.ColorConverters;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueApi.Models.Requests;
using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Implementation of Hue service for managing Philips Hue bridges and lights via HTTPS
/// </summary>
public class HueService : IHueService
{
    private readonly ILogger<HueService> _logger;
    private LocalHueApi? _hueApi;
    private HueBridge? _connectedBridge;
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsConnected => _hueApi != null && _connectedBridge != null;

    /// <inheritdoc/>
    public HueBridge? ConnectedBridge => _connectedBridge;

    /// <summary>
    /// Initializes a new instance of the HueService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public HueService(ILogger<HueService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("HueService initialized");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HueBridge>> DiscoverBridgesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Discovering Hue bridges...");
            var bridges = new List<HueBridge>();

            // Try both HTTP and mDNS discovery for better coverage
            try
            {
                var httpLocator = new HttpBridgeLocator();
                var httpBridges = await httpLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
                
                foreach (var bridge in httpBridges)
                {
                    bridges.Add(new HueBridge
                    {
                        IpAddress = bridge.IpAddress ?? string.Empty,
                        BridgeId = bridge.BridgeId ?? string.Empty
                    });
                }
                
                var httpCount = httpBridges.Count();
                _logger.LogDebug("Found {BridgeCount} bridges via HTTP discovery", httpCount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HTTP bridge discovery failed");
            }

            // Try mDNS discovery as well
            try
            {
                var mdnsLocator = new MdnsBridgeLocator();
                var mdnsBridges = await mdnsLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
                
                foreach (var bridge in mdnsBridges)
                {
                    // Avoid duplicates
                    if (!bridges.Any(b => b.BridgeId == bridge.BridgeId))
                    {
                        bridges.Add(new HueBridge
                        {
                            IpAddress = bridge.IpAddress ?? string.Empty,
                            BridgeId = bridge.BridgeId ?? string.Empty
                        });
                    }
                }
                
                var mdnsCount = mdnsBridges.Count();
                _logger.LogDebug("Found {BridgeCount} additional bridges via mDNS discovery", mdnsCount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "mDNS bridge discovery failed");
            }

            var bridgeCount = bridges.Count;
            _logger.LogInformation("Discovered {BridgeCount} unique Hue bridges", bridgeCount);
            return bridges.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Hue bridges");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<HueAuthResult> RegisterAsync(
        HueBridge bridge,
        string appName,
        string deviceName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceName);

        try
        {
            _logger.LogDebug("Attempting to register with bridge at {IpAddress}", bridge.IpAddress);
            
            var registerResult = await LocalHueApi.RegisterAsync(
                bridge.IpAddress,
                appName,
                deviceName,
                generateClientKey: true);

            if (registerResult?.Username != null && !string.IsNullOrEmpty(registerResult.Username))
            {
                _logger.LogInformation("Successfully registered with bridge {BridgeId}", bridge.BridgeId);
                return new HueAuthResult
                {
                    IsSuccess = true,
                    AppKey = registerResult.Username,
                    RequiresLinkButton = false
                };
            }
            else
            {
                _logger.LogWarning("Registration failed - no username returned");
                return new HueAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Registration failed - no app key returned",
                    RequiresLinkButton = false
                };
            }
        }
        catch (HueApi.Models.Exceptions.LinkButtonNotPressedException)
        {
            _logger.LogDebug("Link button not pressed on bridge {BridgeId}", bridge.BridgeId);
            return new HueAuthResult
            {
                IsSuccess = false,
                ErrorMessage = "Please press the link button on the bridge and try again",
                RequiresLinkButton = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering with bridge {BridgeId}", bridge.BridgeId);
            return new HueAuthResult
            {
                IsSuccess = false,
                ErrorMessage = $"Registration failed: {ex.Message}",
                RequiresLinkButton = false
            };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ConnectAsync(HueBridge bridge, string appKey)
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);

        try
        {
            _logger.LogDebug("Connecting to bridge at {IpAddress}", bridge.IpAddress);
            
            _hueApi = new LocalHueApi(bridge.IpAddress, appKey);
            
            // Verify connection by trying to get bridge config
            var config = await _hueApi.GetBridgeAsync();
            
            if (config != null)
            {
                _connectedBridge = bridge;
                _logger.LogInformation("Successfully connected to bridge {BridgeId}", bridge.BridgeId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to verify bridge connection");
                _hueApi = null;
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to bridge {BridgeId}", bridge.BridgeId);
            _hueApi = null;
            _connectedBridge = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HueLight>> GetLightsAsync(CancellationToken cancellationToken = default)
    {
        if (_hueApi == null)
        {
            throw new InvalidOperationException("Not connected to a bridge. Call ConnectAsync first.");
        }

        try
        {
            _logger.LogDebug("Retrieving lights from bridge");
            
            var lightsResponse = await _hueApi.Light.GetAllAsync();
            var lights = new List<HueLight>();

            if (lightsResponse?.Data != null)
            {
                foreach (var light in lightsResponse.Data)
                {
                    lights.Add(new HueLight
                    {
                        Id = light.Id.ToString(),
                        Name = light.Metadata?.Name ?? "Unknown",
                        IsOn = light.On?.IsOn ?? false,
                        Brightness = (byte)((light.Dimming?.Brightness ?? 0) * 254.0 / 100.0),
                        SupportsColor = light.Color != null,
                        Type = light.Type?.ToString(),
                        IsReachable = true // API v2 doesn't have a direct reachable field
                    });
                }
            }

            var lightCount = lights.Count;
            _logger.LogInformation("Retrieved {LightCount} lights from bridge", lightCount);
            return lights.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lights from bridge");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetLightOnOffAsync(string lightId, bool isOn, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lightId);

        if (_hueApi == null)
        {
            throw new InvalidOperationException("Not connected to a bridge. Call ConnectAsync first.");
        }

        try
        {
            _logger.LogDebug("Setting light {LightId} to {State}", lightId, isOn ? "ON" : "OFF");
            
            var command = new UpdateLight();
            if (isOn)
            {
                command.TurnOn();
            }
            else
            {
                command.TurnOff();
            }

            await _hueApi.Light.UpdateAsync(Guid.Parse(lightId), command);
            _logger.LogInformation("Successfully set light {LightId} to {State}", lightId, isOn ? "ON" : "OFF");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting light {LightId} on/off state", lightId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetLightBrightnessAsync(string lightId, byte brightness, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lightId);

        if (_hueApi == null)
        {
            throw new InvalidOperationException("Not connected to a bridge. Call ConnectAsync first.");
        }

        try
        {
            _logger.LogDebug("Setting light {LightId} brightness to {Brightness}", lightId, brightness);
            
            // HueApi expects brightness as a percentage (0-100)
            double brightnessPercent = (brightness / 254.0) * 100.0;
            
            var command = new UpdateLight()
                .SetBrightness(brightnessPercent);

            await _hueApi.Light.UpdateAsync(Guid.Parse(lightId), command);
            _logger.LogInformation("Successfully set light {LightId} brightness to {Brightness}", lightId, brightness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting light {LightId} brightness", lightId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetLightColorAsync(string lightId, HueColor color, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lightId);
        ArgumentNullException.ThrowIfNull(color);

        if (_hueApi == null)
        {
            throw new InvalidOperationException("Not connected to a bridge. Call ConnectAsync first.");
        }

        try
        {
            _logger.LogDebug("Setting light {LightId} color to RGB({R},{G},{B})", 
                lightId, color.Red, color.Green, color.Blue);
            
            var rgbColor = new RGBColor(color.ToHexString());
            var command = new UpdateLight()
                .SetColor(rgbColor);

            await _hueApi.Light.UpdateAsync(Guid.Parse(lightId), command);
            _logger.LogInformation("Successfully set light {LightId} color", lightId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting light {LightId} color", lightId);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing HueService");
        _hueApi = null;
        _connectedBridge = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
