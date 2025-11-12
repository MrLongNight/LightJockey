#!/usr/bin/env python3
"""
Script to create GitHub Issues for Tasks 13-21
Requires: pip install PyGithub
Usage: GITHUB_TOKEN=your_token python3 create_task_issues.py
"""

import os
import sys

try:
    from github import Github
except ImportError:
    print("Error: PyGithub not installed. Install with: pip install PyGithub")
    sys.exit(1)

# Task definitions from LIGHTJOCKEY_Entwicklungsplan.md
TASKS = [
    {
        "number": 13,
        "title": "Preset-Sharing / Cloud",
        "pr_name": "Task13_PresetImportExport",
        "description": """JSON Preset Export/Import:
- Automatischer Backup Service zur Speicherung von kompletter Konfiguration inklusive Definition wie viele Sicherungen vorgehalten werden und wann gelöscht werden
- Presets Export Import für Farbgruppen, UI Anpassung per Drag and Drop, Lichteffekt Konfiguration
- UI Import/Export Buttons
- Tests: Upload/Download korrekt
- Dokumentation Screenshots + API Beispiel"""
    },
    {
        "number": 14,
        "title": "Security / Verschlüsselung",
        "pr_name": "Task14_SecureKeys",
        "description": """DPAPI oder AES für AppKey und sensible Daten

KI-Prompt:
- ConfigurationService verschlüsseln
- Unit-Test: Keys korrekt entschlüsselt
- Dokumentation: Sicherheitskonzept"""
    },
    {
        "number": 15,
        "title": "Advanced Logging & Metrics",
        "pr_name": "Task15_AdvancedLoggingMetrics",
        "description": """Detailed Logs, Latency/FPS History, Export

KI-Prompt:
- Serilog konfigurieren
- MetricsService erstellen
- UI Anzeige der Metriken
- Unit-Test: Logs + Metriken korrekt
- Dokumentation Screenshots"""
    },
    {
        "number": 16,
        "title": "Theme Enhancements",
        "pr_name": "Task16_ThemeEnhancements",
        "description": """Benutzerdefinierte Farben, persistenter Theme Switch

KI-Prompt:
- ResourceDictionary dynamisch ändern
- Persistenz in ConfigService
- Unit-Test: Theme Wechsel korrekt
- Dokumentation Screenshots"""
    },
    {
        "number": 17,
        "title": "Hue Bridge Multisupport",
        "pr_name": "Task17_Hue-Bridge-MultiSupport",
        "description": """Option eine weitere Bridge zu verwenden um so weitere 10 Lampen für die Entertainment Lichteffekte zu verwenden

Die 2. Bridge inklusive der 2. Entertainment Gruppe sollte man in verschiedenen Modi nutzen können:
- A) Völlig unabhängig voneinander
- B) Beide Bridges vereint
- C) Matrix Mode (man kann die jeweils 10 Lampen der beiden Entertainment Gruppen mit einer App internen Gruppierung vollständig individuell anpassen)"""
    },
    {
        "number": 18,
        "title": "WebSocketAPI-Schnittstelle",
        "pr_name": "Task18_WebSocketAPI",
        "description": """Implementierung einer WebSocket-API zur Fernsteuerung von LightJockey durch externe Tools wie Streamer.Bot

KI-Prompt:
- WebSocket-Server in LightJockey integrieren
- JSON-basiertes Protokoll für Befehle definieren (z.B. Effekte starten/stoppen, Presets wechseln, Farbänderungen)
- Sichere Verbindungsbehandlung gewährleisten
- API-Dokumentation für die Anbindung an Streamer.Bot erstellen
- Unit-Tests für WebSocket-Befehle implementieren"""
    },
    {
        "number": 19,
        "title": "2D-LightMappingManager",
        "pr_name": "Task19_2D-LightMappingManager",
        "description": """Ermöglicht das Platzieren von Lampen auf einer 2D-Karte (X/Y-Achse) in einem virtuellen Raum

Inklusive Live-Vorschau von Lichteffekten, Auswahl von Farbgruppen, Bearbeitung von Lampengruppen und Erstellung individueller oder manueller Effektsteuerungen.

KI-Prompt:
- LightMappingService, zugehörige ViewModels und Views erstellen
- Canvas-basierte UI für Drag-and-Drop von Lampen entwickeln
- Lampenpositionen (X/Y-Koordinaten) in der Konfiguration/den Presets speichern
- Vorschaumodus zur Visualisierung von Effekten auf der 2D-Karte implementieren
- Erstellung und Bearbeitung von Lampengruppen direkt auf der Karte ermöglichen
- EffectEngine erweitern, um das 2D-Mapping für räumliche Effekte zu nutzen
- Unit-Tests für die Mapping-Logik erstellen"""
    },
    {
        "number": 20,
        "title": "CI/CD + Windows Store Deployment",
        "pr_name": "Task20_CICD_StoreDeployment",
        "description": """Signierte MSI + Windows Store Deployment

KI-Prompt:
- GitHub Actions anpassen: SignTool + Store-ready
- Test: Signierte MSI korrekt
- Dokumentation CI/CD Workflow"""
    },
    {
        "number": 21,
        "title": "Comprehensive QA & Documentation",
        "pr_name": "Task21_QA_Documentation",
        "description": """Vollständige Testabdeckung + Dokumentation aktualisiert

KI-Prompt:
- Tests auswerten
- Performance und Coverage Reports
- User Documentation aktualisieren
- Screenshots und How-To Guides"""
    }
]

def main():
    # Get GitHub token from environment
    token = os.environ.get('GITHUB_TOKEN')
    if not token:
        print("Error: GITHUB_TOKEN environment variable not set")
        print("Usage: GITHUB_TOKEN=your_token python3 create_task_issues.py")
        sys.exit(1)
    
    # Initialize GitHub client
    g = Github(token)
    
    # Get repository
    try:
        repo = g.get_repo("MrLongNight/LightJockey")
        print(f"Connected to repository: {repo.full_name}")
    except Exception as e:
        print(f"Error accessing repository: {e}")
        sys.exit(1)
    
    # Create issues for each task
    created_count = 0
    skipped_count = 0
    
    for task in TASKS:
        task_num = task["number"]
        title = f"Task {task_num}: {task['title']}"
        branch_name = f"feature/task-{task_num}-autogenerated"
        
        # Check if issue already exists
        existing_issues = repo.get_issues(state='all', labels=['autogenerated'])
        issue_exists = any(title in issue.title for issue in existing_issues)
        
        if issue_exists:
            print(f"⊘ Task {task_num}: Issue already exists, skipping...")
            skipped_count += 1
            continue
        
        # Create issue body
        body = f"""## Auto-generated Task

{task['description']}

---

**Source:** LIGHTJOCKEY_Entwicklungsplan.md
**Task Number:** {task_num}
**PR-Bezeichnung:** {task['pr_name']}
**Branch:** `{branch_name}`

This issue was created by the task creation script.
"""
        
        try:
            # Create the issue
            issue = repo.create_issue(
                title=title,
                body=body,
                labels=['autogenerated']
            )
            print(f"✓ Task {task_num}: Created issue #{issue.number} - {task['title']}")
            created_count += 1
        except Exception as e:
            print(f"✗ Task {task_num}: Failed to create issue - {e}")
    
    print(f"\n{'='*60}")
    print(f"Summary:")
    print(f"  Created: {created_count} issues")
    print(f"  Skipped: {skipped_count} issues (already exist)")
    print(f"  Total:   {len(TASKS)} tasks")
    print(f"{'='*60}")

if __name__ == "__main__":
    main()
