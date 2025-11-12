# Jules API Integration für LightJockey

## Überblick

Dieses Dokument beschreibt die Integration der Google Jules API in den LightJockey-Automatisierungsprozess. Die Jules API ermöglicht es, Entwicklungsaufgaben programmatisch an Jules zu übergeben, der sie dann autonom bearbeitet und Pull Requests erstellt.

## Workflow-Architektur

Der Jules API Workflow besteht aus drei Hauptkomponenten:

```
┌─────────────────────────────────────────────────────────────┐
│  1. Flow Jules 01: Submit Task to Jules API                │
│     - Findet nächste unerledigte Aufgabe                    │
│     - Erstellt Jules Session via API                        │
│     - Erstellt Tracking-Issue auf GitHub                    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  2. Flow Jules 02: Monitor Session and Review PR           │
│     - Überwacht Jules Session alle 15 Minuten              │
│     - Erkennt wenn Jules einen PR erstellt hat             │
│     - Triggert Copilot Agent Review                        │
│     - Aktualisiert Tracking-Issue                          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Flow Jules 03: Auto-Merge After Review                 │
│     - Prüft CI-Test-Status                                 │
│     - Merged PR wenn alle Tests erfolgreich                │
│     - Schließt Tracking-Issue                              │
│     - Zyklus beginnt von vorne                             │
└─────────────────────────────────────────────────────────────┘
```

## Voraussetzungen

### 1. Jules GitHub App Installation

Bevor Sie die Jules API verwenden können, müssen Sie die Jules GitHub App für Ihr Repository installieren:

1. Gehen Sie zu [jules.google.com](https://jules.google.com)
2. Melden Sie sich mit Ihrem Google-Konto an
3. Installieren Sie die Jules GitHub App für Ihr Repository
4. Autorisieren Sie den Zugriff auf das gewünschte Repository

### 2. Jules API Key

Generieren Sie einen API-Schlüssel in den Jules-Einstellungen:

1. Gehen Sie zu [jules.google.com/settings#api](https://jules.google.com/settings#api)
2. Klicken Sie auf "Create API Key"
3. Kopieren Sie den generierten Schlüssel (Sie können maximal 3 Keys haben)

⚠️ **Wichtig**: Behandeln Sie Ihren API-Schlüssel vertraulich. Teilen Sie ihn nicht und betten Sie ihn nicht in öffentlichen Code ein.

### 3. GitHub Repository Secrets

Fügen Sie den Jules API Key als Secret zu Ihrem GitHub Repository hinzu:

1. Gehen Sie zu Ihren Repository-Einstellungen
2. Navigieren Sie zu `Settings` → `Secrets and variables` → `Actions`
3. Klicken Sie auf "New repository secret"
4. Name: `JulesAPIKey`
5. Value: Ihr Jules API-Schlüssel
6. Klicken Sie auf "Add secret"

### 4. Repository Variables

Aktivieren Sie die Jules-Automatisierung:

1. Gehen Sie zu `Settings` → `Secrets and variables` → `Actions` → `Variables`
2. Klicken Sie auf "New repository variable"
3. Name: `JULES_AUTOMATION_ENABLED`
4. Value: `true`
5. Klicken Sie auf "Add variable"

## Verwendung

### Automatischer Modus

Der Jules API Workflow startet automatisch nach jedem Push auf den `main` Branch:

1. **Automatischer Start**: Nach einem Merge wird automatisch die nächste Aufgabe gefunden
2. **Jules Session**: Eine neue Session wird über die API erstellt
3. **Tracking**: Ein GitHub Issue wird zur Verfolgung erstellt
4. **Monitoring**: Der Status wird alle 15 Minuten überprüft
5. **PR Review**: Copilot Agent überprüft den von Jules erstellten PR
6. **Auto-Merge**: Nach erfolgreichen Tests wird der PR gemerged

### Manueller Start

Sie können den Workflow auch manuell starten:

```bash
# Via GitHub CLI
gh workflow run flow-jules_01-submit-task.yml

# Oder über die GitHub Web UI
# Actions → Flow Jules 01: Submit Task to Jules API → Run workflow
```

### Session Monitoring

Sie können Jules Sessions auch manuell überwachen:

```bash
# Mit dem Helper-Script
export JULES_API_KEY="your-api-key"
./scripts/jules_api_helper.py list-sessions

# Session Details abrufen
./scripts/jules_api_helper.py get-session <session-id>

# Session kontinuierlich überwachen
./scripts/jules_api_helper.py monitor <session-id>
```

## Jules API Helper Script

Das `scripts/jules_api_helper.py` Script bietet mehrere Befehle:

### Verfügbare Sources auflisten

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY list-sources
```

### Source für Repository finden

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY get-source \
  --owner MrLongNight \
  --repo LightJockey
```

### Neue Session erstellen

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY create-session \
  --source "sources/github/owner/repo" \
  --title "Task 13: Preset-Sharing" \
  --prompt "Implement cloud backup functionality for presets"
```

### Session Details abrufen

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY get-session SESSION_ID
```

### Alle Sessions auflisten

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY list-sessions
```

### Session Activities auflisten

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY list-activities SESSION_ID
```

### Session überwachen

```bash
./scripts/jules_api_helper.py --api-key YOUR_KEY monitor SESSION_ID \
  --interval 30 \
  --timeout 3600
```

## Workflow-Details

### Flow Jules 01: Submit Task to Jules API

**Trigger:**
- Push auf `main` Branch
- Manueller Workflow-Dispatch

**Schritte:**
1. Prüft ob Jules-Automatisierung aktiviert ist
2. Validiert Jules API Key
3. Findet nächste unerledigte Aufgabe aus `LIGHTJOCKEY_Entwicklungsplan.md`
4. Ruft Jules Source für das Repository ab
5. Erstellt Jules Session mit detailliertem Prompt
6. Erstellt Tracking-Issue auf GitHub

**Outputs:**
- Session ID
- Tracking Issue Number
- Task Information

### Flow Jules 02: Monitor Session and Review PR

**Trigger:**
- Zeitplan: Alle 15 Minuten (Cron)
- Manueller Workflow-Dispatch

**Schritte:**
1. Findet aktive Jules Sessions aus Tracking-Issues
2. Prüft Session-Status via Jules API
3. Erkennt neu erstellte PRs
4. Aktualisiert Tracking-Issue mit PR-Info
5. Fügt Labels für Copilot-Review hinzu
6. Kommentiert auf PR mit Review-Anfrage

**Outputs:**
- PR Number
- PR URL
- Review Status

### Flow Jules 03: Auto-Merge After Review

**Trigger:**
- PR Review submitted
- Check Suite completed
- Manueller Workflow-Dispatch

**Schritte:**
1. Findet PRs mit `jules-pr` und `automerge` Labels
2. Verifiziert dass alle CI-Checks erfolgreich waren
3. Konvertiert Draft-PR zu "Ready for Review" falls nötig
4. Merged den PR mit Squash-Merge
5. Schließt das Tracking-Issue
6. Kommentiert auf Issue und PR

## Copilot Agent Integration

Der Copilot Agent wird automatisch ausgelöst, wenn Jules einen PR erstellt:

1. **Automatische Review-Anfrage**: Der Workflow kommentiert auf dem PR mit `@copilot`
2. **Review-Checkliste**: Eine detaillierte Checkliste wird bereitgestellt
3. **Automatische Korrekturen**: Copilot kann Fehler direkt im PR korrigieren
4. **Label-basiert**: PRs mit `copilot-review` Label werden priorisiert

### Review-Kriterien

Der Copilot Agent überprüft:
- ✅ Code folgt MVVM-Pattern
- ✅ Unit-Tests sind umfassend
- ✅ Code-Coverage >80%
- ✅ XML-Dokumentation vorhanden
- ✅ Keine offensichtlichen Bugs
- ✅ C#-Best-Practices befolgt
- ✅ Saubere Integration mit bestehendem Code

## Konfiguration

### Workflow-Optionen

Sie können das Verhalten der Workflows anpassen:

```yaml
# In .github/workflows/jules-api/flow-jules_01-submit-task.yml
# Automation Mode ändern (AUTO_CREATE_PR, MANUAL, oder NONE)
"automationMode": "AUTO_CREATE_PR"

# Plan-Genehmigung erforderlich machen
"requirePlanApproval": true
```

### Monitoring-Intervall

```yaml
# In .github/workflows/jules-api/flow-jules_02-monitor-and-review.yml
schedule:
  # Alle 15 Minuten (anpassbar)
  - cron: '*/15 * * * *'
```

### Merge-Strategie

```yaml
# In .github/workflows/jules-api/flow-jules_03-auto-merge.yml
merge_method: 'squash'  # oder 'merge', 'rebase'
```

## Sicherheit

### API-Schlüssel-Sicherheit

- ❌ **Niemals** API-Schlüssel in Code committen
- ✅ **Immer** GitHub Secrets verwenden
- ✅ **Regelmäßig** API-Schlüssel rotieren
- ✅ **Minimal** Permissions verwenden

### Workflow-Permissions

Die Workflows verwenden minimal notwendige Permissions:

```yaml
permissions:
  contents: write      # Für Merge-Operationen
  issues: write        # Für Issue-Erstellung und -Updates
  pull-requests: write # Für PR-Operationen
```

### Branch Protection

Empfohlene Branch-Protection-Regeln für `main`:

- ✅ Require pull request reviews
- ✅ Require status checks to pass
- ✅ Require branches to be up to date
- ✅ Include administrators (optional)

## Fehlerbehandlung

### Jules API Fehler

Wenn die Jules API einen Fehler zurückgibt:

1. **Prüfen Sie den API-Schlüssel**: Ist er gültig und nicht abgelaufen?
2. **Prüfen Sie die Source**: Ist das Repository in Jules verbunden?
3. **Prüfen Sie die Quota**: Haben Sie API-Limits erreicht?

### Session-Timeouts

Falls eine Session nicht fortschreitet:

1. Öffnen Sie die Jules Web UI: `https://jules.google.com/sessions/SESSION_ID`
2. Prüfen Sie Session-Status und Activities
3. Senden Sie bei Bedarf eine Nachricht an Jules
4. Manuell weiterarbeiten falls Jules blockiert ist

### PR-Konflikte

Bei Merge-Konflikten:

1. Der Auto-Merge schlägt fehl
2. Ein Kommentar wird auf dem PR erstellt
3. Lösen Sie Konflikte manuell
4. Triggern Sie Auto-Merge erneut

## Monitoring und Logging

### GitHub Actions Logs

Alle Workflow-Runs werden in den GitHub Actions Logs aufgezeichnet:

1. Gehen Sie zu `Actions` Tab
2. Wählen Sie den Workflow
3. Klicken Sie auf einen Run für Details

### Jules Session Logs

Jules Session Activities können abgerufen werden:

```bash
./scripts/jules_api_helper.py list-activities SESSION_ID
```

### Issue Tracking

Jede Task hat ein Tracking-Issue mit:
- Session ID
- Status-Updates
- PR-Links
- Fehler-Informationen

## Troubleshooting

### "Repository not found in Jules sources"

**Problem**: Das Repository ist nicht in Jules verbunden.

**Lösung**:
1. Gehen Sie zu [jules.google.com](https://jules.google.com)
2. Installieren Sie die Jules GitHub App
3. Autorisieren Sie den Zugriff auf Ihr Repository

### "JulesAPIKey secret is not set"

**Problem**: API-Schlüssel nicht konfiguriert.

**Lösung**:
1. Generieren Sie einen API-Schlüssel in Jules Settings
2. Fügen Sie ihn als Repository Secret hinzu

### "Jules automation is disabled"

**Problem**: `JULES_AUTOMATION_ENABLED` Variable ist nicht gesetzt.

**Lösung**:
1. Gehen Sie zu Repository Variables
2. Erstellen Sie `JULES_AUTOMATION_ENABLED` mit Wert `true`

### "No uncompleted tasks found"

**Problem**: Alle Tasks sind erledigt oder Entwicklungsplan nicht gefunden.

**Lösung**:
1. Prüfen Sie `LIGHTJOCKEY_Entwicklungsplan.md`
2. Stellen Sie sicher, dass unerledigte Tasks existieren
3. Prüfen Sie das Checklist-Format

## Vergleich: Copilot vs Jules API

| Aspekt | GitHub Copilot Agent | Jules API |
|--------|---------------------|-----------|
| **Aktivierung** | Manueller Klick erforderlich | Vollautomatisch via API |
| **Monitoring** | GitHub PR Interface | API + Custom Workflows |
| **Kontrolle** | GitHub Native | Programmatisch |
| **Kosten** | Copilot-Lizenz | Jules API Credits |
| **PR-Erstellung** | Copilot erstellt PR | Jules erstellt PR |
| **Review** | Integriert | Separate Copilot-Review |
| **Automatisierung** | ~95% | ~98% |

## Best Practices

### 1. Klare Task-Beschreibungen

Schreiben Sie detaillierte Task-Beschreibungen in `LIGHTJOCKEY_Entwicklungsplan.md`:

```markdown
Task 13 — Preset-Sharing / Cloud

Beschreibung: Implementiere Cloud-Backup-Funktionalität für Presets.

Anforderungen:
- Automatische Backups zu konfigurierbarem Intervall
- Import/Export von Presets
- Retention-Richtlinien
- Umfassende Unit-Tests

Siehe docs/tasks/Task13_Details.md für vollständige Spezifikation.
```

### 2. Monitoring einrichten

Überwachen Sie Jules Sessions regelmäßig:

```bash
# Cron-Job für Monitoring (optional)
*/30 * * * * /path/to/jules_api_helper.py monitor SESSION_ID --timeout 60
```

### 3. Labels verwenden

Nutzen Sie GitHub Labels zur Organisation:

- `jules-task`: Von Jules bearbeitete Aufgaben
- `jules-pr`: Von Jules erstellte PRs
- `copilot-review`: Benötigt Copilot-Review
- `automerge`: Bereit für Auto-Merge

### 4. Branch Protection

Aktivieren Sie Branch Protection für `main`:

- Verhindert direkte Commits
- Erzwingt PR-Reviews
- Stellt CI-Tests sicher

## Roadmap

### Geplante Verbesserungen

- [ ] Parallele Session-Verarbeitung
- [ ] Intelligenteres Retry-Handling
- [ ] Slack/Discord-Benachrichtigungen
- [ ] Dashboard für Session-Monitoring
- [ ] Erweiterte Fehleranalyse
- [ ] Kosten-Tracking für API-Nutzung

## Support

### Hilfe bekommen

1. **Dokumentation**: Diese Datei und Jules API Docs
2. **Issues**: GitHub Issues für Workflow-Probleme
3. **Jules Support**: [support@jules.google.com](mailto:support@jules.google.com)
4. **API-Referenz**: [developers.google.com/jules/api](https://developers.google.com/jules/api)

### Feedback

Feedback und Verbesserungsvorschläge sind willkommen:

- Öffnen Sie ein GitHub Issue
- Diskutieren Sie in GitHub Discussions
- Erstellen Sie einen Pull Request

## Beispiel: Kompletter Workflow-Durchlauf

### Schritt 1: Task wird gemerged

```bash
# Nach Merge von Task 12 auf main
git push origin main
```

### Schritt 2: Automatischer Start

```
Flow Jules 01 startet automatisch
↓
Findet Task 13 als nächste Aufgabe
↓
Erstellt Jules Session via API
↓
Erstellt Tracking-Issue #45
```

### Schritt 3: Jules arbeitet

```
Jules analysiert Aufgabe
↓
Erstellt Implementierungsplan
↓
Schreibt Code
↓
Erstellt Tests
↓
Aktualisiert Dokumentation
↓
Erstellt PR #46
```

### Schritt 4: Monitoring erkennt PR

```
Flow Jules 02 läuft alle 15 Minuten
↓
Erkennt neuen PR #46
↓
Aktualisiert Issue #45 mit PR-Link
↓
Triggert Copilot-Review
```

### Schritt 5: Copilot Review

```
Copilot Agent reviewt PR #46
↓
Findet kleinere Issues
↓
Macht Korrekturen direkt im PR
↓
Markiert als "copilot-reviewed"
```

### Schritt 6: CI Tests

```
GitHub Actions CI läuft
↓
Build erfolgreich
↓
Tests erfolgreich (180/180 passed)
↓
Code-Coverage 85%
```

### Schritt 7: Auto-Merge

```
Flow Jules 03 erkennt erfolgreiche Tests
↓
Verifiziert alle Checks
↓
Merged PR #46 → main
↓
Schließt Issue #45
↓
Kommentiert "Task completed"
```

### Schritt 8: Nächster Zyklus

```
Merge-Event triggert Flow Jules 01
↓
Findet Task 14 als nächste Aufgabe
↓
Zyklus beginnt von vorne...
```

## Lizenz und Hinweise

### Jules API

Die Jules API ist derzeit in der Alpha-Phase. Spezifikationen können sich ändern.

**Wichtig**: 
- API-Schlüssel können maximal 3 gleichzeitig existieren
- Exponierte API-Schlüssel werden automatisch deaktiviert
- Nutzung unterliegt den Google-Nutzungsbedingungen

### GitHub Copilot

Copilot-Funktionen erfordern eine aktive Copilot-Lizenz.

---

**Erstellt**: 2025-11-12  
**Version**: 1.0  
**Status**: Produktionsbereit  

Für weitere Informationen siehe:
- [Jules API Reference](https://developers.google.com/jules/api/reference/rest)
- [GitHub Actions Dokumentation](https://docs.github.com/en/actions)
- [Entwicklungsplan](../LIGHTJOCKEY_Entwicklungsplan.md)
