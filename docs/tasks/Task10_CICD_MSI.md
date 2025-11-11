# Task 10 — CI/CD & MSI Packaging

**Status**: ✅ Teilweise abgeschlossen  
**PR**: Task10_CICD_MSI  
**Date**: 2025-11-11

## Objective

Implement continuous integration and deployment pipeline with automated testing and MSI package creation for distribution.

## Implementation

### Phase 1: CI/CD Workflows (✅ Completed)

#### 1. Unit Tests Workflow

**File**: `.github/workflows/Unit-Tests.yml`

Automated unit testing on every push and pull request.

**Features**:
- Triggers on push to all branches
- Triggers on pull requests to main and develop
- Runs on Ubuntu latest (cross-platform testing)
- .NET 9.0 SDK setup
- NuGet package restoration
- Build verification
- Unit test execution with detailed reporting

**Workflow Steps**:
1. Checkout code
2. Setup .NET 9.0
3. Restore dependencies
4. Build project
5. Run all unit tests
6. Report test results

#### 2. Build and Release Workflow

**File**: `.github/workflows/build_and_release.yml`

Automated build verification on main branches.

**Features**:
- Triggers on push to main and develop branches
- Triggers on pull requests to main and develop
- Runs on Windows latest (native platform)
- .NET 9.0 SDK setup
- Release configuration build

**Workflow Steps**:
1. Checkout code
2. Setup .NET 9.0
3. Restore dependencies
4. Build in Release configuration

### Phase 2: MSI Packaging (⬜ Pending)

**Goal**: Create Windows Installer (MSI) package using WiX Toolset

#### Planned Implementation

**Tools Required**:
- WiX Toolset v4 or v5
- .NET SDK 9.0
- Windows SDK for signing tools

**Steps to Implement**:

1. **Install WiX Toolset**:
   ```bash
   dotnet tool install --global wix
   ```

2. **Create WiX Project**:
   - Add WiX project to solution
   - Configure product information (name, version, manufacturer)
   - Define installation directory
   - Include application files and dependencies
   - Create Start Menu shortcuts
   - Configure uninstall behavior

3. **Configure Package Metadata**:
   - Product Name: LightJockey
   - Manufacturer: [Your Name/Organization]
   - Version: Auto-increment from Git tags
   - UpgradeCode: Generate and persist
   - License Agreement
   - Product Icon

4. **File Harvesting**:
   - Harvest all binaries from Release build
   - Include dependencies (.NET Runtime if self-contained)
   - Include configuration files
   - Include documentation

5. **Create MSI Build Script**:
   ```xml
   <!-- Example WiX configuration structure -->
   <Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
     <Package Name="LightJockey" 
              Version="$(var.Version)"
              Manufacturer="[Publisher]"
              UpgradeCode="[GUID]">
       
       <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
       
       <Directory Id="TARGETDIR" Name="SourceDir">
         <Directory Id="ProgramFilesFolder">
           <Directory Id="INSTALLFOLDER" Name="LightJockey" />
         </Directory>
       </Directory>
       
       <ComponentGroup Id="ProductComponents">
         <!-- Application files -->
       </ComponentGroup>
     </Package>
   </Wix>
   ```

### Phase 3: Automated Versioning (⬜ Pending)

**Goal**: Automatically version builds from Git tags

**Implementation Strategy**:

1. **Version Number Format**: `MAJOR.MINOR.PATCH.BUILD`
   - MAJOR.MINOR.PATCH from Git tag (e.g., v1.0.0)
   - BUILD from GitHub run number

2. **Git Tag Convention**:
   - Format: `vMAJOR.MINOR.PATCH`
   - Example: `v1.0.0`, `v1.1.0`, `v2.0.0`

3. **Extract Version in CI**:
   ```yaml
   - name: Extract version from tag
     id: version
     run: |
       if [[ "${{ github.ref }}" == refs/tags/v* ]]; then
         VERSION=${GITHUB_REF#refs/tags/v}
       else
         VERSION="0.0.0"
       fi
       echo "VERSION=$VERSION" >> $GITHUB_OUTPUT
   ```

4. **Update Assembly Version**:
   - Modify .csproj during build
   - Set AssemblyVersion and FileVersion
   - Set PackageVersion for NuGet

### Phase 4: Release Artifacts (⬜ Pending)

**Goal**: Upload build artifacts for releases

**Implementation**:

1. **Artifact Types**:
   - MSI installer
   - Portable ZIP (no installer needed)
   - NuGet symbols package (for debugging)

2. **Upload to GitHub Release**:
   ```yaml
   - name: Create Release
     uses: actions/create-release@v1
     with:
       tag_name: ${{ github.ref }}
       release_name: Release ${{ github.ref }}
       draft: false
       prerelease: false
   
   - name: Upload MSI
     uses: actions/upload-release-asset@v1
     with:
       upload_url: ${{ steps.create_release.outputs.upload_url }}
       asset_path: ./artifacts/LightJockey-${{ steps.version.outputs.VERSION }}.msi
       asset_name: LightJockey-${{ steps.version.outputs.VERSION }}.msi
       asset_content_type: application/x-msi
   ```

### Phase 5: Code Signing Preparation (⬜ Pending)

**Goal**: Prepare for code signing (required for Windows Store)

**Note**: Actual signing will be implemented in Task 24 (Windows Store Deployment)

**Preparation Steps**:

1. **Certificate Options**:
   - **Option A**: Microsoft Store signing (recommended for store-only distribution)
   - **Option B**: Own EV Code Signing certificate (required for sideloading)
   
2. **GitHub Secrets to Configure** (when ready):
   - `SIGNING_CERTIFICATE`: Base64-encoded .pfx file
   - `SIGNING_PASSWORD`: Certificate password
   
3. **Signing Command** (example for future use):
   ```powershell
   signtool sign /fd SHA256 /a /f cert.pfx /p password /tr http://timestamp.digicert.com /td SHA256 LightJockey.msi
   ```

## Testing

### CI/CD Testing

**Current Tests** (✅ Completed):
- Automated unit test execution on every commit
- Build verification on Windows and Linux
- Pull request validation before merge

**Pending Tests**:
- MSI installation test on clean Windows VM
- Upgrade test from previous version
- Uninstallation cleanup verification
- Silent installation test

### Manual Testing Checklist (for MSI)

When MSI is implemented, test:

- [ ] Fresh installation on Windows 10
- [ ] Fresh installation on Windows 11
- [ ] Installation to custom directory
- [ ] Start Menu shortcut creation
- [ ] Desktop shortcut creation (if implemented)
- [ ] Uninstallation removes all files
- [ ] Uninstallation removes registry entries
- [ ] Upgrade from version X to X+1
- [ ] Side-by-side installation prevention
- [ ] Installation without admin rights (if user-only install)
- [ ] Installation with admin rights (if system-wide install)

## Current Status

### Completed ✅

1. **Unit Tests Workflow**:
   - Automated testing on every commit
   - Cross-platform build verification
   - Test result reporting

2. **Build Workflow**:
   - Automated Release builds
   - Windows-specific compilation
   - Dependency restoration

### In Progress 🔄

- MSI packaging configuration
- Automated versioning from Git tags
- Release artifact uploads

### Pending ⬜

- WiX Toolset integration
- MSI build in CI/CD
- Code signing preparation
- Release automation
- Installation testing

## Dependencies

- .NET 9.0 SDK
- GitHub Actions runners (Windows and Ubuntu)
- WiX Toolset (for MSI packaging)
- Windows SDK (for future signing)

## Next Steps

1. **Add WiX Project**:
   - Create WiX configuration file
   - Define installer structure
   - Configure product metadata

2. **Integrate MSI Build into CI/CD**:
   - Update build_and_release.yml
   - Add WiX build step
   - Upload MSI as artifact

3. **Implement Automated Versioning**:
   - Create version extraction script
   - Update .csproj during build
   - Tag releases appropriately

4. **Test MSI Locally**:
   - Build MSI on local machine
   - Test installation scenarios
   - Verify uninstallation

5. **Document Release Process**:
   - How to create a release
   - How to tag versions
   - How to verify release artifacts

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [WiX Toolset Documentation](https://wixtoolset.org/docs/)
- [.NET Application Publishing](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [SignTool Documentation](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool)

## Notes

- MSI packaging is prepared but not yet implemented
- Code signing will be fully addressed in Task 24 (Windows Store Deployment)
- Current CI/CD provides solid foundation for automated builds and tests
- For now, releases can be created manually by building in Release mode
