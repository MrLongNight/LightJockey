LIGHTJOCKEY — Entwicklungsplan v5.0 + KI-Coding-Prompts

Dokument-ID: LIGHTJOCKEY_DEVELOPMENT_PLAN_v5.0.md
Projekt: LightJockey (C# / WPF)
Datum: 2025-11-11
Status: Final (MVP + Erweiterungen)


---

1. Vorprojekt-Checkliste (bereitstellen)

1. GitHub Repository + Rechte konfigurieren


2. App Logo & Icons


3. Dark/Light Theme Palette + Fonts


4. Test-Hue-Bridge(s)


5. Test-Audiofiles / Musikdateien


6. Windows CI/CD Runner Zugriff




---

2. MVP Tasks inkl. KI-Coding-Prompts

Task 0 — Projekt Setup & ADR

Beschreibung: Projektstruktur erstellen, MVVM-Ordnerstruktur anlegen, Architekturentscheidungen dokumentieren (ADR).
PR-Bezeichnung: Task0_ProjectSetup
KI-Prompt:

Erstelle ein C# WPF MVVM Projekt für LightJockey.
- Ordnerstruktur: Models, ViewModels, Views, Services, Utilities, Docs
- Projekt soll CI/CD-ready sein (GitHub Actions)
- Erstelle initiale ADR-Dateien und README
- Füge Templates für Unit-Tests hinzu
- Dokumentiere alles in Markdown
- Prüfe Build und Code Style


---

Task 1 — DI, Logging, Global Error Handling

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Dependency Injection einrichten, Logging via Serilog, globales Error Handling implementieren.
PR-Bezeichnung: Task1_DI_Logging
KI-Prompt:

Implementiere:
- ✅ Dependency Injection mit Microsoft.Extensions.DependencyInjection
- ✅ Serilog Logging für alle Services
- ✅ Global Error Handling (try/catch + Logging)
- ✅ Dokumentation in /docs/tasks/Task1_DI_Logging.md
- ✅ Unit-Test Beispiel für Error Handling
- ✅ GitHub Actions Build Test integriert

Ergebnis:
- Serilog mit Console und File Sinks konfiguriert
- Globale Exception Handler für UI und Background Threads implementiert
- Beispiel-Service (ExampleService) mit Logging demonstriert
- Umfassende Unit-Tests mit Moq für Error Handling
- Dokumentation erstellt in docs/tasks/Task1_DI_Logging.md


---

Task 2 — AudioService

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Audio-Geräteauswahl, Loopback, Input Stream bereitstellen.
PR-Bezeichnung: Task2_AudioService
KI-Prompt:

Erstelle AudioService:
- ✅ Listet verfügbare Audio-Devices
- ✅ Ermöglicht Auswahl per UI
- ✅ Stream-Loopback für Effektberechnung
- ✅ FFT-kompatibles Output-Event
- ✅ Dokumentation + Beispielcode
- ✅ CI/CD Build Check
- ✅ Unit-Test: Device Auswahl, Stream Events

Ergebnis:
- NAudio 2.2.1 für Windows-Audio-APIs integriert
- AudioDevice und AudioDataEventArgs Models erstellt
- IAudioService Interface mit Device-Enumeration und Streaming
- AudioService Implementation mit WASAPI Loopback und WaveIn
- FFT-kompatible float[] Samples mit Event-basierter Bereitstellung
- Umfassende Unit-Tests für alle Funktionen
- Dokumentation erstellt in docs/tasks/Task2_AudioService.md


---

Task 3 — FFTProcessor & BeatDetector

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Audioanalyse: FFT, Spectral Analysis, Beat Detection.
PR-Bezeichnung: Task3_FFT_BeatDetector
KI-Prompt:

Implementiere:
- ✅ FFTProcessor für AudioStream
- ✅ BeatDetector (BPM-Erkennung, Peak Detection)
- ✅ SpectralAnalyzer (Low/Mid/High Frequenzbänder)
- ✅ Events für EffectEngine
- ✅ Dokumentation inkl. Diagramme
- ✅ Unit-Tests: Signalanalysen korrekt (41 Tests)

Ergebnis:
- MathNet.Numerics 5.0.0 für FFT-Verarbeitung integriert
- FFTResultEventArgs, SpectralDataEventArgs, BeatDetectedEventArgs Models erstellt
- IFFTProcessor Interface und FFTProcessor Implementation mit Hann-Windowing
- ISpectralAnalyzer Interface und SpectralAnalyzer für Frequenzbandanalyse (Low/Mid/High)
- IBeatDetector Interface und BeatDetector mit BPM-Erkennung und Peak Detection
- Event-basierte Architektur für Integration mit EffectEngine
- Umfassende Unit-Tests für alle Komponenten
- Vollständige Dokumentation mit Architekturdiagrammen in docs/tasks/Task3_FFT_BeatDetector.md


---

Task 4 — HueService (HTTPS)

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: HTTP-basierter Philips Hue Client + Bridge Setup UI.
PR-Bezeichnung: Task4_HueService
KI-Prompt:

Implementiere:
- ✅ HueService HTTPS Client auf Basis von https://www.nuget.org/packages/HueApi
- ✅ Bridge Discovery + UI Setup
- ✅ Authentifizierung
- ✅ Test-LED Steuerung
- ✅ Dokumentation / Screenshots
- ✅ Unit-Test: Verbindung + LED Update

Ergebnis:
- HueApi 3.0.0 und HueApi.ColorConverters 3.0.0 integriert
- HueBridge, HueLight, HueAuthResult, HueColor Models erstellt
- IHueService Interface mit Bridge Discovery, Auth, Light Control
- HueService Implementation mit HTTP und mDNS Discovery
- Fluent API für Light Control (On/Off, Brightness, Color)
- 40+ Unit-Tests für alle Komponenten und Models
- Vollständige Dokumentation in docs/tasks/Task4_HueService.md


---

Task 5 — Entertainment V2 (DTLS/UDP)

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: UDP/DTLS Client für Entertainment V2 Lichteffekte.
PR-Bezeichnung: Task5_EntertainmentV2
KI-Prompt:

Erstelle EntertainmentV2 Service:
- ✅ Umsetzung mit https://www.nuget.org/packages/HueApi.Entertainment
- ✅ DTLS/UDP Verbindung aufbauen
- ✅ Paketformat wie Philips Hue Entertainment V2
- ✅ Ereignisse aus AudioService zur Lichteffektsteuerung
- ✅ Dokumentation + Tests für Paket-Transportsicherheit
- ✅ CI/CD Build Check

Ergebnis:
- HueApi.Entertainment 3.0.0 für DTLS/UDP Streaming integriert
- Portable.BouncyCastle 1.9.0 für DTLS-Verschlüsselung
- EntertainmentArea, LightChannel, LightJockeyEntertainmentConfig Models erstellt
- IEntertainmentService Interface mit Events für Streaming-Status
- EntertainmentService Implementation mit:
  - DTLS/UDP Verbindung über StreamingHueClient
  - Entertainment Area Discovery und Initialisierung
  - High-Performance Streaming Loop (25-60 FPS)
  - Audio-reaktive Beleuchtung mit konfigurierbarer Sensitivität
  - Thread-sichere Channel-Updates
  - Automatisches Frame-Rate Limiting
- 20+ Unit-Tests für Service, Models und Integration
- Vollständige Dokumentation in docs/tasks/Task5_EntertainmentV2.md



---

Task 6 — EffectEngine (MVP modes)

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Implementierung der langsamen und schnellen Lichteffekte.
PR-Bezeichnung: Task6_EffectEngine
KI-Prompt:

EffectEngine:
- ✅ MVP Modi: langsamer HTTPS Effekt, schneller EntertainmentV2 Effekt
- ✅ Event-Handler für AudioService
- ✅ Config Bindings für Effektparameter
- ✅ Dokumentation mit Flowchart
- ✅ Unit-Test: Effekt-Ausgabe korrekt

Ergebnis:
- Plugin-basierte Architektur mit IEffectPlugin Interface
- EffectConfig und EffectState Models erstellt
- SlowHttpsEffect für HTTPS-basierte Effekte (2-5 Updates/Sekunde)
- FastEntertainmentEffect für Entertainment V2 (25-60 FPS)
- Event-basierte Integration mit AudioService
- 28 umfassende Unit-Tests
- Vollständige Dokumentation in docs/tasks/Task6_EffectEngine.md


---

Task 7 — UI (MainWindow) + Visualizer + Controls

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Visualisierung der Effekte, Steuerung von Parametern, Device Auswahl.
PR-Bezeichnung: Task7_UI_Visualizer-MainWindow
KI-Prompt:

MainWindow UI:
- ✅ Device Selection, Effect Selection, Parameter Sliders
- ✅ Visualizer für AudioInput
- ✅ Dark/Light Mode implementiert
- ✅ MVVM Bindings korrekt
- ✅ Dokumentation Screenshots
- ✅ UI Unit-Tests: Bindings, Commands

Ergebnis:
- MainWindowViewModel mit vollständigem MVVM-Support implementiert
- AudioVisualizerControl für Echtzeit-Spektralvisualisierung erstellt
- Dark und Light Theme ResourceDictionaries implementiert
- Umfassendes UI-Layout in MainWindow.xaml mit allen Controls
- Theme-Switching-Funktionalität hinzugefügt
- 17 Unit-Tests für MainWindowViewModel erstellt
- Vollständige Dokumentation in docs/tasks/Task7_UI_Visualizer.md


---

Task 8 — Preset/Configuration Service

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Automatisches Speichern, manuelles Import/Export.
PR-Bezeichnung: Task8_PresetService
KI-Prompt:

PresetService:
- ✅ Persistiert Konfiguration automatisch
- ✅ Import/Export JSON
- ✅ EffectEngine + UI Bindings
- ✅ Dokumentation + Beispiel Presets
- ✅ Unit-Test: Speichern/Laden korrekt

Ergebnis:
- Preset und PresetCollection Models als Record Types für Immutability
- IPresetService Interface mit 12 Methoden und 4 Events
- PresetService Implementation mit:
  - Automatische Persistenz nach %APPDATA%\LightJockey\Presets
  - Auto-Save bei Effect-Änderungen (standardmäßig aktiviert)
  - Import/Export einzelner oder mehrerer Presets
  - Event-basierte Architektur für UI-Integration
  - Vollständige EffectEngine Integration
- 4 Beispiel-Presets (High Energy Party, Ambient Relaxation, Balanced Default, Cinema Mode)
- 33 umfassende Unit-Tests mit vollständiger Abdeckung
- Vollständige Dokumentation in docs/tasks/Task8_PresetService.md


---

Task 9 — Tests, Performance, Metrics

**✅ ABGESCHLOSSEN** - 2025-11-11

Beschreibung: Unit/Integration Tests, Performance Checks.
PR-Bezeichnung: Task9_Tests_Performance-Metrics
KI-Prompt:

Implementiere:
- ✅ Unit Tests für AudioService, EffectEngine, HueService
- ✅ Performance Metrics (FPS, Latency)
- ✅ CI/CD Build Test mit Coverage Report
- ✅ Dokumentation: Test Report

Ergebnis:
- PerformanceMetricsService mit FPS und Latency Tracking implementiert
- IPerformanceMetricsService Interface und PerformanceMetrics Model erstellt
- 30 umfassende Unit-Tests für PerformanceMetricsService
- Thread-sichere Moving-Average-Berechnungen (30-Sample Window)
- 179+ Unit-Tests insgesamt für alle Services (AudioService: 20, EffectEngine: 28, HueService: 40+)
- CI/CD bereits konfiguriert mit Code Coverage via Codecov
- Vollständige Dokumentation in docs/tasks/Task9_Tests_Performance.md


---

Task 10 — CI/CD & MSI Packaging

**✅ TEILWEISE ABGESCHLOSSEN** - 2025-11-11

Beschreibung: GitHub Actions für Build, Test und MSI-Package.
PR-Bezeichnung: Task10_CICD_MSI
KI-Prompt:

CI/CD:
- ✅ GitHub Actions Workflow: Build, Test (Unit-Tests.yml, build_and_release.yml)
- ⬜ MSI Packaging mit WiX Toolset
- ⬜ Automatisches Versionierung
- ⬜ Release Artifacts Upload
- ⬜ Dokumentation: /docs/tasks/Task10_CICD_MSI.md

Ergebnis (bisher):
- GitHub Actions Workflows für Build und Unit-Tests implementiert
- .NET 9.0 Build-Pipeline auf Windows Runner
- Automatische Tests bei PR und Push auf main/develop

Noch zu erledigen:
- WiX Toolset Integration für MSI-Erstellung
- Automatische Versionsnummerierung
- Release-Pipeline mit Artifact-Upload
- Code-Signierung vorbereiten (für späteres Windows Store Deployment)


---

3. Erweiterte Tasks nach MVP (mit Prompts)

Task 11 — Zusätzliche Lichteffekte

PR-Bezeichnung: Task11_AdditionalEffects
Beschreibung: Neue Effekte (Rainbow, Strobe, Smooth Fade)
KI-Prompt:
- Effekt-Implementierung in EffectEngine
- UI Dropdown Auswahl
- Unit-Test: Effekt korrekt rendern
- Dokumentation Screenshots

Task 12 — Effektparameter feinjustierbar

PR-Bezeichnung: Task12_EffectParameterAdjustment
Beschreibung: Intensität, Geschwindigkeit, Farbvariationen einstellbar
KI-Prompt:
- Slider Controls in UI
- EffectEngine Bindings
- Persistenz über PresetService
- Unit-Test für Parameteränderung
- Dokumentation mit Beispiel Screenshots

Task 13 — Preset-Sharing / Cloud

PR-Bezeichnung: Task13_PresetCloudIntegration
Beschreibung: JSON Preset Export/Import online
KI-Prompt:
- Service zur Cloud Speicherung
- UI Import/Export Buttons
- Tests: Upload/Download korrekt
- Dokumentation Screenshots + API Beispiel

Task 14 — Security / Verschlüsselung

PR-Bezeichnung: Task14_SecureKeys
Beschreibung: DPAPI oder AES für AppKey und sensible Daten
KI-Prompt:
- ConfigurationService verschlüsseln
- Unit-Test: Keys korrekt entschlüsselt
- Dokumentation: Sicherheitskonzept

Task 15 — Advanced Logging & Metrics

PR-Bezeichnung: Task15_AdvancedLoggingMetrics
Beschreibung: Detailed Logs, Latency/FPS History, Export
KI-Prompt:
- Serilog konfigurieren
- MetricsService erstellen
- UI Anzeige der Metriken
- Unit-Test: Logs + Metriken korrekt
- Dokumentation Screenshots

Task 16 — Theme Enhancements

PR-Bezeichnung: Task16_ThemeEnhancements
Beschreibung: Benutzerdefinierte Farben, persistenter Theme Switch
KI-Prompt:
- ResourceDictionary dynamisch ändern
- Persistenz in ConfigService
- Unit-Test: Theme Wechsel korrekt
- Dokumentation Screenshots

Task 17 — Multi-Zone Support für Entertainment Areas

PR-Bezeichnung: Task17_MultiZoneSupport
Beschreibung: Unterstützung für mehrere Entertainment-Bereiche gleichzeitig
KI-Prompt:
- Mehrere Entertainment Areas gleichzeitig steuern
- UI für Auswahl und Verwaltung von Zonen
- Zonentrennung in EffectEngine
- Unabhängige Effekte pro Zone
- Unit-Test: Multi-Zone Koordination
- Dokumentation mit Screenshots

Task 18 — System Tray Integration

PR-Bezeichnung: Task18_SystemTrayIntegration
Beschreibung: Minimierung in System Tray, schneller Zugriff auf Funktionen
KI-Prompt:
- System Tray Icon mit Kontext-Menü
- Schnelle Effekt-Aktivierung aus Tray
- Minimierung in Tray statt Schließen (optional)
- Status-Anzeige im Tray Icon
- Unit-Test: Tray-Funktionen
- Dokumentation Screenshots

Task 19 — Keyboard Shortcuts

PR-Bezeichnung: Task19_KeyboardShortcuts
Beschreibung: Tastenkombinationen für häufige Aktionen
KI-Prompt:
- Globale Hotkeys für Start/Stop Effekte
- Preset-Wechsel per Tastatur
- Intensität/Helligkeit Anpassung per Shortcuts
- Konfigurierbare Tastenbelegung
- UI für Shortcut-Übersicht
- Unit-Test: Hotkey-Registrierung
- Dokumentation mit Tastenkombinationen-Liste

Task 20 — Auto-Update Mechanism

PR-Bezeichnung: Task20_AutoUpdateMechanism
Beschreibung: Automatische Update-Prüfung und Installation
KI-Prompt:
- Update-Prüfung bei App-Start (GitHub Releases API)
- Download und Installation von Updates
- UI Benachrichtigung über verfügbare Updates
- Versionsverlauf anzeigen
- Optional: Automatische Installation im Hintergrund
- Unit-Test: Update-Prüfung und Download
- Dokumentation Update-Prozess

Task 21 — Error Recovery & Reconnection

PR-Bezeichnung: Task21_ErrorRecovery
Beschreibung: Automatische Wiederverbindung und Fehlerbehandlung
KI-Prompt:
- Automatische Wiederverbindung bei Hue Bridge Verbindungsabbruch
- Audio-Device Wiederherstellung bei Ausfall
- Graceful Degradation bei Teilausfällen
- Retry-Logic mit Exponential Backoff
- UI-Feedback bei Verbindungsproblemen
- Unit-Test: Reconnection-Szenarien
- Dokumentation Error-Handling-Konzept

Task 22 — Settings Persistence & Configuration Export

PR-Bezeichnung: Task22_SettingsPersistence
Beschreibung: Persistenz aller App-Einstellungen und vollständiger Config-Export
KI-Prompt:
- Persistierung aller UI-Einstellungen (Theme, Fenstergröße, etc.)
- Vollständiger App-Konfigurations-Export (inkl. Bridge-Daten, Presets, etc.)
- Import von kompletten Konfigurationen
- Backup/Restore Funktionalität
- Verschlüsselte Speicherung sensibler Daten
- Unit-Test: Persistenz-Mechanismen
- Dokumentation Config-Format

Task 23 — Comprehensive QA & Documentation

PR-Bezeichnung: Task23_QA_Documentation
Beschreibung: Vollständige Testabdeckung + Dokumentation aktualisiert
KI-Prompt:
- Tests auswerten
- Performance und Coverage Reports
- User Documentation aktualisieren
- Screenshots und How-To Guides
- Installation und Setup-Anleitungen
- FAQ erstellen
- Troubleshooting Guide


---

Task 24 — Windows Store Deployment (MSI Signatur & Store Integration)

**WICHTIG**: Dieser Task kommt ganz zum Schluss und erfordert detaillierte Vorbereitung!

PR-Bezeichnung: Task24_WindowsStore_Deployment
Beschreibung: Vollständige Windows Store Integration mit Code-Signierung und Store-Deployment

### Voraussetzungen (vor Start des Tasks):

1. **Microsoft Partner Center Account**
   - Microsoft Partner Center Konto erstellen
   - App-Name reservieren im Microsoft Store
   - Publisher-Informationen verifizieren

2. **Code-Signing Zertifikat**
   - EV Code Signing Zertifikat erwerben (z.B. DigiCert, Sectigo)
   - Zertifikat für Windows-Anwendungen qualifiziert
   - Kosten: ca. 300-500€/Jahr
   - Alternative: Microsoft Store kann auch selbst signieren (empfohlen für Erstveröffentlichung)

3. **App-Assets vorbereiten**
   - App-Logo in verschiedenen Größen (44x44, 50x50, 150x150, 310x150, 310x310 PNG)
   - Screenshots für Store-Listing (mind. 1 Screenshot, empfohlen 4-5)
   - Hero Image für Store (optional, 1920x1080)
   - App-Beschreibung und Features-Liste
   - Datenschutzerklärung URL
   - Support-Kontakt und Website URL

4. **Microsoft Store Submission Requirements**
   - App muss alle Microsoft Store Policies erfüllen
   - Keine verbotenen Inhalte oder Funktionen
   - Datenschutzerklärung vorhanden
   - App-Altersbewertung festlegen (IARC Rating)

### Detaillierte Implementierungsschritte:

#### Phase 1: MSIX/APPX Package Erstellung

KI-Prompt:
```
Erstelle MSIX Package für LightJockey:
1. .csproj Projekt anpassen:
   - Package-Format auf MSIX umstellen
   - App-Manifest erstellen (Package.appxmanifest)
   - Icons und Assets einbinden
   - Capabilities definieren (Internet, Microphone)

2. Package.appxmanifest konfigurieren:
   - Publisher Identity setzen (CN=<Publisher Name>)
   - App-Namen und Beschreibung
   - Visual Assets referenzieren
   - Capabilities: internetClient, microphone, privateNetworkClientServer
   - Target Devices: Desktop, min Version Windows 10 1809

3. Build-Konfiguration:
   - Release Configuration für Store
   - Platform: x64 und ARM64 (optional)
   - Self-contained: false (Framework-dependent bevorzugt für Store)
   - Dokumentation: /docs/tasks/Task24_MSIX_Creation.md
```

#### Phase 2: Lokales Testen der MSIX

KI-Prompt:
```
Teste MSIX Package lokal:
1. Selbst-signiertes Testzertifikat erstellen:
   - New-SelfSignedCertificate in PowerShell
   - Zertifikat im Trusted Root Store installieren

2. MSIX mit Testzertifikat signieren:
   - SignTool.exe aus Windows SDK verwenden
   - Signier-Kommando: signtool sign /fd SHA256 /a /f <cert.pfx> /p <password> <package.msix>

3. Installation testen:
   - Add-AppxPackage in PowerShell
   - App starten und alle Funktionen testen
   - Deinstallation testen
   - Update-Szenario testen (neue Version installieren)

4. Dokumentation: /docs/tasks/Task24_MSIX_Testing.md
```

#### Phase 3: CI/CD Integration für MSIX Build

KI-Prompt:
```
Erweitere GitHub Actions für MSIX Build:
1. Workflow-Datei: .github/workflows/msix_build.yml
   - Trigger: Push auf Release-Tags (v*.*.*)
   - Windows-Runner verwenden
   - .NET 9 SDK Setup
   - NuGet Restore
   - MSBuild für MSIX Package
   - Artifact Upload für MSIX-Datei

2. Version automatisch aus Git Tag extrahieren:
   - Tag-Format: v1.0.0
   - Assembly Version und Package Version setzen
   - Build Number aus GitHub Run Number

3. Secrets konfigurieren:
   - SIGNING_CERTIFICATE (Base64-encoded .pfx)
   - SIGNING_PASSWORD
   - Optional: STORE_CREDENTIALS

4. MSIX signieren im Workflow:
   - Zertifikat aus Secret laden
   - SignTool im Workflow ausführen
   - Signierte MSIX als Artifact speichern

5. Dokumentation: /docs/tasks/Task24_CICD_MSIX.md
```

#### Phase 4: Microsoft Store Submission Vorbereitung

**WICHTIG**: Dieser Schritt erfordert manuellen Aufwand im Microsoft Partner Center!

Schritte:
1. **Partner Center einrichten**:
   - Anmelden bei https://partner.microsoft.com/dashboard
   - Neue App erstellen
   - App-Namen reservieren (z.B. "LightJockey")
   - App-Identität notieren (Package/Identity/Name und Publisher)

2. **Package.appxmanifest aktualisieren**:
   - Identity-Werte aus Partner Center übernehmen
   - Publisher-ID korrekt setzen
   - Package-Name muss exakt übereinstimmen

3. **Store-Listing ausfüllen**:
   - App-Beschreibung (Deutsch und Englisch)
   - Screenshots hochladen
   - App-Kategorie wählen (Utilities & tools)
   - Altersbewertung: PEGI 3 / ESRB E (Everyone)
   - Privacy Policy URL angeben

4. **App-Capabilities prüfen**:
   - Microphone-Zugriff begründen (Audio-Analyse)
   - Netzwerk-Zugriff begründen (Philips Hue Kommunikation)

5. **Preisgestaltung**:
   - Entscheiden: Gratis oder kostenpflichtig
   - Verfügbare Märkte auswählen

#### Phase 5: Erste Store Submission (Manuell)

**Achtung**: Erster Upload erfordert manuelle Submission!

Schritte:
1. **MSIX Package vorbereiten**:
   - Finale Version builden (z.B. v1.0.0)
   - Lokal testen (siehe Phase 2)
   - Optional: Externe Tester Alpha/Beta testen lassen

2. **Packages hochladen**:
   - Im Partner Center: "Packages" Section
   - MSIX-Datei hochladen
   - Store validiert Package automatisch
   - Fehler beheben falls Validierung fehlschlägt

3. **Zertifizierung einreichen**:
   - "Submit for certification" klicken
   - Microsoft prüft App (1-3 Werktage)
   - Possible Rejections:
     * Fehlende Privacy Policy
     * Nicht funktionierende Features
     * Crashes bei Startup
     * Fehlende Beschreibungen

4. **Nach Zertifizierung**:
   - App ist im Store verfügbar
   - Link zum Store-Listing notieren
   - README.md aktualisieren mit Store-Badge

#### Phase 6: Automatisierte Store Updates (Optional)

KI-Prompt:
```
Automatisiere Store Submission (optional, nach erster manueller Submission):

1. Microsoft Store Submission API einrichten:
   - Azure AD App Registration erstellen
   - API-Zugriff für Partner Center konfigurieren
   - Tenant ID, Client ID, Client Secret notieren

2. GitHub Actions erweitern:
   - Workflow für Store Submission
   - Trigger: Release veröffentlicht
   - Microsoft Store Submission API aufrufen
   - Package hochladen
   - Submission einreichen

3. Secrets hinzufügen:
   - AZURE_TENANT_ID
   - AZURE_CLIENT_ID
   - AZURE_CLIENT_SECRET
   - STORE_APP_ID

4. PowerShell Script für Submission:
   - StoreBroker PowerShell Module verwenden
   - Authentifizierung gegen Azure AD
   - Package-Upload
   - Submission erstellen und einreichen

5. Dokumentation: /docs/tasks/Task24_Automated_Submission.md
```

### Testing & QA Checklist

Vor Store Submission sicherstellen:

- [ ] App startet ohne Fehler auf frischer Windows-Installation
- [ ] Alle Features funktionieren (Audio, Hue, Effekte)
- [ ] Keine Crashes bei normaler Nutzung
- [ ] App schließt sauber (keine hängenden Prozesse)
- [ ] Deinstallation entfernt alle Dateien korrekt
- [ ] Update von alter Version funktioniert
- [ ] Alle Berechtigungen werden korrekt angefordert (Microphone, Network)
- [ ] Privacy Policy ist erreichbar
- [ ] Support-Kontakt ist gültig
- [ ] Screenshots sind aktuell und repräsentativ
- [ ] App-Beschreibung ist korrekt und vollständig

### Wichtige Links und Ressourcen

- **Microsoft Partner Center**: https://partner.microsoft.com/dashboard
- **Windows App Certification Kit**: https://docs.microsoft.com/windows/uwp/debug-test-perf/windows-app-certification-kit
- **MSIX Packaging Documentation**: https://docs.microsoft.com/windows/msix/
- **Store Policies**: https://docs.microsoft.com/windows/uwp/publish/store-policies
- **SignTool Documentation**: https://docs.microsoft.com/windows/win32/seccrypto/signtool
- **StoreBroker PowerShell**: https://github.com/microsoft/StoreBroker

### Geschätzte Kosten und Zeitaufwand

- **Microsoft Partner Center Account**: 19 USD einmalig (Einzelperson) oder 99 USD/Jahr (Unternehmen)
- **Code Signing Zertifikat**: 300-500€/Jahr (optional, Store kann auch signieren)
- **Zeitaufwand**:
  - MSIX Setup und lokales Testen: 8-16 Stunden
  - CI/CD Integration: 4-8 Stunden
  - Store-Listing Vorbereitung: 4-6 Stunden
  - Erste Submission und Zertifizierung: 1-3 Werktage (Microsoft Review)
  - Optional Automatisierung: 8-12 Stunden

### Hinweise

- **Store-Signierung empfohlen**: Für die erste Submission ist es einfacher, wenn Microsoft die App signiert. Eigenes Zertifikat ist nur nötig für Sideloading außerhalb des Stores.
- **Framework-Dependent vs Self-Contained**: Store bevorzugt Framework-dependent (kleinere Paketgröße, schnellere Updates)
- **Multiple Packages**: Für verschiedene Architekturen (x64, ARM64) separate Packages oder Bundle erstellen
- **Updates**: Nach initialer Submission können Updates über API oder Partner Center hochgeladen werden
- **Beta/Alpha**: Store unterstützt Flight-Gruppen für Beta-Tests vor Production-Release


---

4. Master Checkliste (Execution Checklist)

Task	PR-Bezeichnung	Umsetzung abgeschlossen	Doku-Link	QA-Check (manuell)

0	Task0_ProjectSetup	✅	[docs/tasks/Task0_ProjectSetup.md](docs/tasks/Task0_ProjectSetup.md)	✅
1	Task1_DI_Logging	✅	[docs/tasks/Task1_DI_Logging.md](docs/tasks/Task1_DI_Logging.md)	✅
2	Task2_AudioService	✅	[docs/tasks/Task2_AudioService.md](docs/tasks/Task2_AudioService.md)	✅
3	Task3_FFT_BeatDetector	✅	[docs/tasks/Task3_FFT_BeatDetector.md](docs/tasks/Task3_FFT_BeatDetector.md)	✅
4	Task4_HueService	✅	[docs/tasks/Task4_HueService.md](docs/tasks/Task4_HueService.md)	✅
5	Task5_EntertainmentV2	✅	[docs/tasks/Task5_EntertainmentV2.md](docs/tasks/Task5_EntertainmentV2.md)	✅
6	Task6_EffectEngine	✅	[docs/tasks/Task6_EffectEngine.md](docs/tasks/Task6_EffectEngine.md)	✅
7	Task7_UI_Visualizer	✅	[docs/tasks/Task7_UI_Visualizer.md](docs/tasks/Task7_UI_Visualizer.md)	✅
8	Task8_PresetService	✅	[docs/tasks/Task8_PresetService.md](docs/tasks/Task8_PresetService.md)	✅
9	Task9_Tests_Performance	✅	[docs/tasks/Task9_Tests_Performance.md](docs/tasks/Task9_Tests_Performance.md)	✅
10	Task10_CICD_MSI	⬜	[docs/tasks/Task10_CICD_MSI.md](docs/tasks/Task10_CICD_MSI.md)	⬜
11	Task11_AdditionalEffects	⬜	[link]	⬜
12	Task12_EffectParameterAdjustment	⬜	[link]	⬜
13	Task13_PresetCloudIntegration	⬜	[link]	⬜
14	Task14_SecureKeys	⬜	[link]	⬜
15	Task15_AdvancedLoggingMetrics	⬜	[link]	⬜
16	Task16_ThemeEnhancements	⬜	[link]	⬜
17	Task17_MultiZoneSupport	⬜	[link]	⬜
18	Task18_SystemTrayIntegration	⬜	[link]	⬜
19	Task19_KeyboardShortcuts	⬜	[link]	⬜
20	Task20_AutoUpdateMechanism	⬜	[link]	⬜
21	Task21_ErrorRecovery	⬜	[link]	⬜
22	Task22_SettingsPersistence	⬜	[link]	⬜
23	Task23_QA_Documentation	⬜	[link]	⬜
24	Task24_WindowsStore_Deployment	⬜	[link]	⬜
