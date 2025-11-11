# GitHub Actions Workflows

This directory contains automated CI/CD workflows for the LightJockey project.

## Available Workflows

### 1. CI/CD - Build, Test & MSI Package
**File**: `ci-cd-msi.yml`  
**Status**: [![Build & Package](https://github.com/MrLongNight/LightJockey/actions/workflows/ci-cd-msi.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/ci-cd-msi.yml)

**Purpose**: Complete CI/CD pipeline for building, testing, and packaging the application.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`
- Version tags matching `v*` pattern (e.g., `v1.0.0`)
- Manual workflow dispatch

**Jobs**:
1. **Build** - Compiles the solution in Release configuration
2. **Test** - Runs unit tests with code coverage
3. **Package MSI** - Creates Windows installer using WiX Toolset
4. **Summary** - Generates pipeline summary report

**Artifacts**:
- Build output (1-day retention)
- Test results (7-day retention)
- MSI installer package (90-day retention)

**Creating a Release**:
```bash
git tag v1.0.0
git push origin v1.0.0
```
This will:
- Trigger the full CI/CD pipeline
- Build and test the application
- Create an MSI installer
- Create a GitHub Release with the MSI attached

### 2. Unit Tests
**File**: `Unit-Tests.yml`  
**Status**: [![Unit Tests](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml)

**Purpose**: Run unit tests and upload coverage to Codecov.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`

### 3. Build and Release
**File**: `build_and_release.yml`

**Purpose**: Basic build verification workflow.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`

## Running Workflows Manually

1. Navigate to the **Actions** tab in GitHub
2. Select the workflow you want to run
3. Click **Run workflow**
4. Choose the branch
5. Click **Run workflow** button

## Workflow Requirements

### Secrets
The following secrets must be configured in GitHub repository settings:

- `CODECOV_TOKEN` - Token for uploading code coverage to Codecov (optional but recommended)
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions (no configuration needed)

### Required Software (CI Environment)
- Windows runner (windows-latest)
- .NET 9.0 SDK
- WiX Toolset v4 (installed automatically)

## Local Development

To test the build and package locally:

```powershell
# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release

# Publish
dotnet publish src/LightJockey/LightJockey.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output publish

# Install WiX (one-time)
dotnet tool install --global wix

# Build MSI (requires WiX source file)
# See docs/tasks/Task10_CICD_MSI.md for details
```

## Troubleshooting

### Build Failures
- Check that all NuGet packages are available
- Verify .NET 9.0 SDK is installed
- Review build logs in the Actions tab

### Test Failures
- Tests require Windows environment
- Some tests may require specific hardware (audio devices)
- Check test logs for specific failure reasons

### MSI Package Failures
- Ensure WiX Toolset installation succeeded
- Verify publish directory contains all required files
- Check WiX source file (.wxs) syntax

## Documentation

For detailed information about the CI/CD pipeline and MSI packaging:
- [Task 10: CI/CD & MSI Packaging](../docs/tasks/Task10_CICD_MSI.md)

## Contributing

When adding new workflows:
1. Test locally if possible
2. Use descriptive names
3. Add appropriate triggers
4. Document the workflow purpose
5. Update this README
