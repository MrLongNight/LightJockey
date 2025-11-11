# Task 0 — Projekt Setup & ADR

**Status**: ✅ Completed  
**PR**: Task0_ProjectSetup  
**Date**: 2025-11-11

## Objective

Set up the foundational project structure for LightJockey with MVVM architecture, documentation, and CI/CD integration.

## Implementation

### 1. Project Structure Created

- Solution file: `LightJockey.sln`
- Main WPF project: `src/LightJockey/LightJockey.csproj`
- Test project: `tests/LightJockey.Tests/LightJockey.Tests.csproj`

### 2. MVVM Folder Structure

```
src/LightJockey/
├── Models/            # Data models and entities
├── ViewModels/        # Presentation logic
│   └── ViewModelBase.cs
├── Views/             # XAML UI components
│   ├── MainWindow.xaml
│   └── MainWindow.xaml.cs
├── Services/          # Business logic services
├── Utilities/         # Helper classes
│   └── RelayCommand.cs
├── App.xaml
└── App.xaml.cs
```

### 3. Core Components

#### ViewModelBase
- Implements `INotifyPropertyChanged` for data binding
- Provides `SetProperty` helper method for property change notifications
- Base class for all ViewModels

#### RelayCommand
- Implementation of `ICommand` interface
- Supports command execution with optional can-execute predicate
- Used for binding user actions to ViewModel methods

#### App.xaml.cs
- Configured with Dependency Injection using `Microsoft.Extensions.DependencyInjection`
- Service registration in `ConfigureServices` method
- Proper disposal of service provider on application exit

### 4. Documentation

#### Architecture Decision Records (ADR)
- [ADR-0001: MVVM Architecture Pattern](../adr/adr-0001-mvvm-architecture.md)
- [ADR-0002: Dependency Injection](../adr/adr-0002-dependency-injection.md)
- [ADR-0003: WPF as UI Framework](../adr/adr-0003-wpf-framework.md)

#### README.md
- Project overview and features
- Architecture description
- Getting started guide
- Technology stack

### 5. Testing Infrastructure

#### Test Project Structure
```
tests/LightJockey.Tests/
├── ViewModels/
│   └── ViewModelBaseTests.cs
└── Utilities/
    └── RelayCommandTests.cs
```

#### Test Coverage
- ViewModelBase property change notification tests
- RelayCommand execution and can-execute tests
- Framework: xUnit with coverlet for code coverage

### 6. CI/CD

#### GitHub Actions Workflow (`.github/workflows/build.yml`)
- Triggers on push/PR to main and develop branches
- Runs on Windows (required for WPF)
- Steps:
  1. Checkout code
  2. Setup .NET 9.0 SDK
  3. Restore dependencies
  4. Build in Release configuration
  5. Run tests with code coverage
  6. Upload coverage reports (optional)

### 7. Configuration Files

- `.gitignore`: Standard .NET gitignore template
- `LightJockey.sln`: Solution file with both projects

## Technology Decisions

- **Framework**: .NET 9.0 with net9.0-windows target
- **UI**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern
- **DI**: Microsoft.Extensions.DependencyInjection
- **Testing**: xUnit with coverlet
- **CI/CD**: GitHub Actions

## Build Verification

To verify the setup:

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

All commands should complete successfully with no errors.

## Next Steps

The project structure is now ready for:
- Task 1: DI, Logging, Global Error Handling
- Implementation of AudioService
- FFT Processor and Beat Detector
- Philips Hue Integration

## Notes

- The project is Windows-only due to WPF framework requirements
- .NET 9.0 is used for latest features and performance improvements
- Unit tests demonstrate testing patterns for future development
- CI/CD is configured but may require secrets for coverage reports
