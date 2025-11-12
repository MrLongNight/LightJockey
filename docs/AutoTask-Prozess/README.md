# AutoTask-Prozess Dokumentation

Dieser Ordner enthält die Dokumentation für die automatisierten Task-Verarbeitungsprozesse in LightJockey.

## Übersicht

LightJockey verwendet zwei verschiedene Automatisierungsansätze:

1. **Issue-basierter Prozess** (~95% Automatisierung)
2. **Jules API-basierter Prozess** (~98% Automatisierung)

## Dokumentation

### [Issue-basierter-Prozess.md](Issue-basierter-Prozess.md)
Beschreibt den ersten AutoTask-Ansatz mit GitHub Issues und Copilot Agent.

**Highlights**:
- GitHub-native Lösung
- 1 manueller Klick erforderlich
- Einfaches Setup
- Bewährt und stabil

### [Google Jules API-Referenz.md](Google%20Jules%20API-Referenz.md)
Vollständige API-Referenz für die Google Jules API.

**Inhalt**:
- Alle API-Endpunkte
- Authentifizierung
- Request/Response Beispiele
- Fehlerbehandlung
- Sicherheits-Best-Practices

### Jules API Integration (im docs-Hauptverzeichnis)

Die vollständige Jules API Dokumentation befindet sich im Hauptverzeichnis:
- `JULES_API_INTEGRATION.md` - Kompletter Guide
- `JULES_API_QUICKSTART.md` - Quick Start
- `JULES_API_SECURITY_REVIEW.md` - Sicherheitsanalyse
- `AUTOMATION_WORKFLOWS_COMPARISON.md` - Vergleich aller Prozesse

## Workflows

Die Workflow-Dateien befinden sich in `.github/workflows/`:

### Issue-basierte Workflows
- `flow-autotask_01-start.yml` - Task finden und Issue/PR erstellen
- `flow-autotask_01b-notify-copilot.yml` - Copilot-Benachrichtigungen
- `flow-autotask_02-merge.yml` - Automatisches Mergen
- `copilot-assign-agent.yml` - Copilot-Agent Zuweisung

### Jules API Workflows
- `flow-jules_01-submit-task.yml` - Task an Jules API senden
- `flow-jules_02-monitor-and-review.yml` - Session überwachen
- `flow-jules_03-auto-merge.yml` - Nach Review mergen

### CI/CD Workflows
- `flow-ci_01-build-and-test.yml` - Build und Tests
- `flow-release_01-msi.yml` - MSI Package erstellen

## Schnellstart

### Issue-basierter Prozess

```bash
# 1. Automation aktivieren
gh variable set AUTOMATION_ENABLED --body "true"

# 2. Push to main (triggert Workflow)
git push origin main

# 3. Issue öffnen und Copilot aktivieren (1 Klick)
# → Copilot implementiert automatisch
# → PR wird automatisch gemerged
```

### Jules API Prozess

```bash
# 1. Jules API Key generieren
# → jules.google.com/settings#api

# 2. Secret setzen
gh secret set JulesAPIKey

# 3. Variable setzen
gh variable set JULES_AUTOMATION_ENABLED --body "true"

# 4. Push to main
git push origin main
# → Komplett automatisch, kein manueller Schritt!
```

## Vergleich

| Feature | Issue-basiert | Jules API |
|---------|---------------|-----------|
| Automatisierung | 95% | 98% |
| Setup | 1 Variable | 1 Secret + 1 Variable |
| Manueller Schritt | 1 Klick | Keiner |
| Abhängigkeiten | GitHub + Copilot | GitHub + Copilot + Jules |
| Kosten | $20/mo | $20+/mo |
| Empfohlen für | Einfachheit | Max. Automation |

## Scripts

### jules_api_helper.py

CLI-Tool für Jules API Interaktion:

```bash
# Source finden
./scripts/jules_api_helper.py get-source \
  --owner MrLongNight --repo LightJockey

# Session erstellen
./scripts/jules_api_helper.py create-session \
  --source "sources/github/MrLongNight/LightJockey" \
  --title "Task 13" \
  --prompt "Implement cloud backup"

# Session überwachen
./scripts/jules_api_helper.py monitor SESSION_ID
```

Siehe: [scripts/README.md](../../scripts/README.md)

## Support

Bei Problemen oder Fragen:

1. **Dokumentation prüfen**:
   - Issue-basiert: [Issue-basierter-Prozess.md](Issue-basierter-Prozess.md)
   - Jules API: [../JULES_API_INTEGRATION.md](../JULES_API_INTEGRATION.md)

2. **Troubleshooting**:
   - [../AUTO_TASK_TROUBLESHOOTING.md](../AUTO_TASK_TROUBLESHOOTING.md)

3. **GitHub Issues**:
   - Für LightJockey-spezifische Probleme

## Status

- **Issue-basierter Prozess**: ✅ Aktiv in Produktion
- **Jules API Prozess**: ✅ Implementiert, bereit zum Testen

---

**Letzte Aktualisierung**: 2025-11-12  
**Maintainer**: MrLongNight  
**Lizenz**: Siehe Haupt-Repository
