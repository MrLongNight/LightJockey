# Task 10 — CI/CD & MSI Packaging

**Status**: ✅ Completed  
**PR**: Task10_CICD_MSI  
**Date**: 2025-11-11

## Overview

This task implements a comprehensive CI/CD pipeline using GitHub Actions for building, testing, and packaging LightJockey as an MSI installer. The pipeline ensures code quality, runs automated tests, and produces deployment-ready artifacts.

## Objectives

1. ✅ Create GitHub Actions workflow for CI/CD
2. ✅ Implement automated build process
3. ✅ Configure automated testing with coverage reports
4. ✅ Generate MSI installer package using WiX Toolset
5. ✅ Upload artifacts for distribution
6. ✅ Prepare for future Windows Store deployment

## Architecture

### CI/CD Pipeline Overview

```
┌─────────────────────────────────────────────────────────┐
│                   GitHub Actions Trigger                │
│        (Push to main/develop, PR, Tag, Manual)         │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│                      Build Job                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │ 1. Checkout code                                  │  │
│  │ 2. Setup .NET 9.0                                 │  │
│  │ 3. Restore NuGet packages                         │  │
│  │ 4. Build solution (Release)                       │  │
│  │ 5. Upload build artifacts                         │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│                      Test Job                           │
│  ┌──────────────────────────────────────────────────┐  │
│  │ 1. Checkout code                                  │  │
│  │ 2. Setup .NET 9.0                                 │  │
│  │ 3. Restore dependencies                           │  │
│  │ 4. Run unit tests                                 │  │
│  │ 5. Collect code coverage                          │  │
│  │ 6. Upload test results                            │  │
│  │ 7. Upload coverage to Codecov                     │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│                   Package MSI Job                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │ 1. Checkout code                                  │  │
│  │ 2. Setup .NET 9.0                                 │  │
│  │ 3. Extract version from git tags                  │  │
│  │ 4. Publish application (win-x64)                  │  │
│  │ 5. Install WiX Toolset v4                         │  │
│  │ 6. Generate WiX source (.wxs)                     │  │
│  │ 7. Build MSI installer                            │  │
│  │ 8. Upload MSI artifact                            │  │
│  │ 9. Create GitHub Release (on tags)                │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│                    Summary Job                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Generate build summary report                     │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Workflow File Structure

**Location**: `.github/workflows/ci-cd-msi.yml`

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`
- Git tags matching `v*` pattern
- Manual workflow dispatch

## Implementation Details

### 1. Build Job

**Platform**: Windows Latest  
**Purpose**: Compile the solution and verify build integrity

**Steps**:
1. Checks out the repository with full git history
2. Sets up .NET 9.0 SDK
3. Restores NuGet package dependencies
4. Builds the entire solution in Release configuration
5. Uploads build artifacts for downstream jobs

**Outputs**:
- Compiled binaries for main application
- Compiled binaries for test project
- Build logs

### 2. Test Job

**Platform**: Windows Latest  
**Purpose**: Execute automated tests and measure code coverage

**Steps**:
1. Restores and builds the solution
2. Runs all xUnit tests with code coverage collection
3. Generates test result files (TRX format)
4. Uploads test results as artifacts
5. Submits coverage reports to Codecov

**Outputs**:
- Test results (TRX format)
- Code coverage reports (Cobertura XML)
- Codecov integration

**Test Configuration**:
```bash
dotnet test --configuration Release \
  --verbosity normal \
  --collect:"XPlat Code Coverage" \
  --logger "trx;LogFileName=test-results.trx"
```

### 3. MSI Packaging Job

**Platform**: Windows Latest  
**Purpose**: Create Windows installer package

**Technology**: WiX Toolset v4 (Windows Installer XML)

**Steps**:

#### 3.1. Version Extraction
```powershell
$version = "1.0.0"  # Default
$tag = git describe --tags --abbrev=0
if ($tag -match '^v?(\d+\.\d+\.\d+)') {
  $version = $matches[1]
}
```

Extracts version from git tags (e.g., `v1.0.0` → `1.0.0`)

#### 3.2. Application Publishing
```bash
dotnet publish --configuration Release \
  --runtime win-x64 \
  --self-contained false \
  --output publish
```

Creates a framework-dependent deployment targeting Windows x64.

**Why framework-dependent?**
- Smaller installer size
- User must have .NET 9.0 Runtime installed
- Easier updates through .NET runtime updates
- Standard deployment approach

#### 3.3. WiX Installation
```bash
dotnet tool install --global wix
```

Installs WiX Toolset v4 globally via .NET tool

#### 3.4. WiX Source Generation

The workflow dynamically generates a `.wxs` file that includes:
- Product information (name, version, manufacturer)
- Upgrade GUID for proper upgrade handling
- All published application files
- Installation directory structure
- Major upgrade configuration

**Key WiX Features**:
- **UpgradeCode**: Persistent GUID for update detection
- **MajorUpgrade**: Handles installation upgrades
- **MediaTemplate**: Embeds files in single MSI
- **ProgramFiles64Folder**: Standard installation location
- **ComponentGroup**: Organized file components

#### 3.5. MSI Build
```bash
wix build -arch x64 -o "LightJockey-v{version}.msi" "installer\LightJockey.wxs"
```

Compiles the WiX source into a Windows Installer package.

**Output**: `LightJockey-v1.0.0.msi`

### 4. Artifact Management

#### Build Artifacts (Retention: 1 day)
- Purpose: Enable downstream jobs
- Contents: Compiled binaries
- Location: `build-output` artifact

#### Test Results (Retention: 7 days)
- Purpose: Test history and debugging
- Contents: TRX files, coverage reports
- Location: `test-results` artifact

#### MSI Installer (Retention: 90 days)
- Purpose: Distribution and deployment
- Contents: Windows installer package
- Location: `LightJockey-MSI` artifact

### 5. GitHub Releases

**Trigger**: Git tag push matching `v*`

**Process**:
1. Creates a new GitHub Release
2. Attaches the MSI installer
3. Uses tag name as release name
4. Marks as non-draft, non-prerelease

**Example**:
```bash
git tag v1.0.0
git push origin v1.0.0
# Triggers release creation with MSI attachment
```

## Project Configuration

### Assembly Information

The `.csproj` file includes metadata for MSI generation:

```xml
<PropertyGroup>
  <!-- Assembly Information -->
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <Product>LightJockey</Product>
  <Company>LightJockey</Company>
  <Copyright>Copyright © 2025 LightJockey</Copyright>
  <Description>Audio-reactive lighting control for Philips Hue</Description>
  <Authors>LightJockey</Authors>
</PropertyGroup>
```

Publishing configuration (runtime identifier, self-contained flag, etc.) is specified in the workflow's `dotnet publish` command rather than in the project file.

## Usage

### Manual Workflow Trigger

1. Navigate to **Actions** tab in GitHub
2. Select **CI/CD - Build, Test & MSI Package**
3. Click **Run workflow**
4. Select branch and run

### Automated Triggers

**On Push to main/develop**:
- Runs full pipeline
- Creates MSI artifact
- No release creation

**On Pull Request**:
- Runs build and test
- Validates MSI creation
- Provides feedback in PR

**On Version Tag**:
```bash
git tag v1.2.3
git push origin v1.2.3
```
- Runs full pipeline
- Creates MSI artifact
- **Creates GitHub Release** with MSI attached

## MSI Installer Details

### Installation Location
```
C:\Program Files\LightJockey\
```

### Included Files
- `LightJockey.exe` - Main application
- `LightJockey.dll` - Application library
- `*.dll` - All dependency libraries
- Configuration files (`.runtimeconfig.json`, `.deps.json`)

### User Requirements
- Windows 10/11 (x64)
- .NET 9.0 Desktop Runtime
- Administrator privileges for installation

### Upgrade Behavior
- Detects existing installations
- Removes old version automatically
- Installs new version
- Preserves user data (in `%APPDATA%`)

## Testing the MSI Package

### Local Build Test
```powershell
# Navigate to repository
cd LightJockey

# Publish application
dotnet publish src/LightJockey/LightJockey.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output publish

# Install WiX (if not installed)
dotnet tool install --global wix

# Build MSI (requires WiX source file)
wix build -arch x64 -o LightJockey.msi installer/LightJockey.wxs
```

### Installation Test
1. Download MSI from GitHub Actions artifacts
2. Right-click → Install
3. Follow installation wizard
4. Verify installation in Program Files
5. Launch application from Start Menu

### Upgrade Test
1. Install version 1.0.0
2. Build and install version 1.1.0
3. Verify automatic upgrade
4. Check no duplicate installations exist

## Future Enhancements

### Windows Store Deployment (Task 17)

**Preparation Complete**:
- ✅ MSI package generation
- ✅ Automated build pipeline
- ✅ Version management

**Remaining for Store**:
- Add code signing certificate
- Create MSIX package (Store requirement)
- Configure Store submission workflow
- Add Store metadata and screenshots

### Code Signing

For production release, the MSI should be signed:

```yaml
- name: Sign MSI
  run: |
    signtool sign /f certificate.pfx `
      /p ${{ secrets.CERT_PASSWORD }} `
      /t http://timestamp.digicert.com `
      LightJockey-v${{ steps.get_version.outputs.VERSION }}.msi
```

**Requirements**:
- Code signing certificate
- Certificate stored in GitHub Secrets
- SignTool from Windows SDK

## CI/CD Badges

Add to README.md:

```markdown
[![Build](https://github.com/MrLongNight/LightJockey/actions/workflows/ci-cd-msi.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/ci-cd-msi.yml)
[![Tests](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml/badge.svg)](https://github.com/MrLongNight/LightJockey/actions/workflows/Unit-Tests.yml)
[![codecov](https://codecov.io/gh/MrLongNight/LightJockey/branch/main/graph/badge.svg)](https://codecov.io/gh/MrLongNight/LightJockey)
```

## Troubleshooting

### Build Failures

**Issue**: NuGet restore fails  
**Solution**: Check package versions in `.csproj` files

**Issue**: WPF compilation errors  
**Solution**: Ensure Windows-latest runner is used

### Test Failures

**Issue**: Tests timeout  
**Solution**: Increase test timeout in workflow

**Issue**: Coverage upload fails  
**Solution**: Verify CODECOV_TOKEN secret is set

### MSI Creation Failures

**Issue**: WiX not found  
**Solution**: Verify `dotnet tool install --global wix` succeeds

**Issue**: File not found in publish directory  
**Solution**: Check publish output path matches WiX source

**Issue**: Invalid WiX XML  
**Solution**: Validate `.wxs` file syntax

## Metrics

### Pipeline Performance
- **Build Job**: ~2-3 minutes
- **Test Job**: ~2-4 minutes  
- **MSI Package Job**: ~3-5 minutes
- **Total Pipeline**: ~7-12 minutes

### Artifact Sizes
- **Build Output**: ~50-100 MB
- **Test Results**: ~1-5 MB
- **MSI Installer**: ~30-60 MB

## Validation Checklist

- [x] Workflow file created and validated
- [x] Build job succeeds on Windows
- [x] Test job runs all unit tests
- [x] Code coverage reports upload successfully
- [x] MSI package builds successfully
- [x] MSI artifact uploads to GitHub Actions
- [x] Version extraction from git tags works
- [x] GitHub Release creation on tag push
- [x] Assembly metadata configured in .csproj
- [x] Documentation complete

## Related Documentation

- [Task 0: Project Setup](Task0_ProjectSetup.md)
- [Task 1: DI & Logging](Task1_DI_Logging.md)
- [Task 9: Tests & Performance](Task9_Tests_Performance.md)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [WiX Toolset Documentation](https://wixtoolset.org/docs/)

## Summary

Task 10 successfully implements a complete CI/CD pipeline that:
- ✅ Builds the application on every push/PR
- ✅ Runs comprehensive unit tests
- ✅ Collects and reports code coverage
- ✅ Generates MSI installer packages
- ✅ Creates GitHub Releases for version tags
- ✅ Provides artifacts for manual distribution
- ✅ Prepares foundation for Windows Store deployment

The pipeline is production-ready and can be extended with code signing and Store submission in future tasks.
