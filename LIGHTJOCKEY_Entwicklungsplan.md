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

Beschreibung: Audioanalyse: FFT, Spectral Analysis, Beat Detection.
PR-Bezeichnung: Task3_FFT_BeatDetector
KI-Prompt:

Implementiere:
- FFTProcessor für AudioStream
- BeatDetector (BPM-Erkennung, Peak Detection)
- SpectralAnalyzer (Low/Mid/High Frequenzbänder)
- Events für EffectEngine
- Dokumentation inkl. Diagramme
- Unit-Tests: Signalanalysen korrekt


---

Task 4 — HueService (HTTPS)

Beschreibung: HTTP-basierter Philips Hue Client + Bridge Setup UI.
PR-Bezeichnung: Task4_HueService
KI-Prompt:

Implementiere:
- HueService HTTPS Client
- Bridge Discovery + UI Setup
- Authentifizierung
- Test-LED Steuerung
- Dokumentation / Screenshots
- Unit-Test: Verbindung + LED Update


---

Task 5 — Entertainment V2 (DTLS/UDP)

Beschreibung: UDP/DTLS Client für Entertainment V2 Lichteffekte.
PR-Bezeichnung: Task5_EntertainmentV2
KI-Prompt:

Erstelle EntertainmentV2 Service:
- DTLS/UDP Verbindung aufbauen
- Paketformat wie Philips Hue Entertainment V2
- Ereignisse aus AudioService zur Lichteffektsteuerung
- Dokumentation + Tests für Paket-Transportsicherheit
- CI/CD Build Check


---

Task 6 — EffectEngine (MVP modes)

Beschreibung: Implementierung der langsamen und schnellen Lichteffekte.
PR-Bezeichnung: Task6_EffectEngine
KI-Prompt:

EffectEngine:
- MVP Modi: langsamer HTTPS Effekt, schneller EntertainmentV2 Effekt
- Event-Handler für AudioService
- Config Bindings für Effektparameter
- Dokumentation mit Flowchart
- Unit-Test: Effekt-Ausgabe korrekt


---

Task 7 — UI (MainWindow) + Visualizer + Controls

Beschreibung: Visualisierung der Effekte, Steuerung von Parametern, Device Auswahl.
PR-Bezeichnung: Task7_UI_Visualizer
KI-Prompt:

MainWindow UI:
- Device Selection, Effect Selection, Parameter Sliders
- Visualizer für AudioInput
- Dark/Light Mode implementiert
- MVVM Bindings korrekt
- Dokumentation Screenshots
- UI Unit-Tests: Bindings, Commands


---

Task 8 — Preset/Configuration Service

Beschreibung: Automatisches Speichern, manuelles Import/Export.
PR-Bezeichnung: Task8_PresetService
KI-Prompt:

PresetService:
- Persistiert Konfiguration automatisch
- Import/Export JSON
- EffectEngine + UI Bindings
- Dokumentation + Beispiel Presets
- Unit-Test: Speichern/Laden korrekt


---

Task 9 — Tests, Performance, Metrics

Beschreibung: Unit/Integration Tests, Performance Checks.
PR-Bezeichnung: Task9_Tests_Performance
KI-Prompt:

Implementiere:
- Unit Tests für AudioService, EffectEngine, HueService
- Performance Metrics (FPS, Latency)
- CI/CD Build Test mit Coverage Report
- Dokumentation: Test Report


---

Task 10 — CI/CD & MSI Packaging

Beschreibung: GitHub Actions für Build, Test und MSI-Package.
PR-Bezeichnung: Task10_CICD_MSI
KI-Prompt:

CI/CD:
- GitHub Actions Workflow: Build, Test, MSI Packaging
- Dokumentation: /docs/tasks/Task10_CICD_MSI.md
- Unit-Test Build success
- Bereit für spätere Store Deployment Integration


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

Task 17 — CI/CD + Windows Store Deployment

PR-Bezeichnung: Task17_CICD_StoreDeployment
Beschreibung: Signierte MSI + Windows Store Deployment
KI-Prompt:
- GitHub Actions anpassen: SignTool + Store-ready
- Test: Signierte MSI korrekt
- Dokumentation CI/CD Workflow

Task 18 — Comprehensive QA & Documentation

PR-Bezeichnung: Task18_QA_Documentation
Beschreibung: Vollständige Testabdeckung + Dokumentation aktualisiert
KI-Prompt:
- Tests auswerten
- Performance und Coverage Reports
- User Documentation aktualisieren
- Screenshots und How-To Guides


---

4. Master Checkliste (Execution Checklist)

Task	PR-Bezeichnung	Umsetzung abgeschlossen	Doku-Link	QA-Check (manuell)

0	Task0_ProjectSetup	✅	[docs/tasks/Task0_ProjectSetup.md](docs/tasks/Task0_ProjectSetup.md)	✅
1	Task1_DI_Logging	✅	[docs/tasks/Task1_DI_Logging.md](docs/tasks/Task1_DI_Logging.md)	✅
2	Task2_AudioService	✅	[docs/tasks/Task2_AudioService.md](docs/tasks/Task2_AudioService.md)	✅
3	Task3_FFT_BeatDetector	⬜	[link]	⬜
4	Task4_HueService	⬜	[link]	⬜
5	Task5_EntertainmentV2	⬜	[link]	⬜
6	Task6_EffectEngine	⬜	[link]	⬜
7	Task7_UI_Visualizer	⬜	[link]	⬜
8	Task8_PresetService	⬜	[link]	⬜
9	Task9_Tests_Performance	⬜	[link]	⬜
10	Task10_CICD_MSI	⬜	[link]	⬜
11	Task11_AdditionalEffects	⬜	[link]	⬜
12	Task12_EffectParameterAdjustment	⬜	[link]	⬜
13	Task13_PresetCloudIntegration	⬜	[link]	⬜
14	Task14_SecureKeys	⬜	[link]	⬜
15	Task15_AdvancedLoggingMetrics	⬜	[link]	⬜
16	Task16_ThemeEnhancements	⬜	[link]	⬜
17	Task17_CICD_StoreDeployment	⬜	[link]	⬜
18	Task18_QA_Documentation	⬜	[link]	⬜
