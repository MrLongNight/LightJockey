# Installer Build Process Documentation

This document outlines the configuration and steps required to successfully build the WiX-based MSI installer for the LightJockey application.

## Overview

The installer build is managed via a `.wixproj` project file and executed within a GitHub Actions workflow. The process relies on the WiX Toolset (v4) and specific extensions to handle the bootstrapper and registry searches.

## WiX Project Configuration (`installer/installer.wixproj`)

To correctly build the installer, the project file must reference the necessary WiX Toolset extensions. These are included via a dedicated `ItemGroup`.

```xml
<ItemGroup>
  <WixExtension Include="WixToolset.Bal.wixext" />
  <WixExtension Include="WixToolset.Util.wixext" />
</ItemGroup>
```

- **`WixToolset.Bal.wixext`**: Required for the bootstrapper application (`<bal:WixStandardBootstrapperApplication>`).
- **`WixToolset.Util.wixext`**: Required for utility functions, such as searching the registry (`<util:RegistrySearch>`).

## Source File Configuration (`installer/Bundle.wxs`)

The `Bundle.wxs` file must declare the `bal` and `util` XML namespaces in its root `<Wix>` element to use the extension elements correctly.

```xml
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs"
     xmlns:util="http://wixtoolset.org/schemas/v4/wxs/util"
     xmlns:bal="http://wixtoolset.org/schemas/v4/wxs/bal">
  ...
</Wix>
```

## GitHub Actions Workflow (`.github/workflows/RELEASE.01_Create-MSI_on-Push_AUTO.yml`)

The CI/CD workflow for creating the release installer requires specific setup steps to ensure the build environment is correctly configured.

### 1. WiX Toolset Installation

The WiX Toolset is not pre-installed on the standard GitHub Actions Windows runners. It must be installed using Chocolatey.

```yaml
- name: Install WiX Toolset
  run: choco install wix --version=4.0.4 -y
```

### 2. Adding WiX to the PATH

After installation, the WiX binaries must be added to the runner's `PATH` environment variable.

```yaml
- name: Add WiX to PATH
  run: echo "C:\Program Files (x86)\WiX Toolset v4.0\bin" | Out-File -FilePath $env:GITHUB_PATH -Encoding utf8 -Append
  shell: pwsh
```

### 3. Build Sequence

The workflow follows a standard .NET build sequence before attempting to build the installer:
1.  `dotnet restore`
2.  `dotnet build`
3.  `dotnet test`
4.  `dotnet publish`
5.  `dotnet build installer/installer.wixproj` (Builds the MSI installer)

This ensures that the application is fully built and tested before the installer packaging process begins.
