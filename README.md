# LightJockey

Audio-reactive lighting control application for Philips Hue Entertainment using C# and WPF.

## Overview

LightJockey is a Windows desktop application that synchronizes Philips Hue smart lights with audio input in real-time. The application analyzes audio streams using FFT (Fast Fourier Transform) and beat detection to create dynamic lighting effects that respond to music.

## Features

### Implemented ✅

- **Real-time Audio Analysis**: FFT-based audio processing with spectral analysis and beat detection
- **Philips Hue Integration**: 
  - HTTP/HTTPS API support for bridge discovery and authentication
  - Entertainment V2 (DTLS/UDP) for high-performance streaming (25-60 FPS)
- **Effect Engine**: Plugin-based architecture with slow (HTTPS) and fast (Entertainment V2) effects
- **Visual Feedback**: Real-time audio visualization with spectral display
- **Preset Management**: Save, load, import/export lighting configurations with auto-save
- **Modern UI**: 
  - WPF-based interface with MVVM architecture
  - Dark and Light theme support
  - Audio device selection
  - Hue bridge and light management
  - Real-time effect parameter adjustment
- **Performance Monitoring**: FPS and latency tracking
- **Comprehensive Testing**: 179+ unit tests with CI/CD integration

### Planned 📋

- **Additional Effects**: Rainbow, Strobe, Smooth Fade, and more
- **Enhanced Parameters**: Fine-tunable intensity, speed, and color variations
- **Multi-Zone Support**: Control multiple Entertainment Areas simultaneously
- **System Tray Integration**: Quick access and background operation
- **Keyboard Shortcuts**: Hotkeys for common actions
- **Auto-Update**: Automatic update checking and installation
- **Error Recovery**: Automatic reconnection and graceful degradation
- **Cloud Preset Sharing**: Import/export presets online
- **Security Enhancements**: Encrypted storage for sensitive data
- **Advanced Logging**: Detailed logs and metrics history
- **Theme Customization**: User-defined colors and persistent theme settings
- **Windows Store Deployment**: Professional MSI installer and Store distribution

## Architecture

This project follows the **MVVM (Model-View-ViewModel)** architectural pattern with dependency injection.

### Project Structure

```
LightJockey/
├── src/
│   └── LightJockey/           # Main WPF application
│       ├── Models/            # Data models
│       ├── ViewModels/        # Presentation logic
│       ├── Views/             # XAML UI components
│       ├── Services/          # Business logic services
│       └── Utilities/         # Helper classes
├── tests/
│   └── LightJockey.Tests/     # Unit tests
├── docs/
│   ├── adr/                   # Architecture Decision Records
│   └── tasks/                 # Task documentation
└── .github/
    └── workflows/             # CI/CD workflows
```

## Prerequisites

- .NET 9.0 SDK or later
- Windows 10/11
- Philips Hue Bridge (for testing with actual lights)

## Getting Started

### Build the Project

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run the Application

```bash
dotnet run --project src/LightJockey/LightJockey.csproj
```

## Development

### Architecture Decision Records

See [docs/adr/README.md](docs/adr/README.md) for important architectural decisions.

### Contributing

This project follows a task-based development approach. See [LIGHTJOCKEY_Entwicklungsplan.md](LIGHTJOCKEY_Entwicklungsplan.md) for the complete development plan.

## Technology Stack

- **Framework**: .NET 9.0
- **UI**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Logging**: Serilog
- **Audio**: NAudio 2.2.1
- **FFT**: MathNet.Numerics 5.0.0
- **Hue Integration**: HueApi 3.0.0, HueApi.Entertainment 3.0.0
- **Testing**: xUnit with Moq
- **CI/CD**: GitHub Actions

## License

TBD

## Status

✅ **MVP Completed** - Core functionality implemented (Tasks 0-9)

### Completed Tasks

- ✅ **Task 0**: Project Setup & ADR
- ✅ **Task 1**: DI, Logging, Global Error Handling
- ✅ **Task 2**: AudioService (Device enumeration, audio streaming)
- ✅ **Task 3**: FFTProcessor & BeatDetector (Audio analysis, beat detection)
- ✅ **Task 4**: HueService (HTTPS bridge discovery, authentication, light control)
- ✅ **Task 5**: Entertainment V2 (DTLS/UDP streaming, audio-reactive lighting)
- ✅ **Task 6**: EffectEngine (Plugin-based architecture with SlowHttpsEffect and FastEntertainmentEffect)
- ✅ **Task 7**: UI & Visualizer (MainWindow, AudioVisualizer, Dark/Light themes)
- ✅ **Task 8**: PresetService (Preset management with auto-save, import/export)
- ✅ **Task 9**: Tests, Performance & Metrics (179+ unit tests, performance tracking)

### In Progress / Planned

- 🔄 **Task 10**: CI/CD & MSI Packaging (Basic CI/CD done, MSI packaging pending)
- 📋 **Tasks 11-24**: Enhancement features and Windows Store deployment

See [LIGHTJOCKEY_Entwicklungsplan.md](LIGHTJOCKEY_Entwicklungsplan.md) for the complete development plan.