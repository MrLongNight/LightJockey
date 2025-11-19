LightJockey MSI Installer
Schnellstart
Der MSI Installer wird automatisch über GitHub Actions gebaut.

Manueller Build lokal (Windows)
# Voraussetzungen
# - .NET 9.0 SDK
# - WiX Toolset v5.0.0
# WiX installieren
dotnet tool install --global wix --version 5.0.0
# Anwendung publishen
dotnet publish ..\src\LightJockey\LightJockey.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained false `
  --output ..\publish
# AppComponents.wxs generieren (siehe PowerShell Script im Workflow)
# Siehe .github/workflows/Build-&-Release-MSI.yml Zeile 75-139
# MSI bauen
dotnet build installer.wixproj `
  --configuration Release `
  -p:Platform=x64 `
  -p:Version=1.0.0
# MSI ist hier: bin\Release\x64\LightJockey.msi

Dateien
installer.wixproj - MSBuild Projekt (WiX v5.0)
Product.wxs - Haupt-Installationsdefinition
AppComponents.wxs - Auto-generierte Komponenten (wird vom Build erstellt)
Verzeichnisstruktur nach Installation
C:\Program Files\LightJockey\
├── LightJockey.exe
├── *.dll
├── Resources\
│   ├── icon.ico
│   └── *.png
└── Themes\
    ├── DarkTheme.xaml
    └── LightTheme.xaml

Laufzeit-Daten (vom Benutzer)
Diese werden NICHT vom Installer erstellt:

C:\Users\[User]\AppData\Roaming\LightJockey\
├── config.json
├── settings.json
├── Presets\
└── Backups\

Details
Siehe MSI_INSTALLER_DOCUMENTATION.md für vollständige Dokumentation.

