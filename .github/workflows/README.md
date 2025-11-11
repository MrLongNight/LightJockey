# GitHub Actions Workflows

This directory contains automated CI/CD workflows for the LightJockey project.

## Workflow Structure

### 1. Unit Tests (Automatic)
**File**: `Unit-Tests.yml`  
**Status**: [![Unit Tests](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml)

**Purpose**: Run unit tests automatically and upload coverage to Codecov.

**Triggers**:
- ✅ Automatic on push to `main` or `develop`
- ✅ Automatic on pull requests to `main` or `develop`

**What it does**:
- Builds the solution
- Runs all unit tests (179+ tests)
- Collects code coverage
- Uploads coverage to Codecov

### 2. Build (Automatic)
**File**: `build.yml`  
**Status**: [![Build](https://github.com/MrLongNight/LightJockey/actions/workflows/build.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/build.yml)

**Purpose**: Verify that the solution builds successfully.

**Triggers**:
- ✅ Automatic on push to `main` or `develop`
- ✅ Automatic on pull requests to `main` or `develop`

**What it does**:
- Restores NuGet packages
- Builds the solution in Release configuration
- Uploads build artifacts (1-day retention)

### 3. Release MSI Package (Manual)
**File**: `release-msi.yml`  
**Status**: [![Release MSI](https://github.com/MrLongNight/LightJockey/actions/workflows/release-msi.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/release-msi.yml)

**Purpose**: Create MSI installer package for distribution.

**Triggers**:
- 🔧 Manual workflow dispatch (with optional version input)
- ✅ Automatic on version tags (e.g., `v1.0.0`)

**What it does**:
- Builds the solution
- Runs tests to ensure quality
- Publishes the application
- Creates MSI installer using WiX Toolset v4
- Uploads MSI artifact (90-day retention)
- Creates GitHub Release (when triggered by tag)

## Usage

### Running Tests (Automatic)
Tests run automatically on every push and PR. No action needed.

### Building (Automatic)
Builds run automatically on every push and PR. No action needed.

### Creating a Release Package (Manual)

**Option 1: Manual trigger with custom version**
1. Go to **Actions** → **Release MSI Package**
2. Click **Run workflow**
3. Enter version number (e.g., `1.0.0`) or leave default
4. Click **Run workflow**

**Option 2: Tag-based release**
```bash
git tag v1.0.0
git push origin v1.0.0
```
This will:
- Trigger MSI package creation
- Create a GitHub Release
- Attach the MSI installer to the release

## Workflow Requirements

### Secrets
- `CODECOV_TOKEN` - For code coverage uploads (optional but recommended)
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions

### CI Environment
- Windows runner (windows-latest)
- .NET 9.0 SDK
- WiX Toolset v4 (installed automatically)

## Local Development

```powershell
# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release

# Publish (for MSI packaging)
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
- Verify .NET 9.0 SDK is installed
- Check NuGet package availability
- Review build logs in Actions tab

### Test Failures
- Tests require Windows environment
- Check test logs for specific failures

### MSI Package Failures
- Ensure WiX Toolset installation succeeded
- Verify publish directory contains all required files
- Check WiX source file (.wxs) syntax

## Documentation

For detailed information:
- [Task 10: CI/CD & MSI Packaging](../../docs/tasks/Task10_CICD_MSI.md)

## Summary

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| Unit Tests | Automatic (push/PR) | Run tests & coverage |
| Build | Automatic (push/PR) | Verify build success |
| Release MSI | Manual / Tags | Create installer package |

This structure provides:
- ✅ Automatic quality checks on every change
- ✅ Manual control over release packaging
- ✅ Clear separation of concerns
