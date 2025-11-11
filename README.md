# LightJockey

[![Build](https://github.com/MrLongNight/LightJockey/actions/workflows/build.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/build.yml)
[![Unit Tests](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml)
[![codecov](https://codecov.io/gh/MrLongNight/LightJockey/branch/main/graph/badge.svg)](https://codecov.io/gh/MrLongNight/LightJockey)

Audio-reactive lighting control application for Philips Hue Entertainment using C# and WPF.

## Overview

LightJockey is a Windows desktop application that synchronizes Philips Hue smart lights with audio input in real-time. The application analyzes audio streams using FFT (Fast Fourier Transform) and beat detection to create dynamic lighting effects that respond to music.

## Features (Planned)

- **Audio Analysis**: Real-time audio processing with FFT and beat detection
- **Philips Hue Integration**: Support for both HTTPS API and Entertainment V2 (DTLS/UDP)
- **Effect Engine**: Multiple lighting effect modes (slow/fast)
- **Visual Feedback**: Real-time audio visualization
- **Preset Management**: Save and load lighting configurations
- **Modern UI**: WPF-based interface with dark/light themes

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

### Download and Install

Download the latest MSI installer from the [Releases](https://github.com/MrLongNight/LightJockey/releases) page.

**Requirements**:
- Windows 10/11 (x64)
- [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build from Source

#### Build the Project

```bash
dotnet restore
dotnet build
```

#### Run Tests

```bash
dotnet test
```

#### Run the Application

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

🚧 **In Development** - Implementing CI/CD & MSI Packaging

### Completed Tasks

- ✅ **Task 0**: Project Setup & ADR
- ✅ **Task 1**: DI, Logging, Global Error Handling
- ✅ **Task 2**: AudioService (Device enumeration, audio streaming)
- ✅ **Task 3**: FFTProcessor & BeatDetector (Audio analysis, beat detection)
- ✅ **Task 4**: HueService (HTTPS bridge discovery, authentication, light control)
- ✅ **Task 5**: Entertainment V2 (DTLS/UDP streaming, audio-reactive lighting)
- ✅ **Task 6**: EffectEngine (Plugin-based architecture with SlowHttpsEffect and FastEntertainmentEffect)
- ✅ **Task 7**: UI & Visualizer (MainWindow, AudioVisualizerControl, Dark/Light themes)
- ✅ **Task 8**: PresetService (Preset management with auto-save, import/export)
- ✅ **Task 9**: Tests & Performance (179+ unit tests, performance metrics, CI/CD integration)