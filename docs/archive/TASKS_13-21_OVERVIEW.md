# Tasks 13-21 Overview

This document provides a quick reference for Tasks 13-21 from the LightJockey development plan.

## Task 13 — Preset-Sharing / Cloud

**PR-Bezeichnung:** Task13_PresetImportExport

**Beschreibung:** JSON Preset Export/Import
- Automatischer Backup Service zur Speicherung von kompletter Konfiguration inklusive Definition wie viele Sicherungen vorgehalten werden und wann gelöscht werden
- Presets Export Import für Farbgruppen, UI Anpassung per Drag and Drop, Lichteffekt Konfiguration
- UI Import/Export Buttons
- Tests: Upload/Download korrekt
- Dokumentation Screenshots + API Beispiel

**Status:** ⬜ Not Started

---

## Task 14 — Security / Verschlüsselung

**PR-Bezeichnung:** Task14_SecureKeys

**Beschreibung:** DPAPI oder AES für AppKey und sensible Daten

**KI-Prompt:**
- ConfigurationService verschlüsseln
- Unit-Test: Keys korrekt entschlüsselt
- Dokumentation: Sicherheitskonzept

**Status:** ⬜ Not Started

---

## Task 15 — Advanced Logging & Metrics

**PR-Bezeichnung:** Task15_AdvancedLoggingMetrics

**Beschreibung:** Detailed Logs, Latency/FPS History, Export

**KI-Prompt:**
- Serilog konfigurieren
- MetricsService erstellen
- UI Anzeige der Metriken
- Unit-Test: Logs + Metriken korrekt
- Dokumentation Screenshots

**Status:** ⬜ Not Started

---

## Task 16 — Theme Enhancements

**PR-Bezeichnung:** Task16_ThemeEnhancements

**Beschreibung:** Benutzerdefinierte Farben, persistenter Theme Switch

**KI-Prompt:**
- ResourceDictionary dynamisch ändern
- Persistenz in ConfigService
- Unit-Test: Theme Wechsel korrekt
- Dokumentation Screenshots

**Status:** ⬜ Not Started

---

## Task 17 — Hue Bridge Multisupport

**PR-Bezeichnung:** Task17_Hue-Bridge-MultiSupport

**Beschreibung:** Option eine weitere Bridge zu verwenden um so weitere 10 Lampen für die Entertainment Lichteffekte zu verwenden

**Details:**
- Die 2. Bridge inklusive der 2. Entertainment Gruppe sollte man in verschiedenen Modi nutzen können:
  - A) Völlig unabhängig voneinander
  - B) Beide Bridges vereint
  - C) Matrix Mode (man kann die jeweils 10 Lampen der beiden Entertainment Gruppen mit einer App internen Gruppierung vollständig individuell anpassen)

**Status:** ⬜ Not Started

---

## Task 18 — WebSocketAPI-Schnittstelle

**PR-Bezeichnung:** Task18_WebSocketAPI

**Beschreibung:** Implementierung einer WebSocket-API zur Fernsteuerung von LightJockey durch externe Tools wie Streamer.Bot

**KI-Prompt:**
- WebSocket-Server in LightJockey integrieren
- JSON-basiertes Protokoll für Befehle definieren (z.B. Effekte starten/stoppen, Presets wechseln, Farbänderungen)
- Sichere Verbindungsbehandlung gewährleisten
- API-Dokumentation für die Anbindung an Streamer.Bot erstellen
- Unit-Tests für WebSocket-Befehle implementieren

**Status:** ⬜ Not Started

---

## Task 19 — 2D-LightMappingManager

**PR-Bezeichnung:** Task19_2D-LightMappingManager

**Beschreibung:** Ermöglicht das Platzieren von Lampen auf einer 2D-Karte (X/Y-Achse) in einem virtuellen Raum. Inklusive Live-Vorschau von Lichteffekten, Auswahl von Farbgruppen, Bearbeitung von Lampengruppen und Erstellung individueller oder manueller Effektsteuerungen.

**KI-Prompt:**
- LightMappingService, zugehörige ViewModels und Views erstellen
- Canvas-basierte UI für Drag-and-Drop von Lampen entwickeln
- Lampenpositionen (X/Y-Koordinaten) in der Konfiguration/den Presets speichern
- Vorschaumodus zur Visualisierung von Effekten auf der 2D-Karte implementieren
- Erstellung und Bearbeitung von Lampengruppen direkt auf der Karte ermöglichen
- EffectEngine erweitern, um das 2D-Mapping für räumliche Effekte zu nutzen
- Unit-Tests für die Mapping-Logik erstellen

**Status:** ⬜ Not Started

---

## Task 20 — CI/CD + Windows Store Deployment

**PR-Bezeichnung:** Task20_CICD_StoreDeployment

**Beschreibung:** Signierte MSI + Windows Store Deployment

**KI-Prompt:**
- GitHub Actions anpassen: SignTool + Store-ready
- Test: Signierte MSI korrekt
- Dokumentation CI/CD Workflow

**Status:** ⬜ Not Started

---

## Task 21 — Comprehensive QA & Documentation

**PR-Bezeichnung:** Task21_QA_Documentation

**Beschreibung:** Vollständige Testabdeckung + Dokumentation aktualisiert

**KI-Prompt:**
- Tests auswerten
- Performance und Coverage Reports
- User Documentation aktualisieren
- Screenshots und How-To Guides

**Status:** ⬜ Not Started

---

## Next Steps

To create issues/PRs for these tasks:

1. **Automatic (Recommended):** Enable the automation workflow by setting the `AUTOMATION_ENABLED` repository variable to `true`. The workflow will automatically create issues and PRs for the next uncompleted task.

2. **Manual:** Create issues and PRs manually using the information above.

3. **Workflow Dispatch:** Manually trigger the `.github/workflows/TASK-B.01_Start-Task_on-Manual_MAN.yml` workflow to create the next task.

⚠️ **Important**: After the automation creates an issue/PR, you must manually activate GitHub Copilot Workspace to implement the task. See [AUTO_TASK_TROUBLESHOOTING.md](../docs/AUTO_TASK_TROUBLESHOOTING.md) for details.

## Task Priority

According to the user, **Task 13** is the current priority and should be worked on next.
