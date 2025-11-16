# Workflow-Status und Übersicht

## Aktive Workflows

Stand: 2025-11-12

### Issue-basierte AutoTask-Workflows

| Workflow | Datei | Status | Naming | Trigger |
|----------|-------|--------|--------|---------|
| **Flow AutoTask 01: Start** | `flow-autotask_01-start.yml` | ✅ Aktiv | ✅ Korrekt | Push to main |
| **Flow AutoTask 01b: Notify** | `flow-autotask_01b-notify-copilot.yml` | ✅ Aktiv | ✅ Korrekt | PR opened |
| **Flow AutoTask 02: Merge** | `flow-autotask_02-merge.yml` | ✅ Aktiv | ✅ Korrekt | CI completed |
| **Copilot Assign Agent** | `copilot-assign-agent.yml` | ✅ Aktiv | ✅ Korrekt | Issue labeled |

**Abhängigkeit**: `AUTOMATION_ENABLED` Variable muss `true` sein

**Naming-Schema**: ✅ Konform mit `flow-{prozess}_{schritt}-{aktion}.yml`

### Jules API Workflows

**Speicherort**: `.github/workflows/jules-api/`

| Workflow | Datei | Status | Naming | Trigger |
|----------|-------|--------|---------|---------|
| **Flow Jules 01: Submit** | `flow-jules_01-submit-task.yml` | ✅ Bereit | ✅ Korrekt | Push to main |
| **Flow Jules 02: Monitor** | `flow-jules_02-monitor-and-review.yml` | ✅ Bereit | ✅ Korrekt | Cron (15 min) |
| **Flow Jules 03: Merge** | `flow-jules_03-auto-merge.yml` | ✅ Bereit | ✅ Korrekt | PR review / CI |

**Abhängigkeit**: `JULES_AUTOMATION_ENABLED` Variable + `JulesAPIKey` Secret

**Naming-Schema**: ✅ Konform mit `flow-{prozess}_{schritt}-{aktion}.yml`

⚠️ **Hinweis**: Workflows wurden in Unterordner `jules-api/` verschoben für bessere Organisation.

### CI/CD Workflows

| Workflow | Datei | Status | Naming | Trigger |
|----------|-------|--------|--------|---------|
| **Flow CI 01: Build & Test** | `flow-ci_01-build-and-test.yml` | ✅ Aktiv | ✅ Korrekt | Push / PR |
| **Flow Release 01: MSI** | `flow-release_01-msi.yml` | ✅ Aktiv | ✅ Korrekt | Tag / Manual |
| **Build** | `build.yml` | ✅ Aktiv | ⚠️ Legacy | Push / PR |

**Naming-Schema**: 
- ✅ Neue Flows: `flow-{prozess}_{schritt}-{aktion}.yml`
- ⚠️ Legacy: `build.yml` (sollte zu `flow-ci_00-build.yml` umbenannt werden)

## Naming-Schema

### Aktuelles Schema

```
flow-{prozess}_{schritt}-{aktion}.yml
```

**Komponenten**:
- `flow-`: Präfix für alle Workflows
- `{prozess}`: autotask, jules, ci, release
- `_{schritt}`: 01, 02, 03, ... (chronologische Reihenfolge)
- `-{aktion}`: start, merge, build, test, etc.

### Beispiele

✅ **Korrekt**:
- `flow-autotask_01-start.yml`
- `flow-jules_02-monitor-and-review.yml`
- `flow-ci_01-build-and-test.yml`

❌ **Legacy** (zu migrieren):
- `build.yml` → `flow-ci_00-build.yml`

### Sonderfälle

Workflows ohne Flow-Präfix:
- `copilot-assign-agent.yml` - Spezial-Workflow für Copilot
  - ✅ Akzeptabel als Ausnahme

## Workflow-Abhängigkeiten

### Issue-basiert

```
flow-autotask_01-start.yml
    ↓
flow-autotask_01b-notify-copilot.yml
    ↓
copilot-assign-agent.yml
    ↓
[Manuelle Copilot-Aktivierung]
    ↓
flow-ci_01-build-and-test.yml
    ↓
flow-autotask_02-merge.yml
```

### Jules API

```
flow-jules_01-submit-task.yml
    ↓
[Jules arbeitet autonom]
    ↓
flow-jules_02-monitor-and-review.yml (Cron)
    ↓
[Copilot Review]
    ↓
flow-ci_01-build-and-test.yml
    ↓
flow-jules_03-auto-merge.yml
```

## Empfohlene Aktionen

### 1. Legacy-Workflow umbenennen

**Aktuell**:
```
.github/workflows/build.yml
```

**Empfohlen**:
```
.github/workflows/flow-ci_00-build.yml
```

**Änderungen**:
```bash
# Datei umbenennen
git mv .github/workflows/build.yml \
       .github/workflows/flow-ci_00-build.yml

# Badges in README.md aktualisieren
# Von: workflows/build.yml/badge.svg
# Zu:  workflows/flow-ci_00-build.yml/badge.svg
```

### 2. Workflow-Ordnerstruktur

**Aktuelle Struktur**:
```
.github/workflows/
├── flow-autotask_01-start.yml
├── flow-autotask_01b-notify-copilot.yml
├── flow-autotask_02-merge.yml
├── flow-ci_01-build-and-test.yml
├── flow-release_01-msi.yml
├── copilot-assign-agent.yml
├── build.yml
└── jules-api/
    ├── flow-jules_01-submit-task.yml
    ├── flow-jules_02-monitor-and-review.yml
    └── flow-jules_03-auto-merge.yml
```

**Alternative (optional)**:
```
.github/workflows/
├── autotask/
│   ├── flow-autotask_01-start.yml
│   ├── flow-autotask_01b-notify-copilot.yml
│   └── flow-autotask_02-merge.yml
├── jules-api/
│   ├── flow-jules_01-submit-task.yml
│   ├── flow-jules_02-monitor-and-review.yml
│   └── flow-jules_03-auto-merge.yml
├── ci-cd/
│   ├── flow-ci_00-build.yml
│   ├── flow-ci_01-build-and-test.yml
│   └── flow-release_01-msi.yml
└── copilot-assign-agent.yml
```

⚠️ **Hinweis**: Unterordner funktionieren in GitHub Actions, aber alle Workflows werden in der UI flach angezeigt.

### 3. README.md Badges aktualisieren

Nach Umbenennungen Badges aktualisieren:

```markdown
<!-- Aktuell -->
[![Build](https://github.com/MrLongNight/LightJockey/actions/workflows/build.yml/badge.svg)](...)

<!-- Nach Umbenennung -->
[![Build](https://github.com/MrLongNight/LightJockey/actions/workflows/flow-ci_00-build.yml/badge.svg)](...)
```

## Workflow-Konfiguration

### Erforderliche Variables

| Variable | Wert | Verwendung | Status |
|----------|------|------------|--------|
| `AUTOMATION_ENABLED` | `true` | Issue-basierte Workflows | ✅ Gesetzt |
| `JULES_AUTOMATION_ENABLED` | `true` | Jules API Workflows | ⏳ Zu setzen |

### Erforderliche Secrets

| Secret | Verwendung | Status |
|--------|------------|--------|
| `GITHUB_TOKEN` | Automatisch (für alle Workflows) | ✅ Auto |
| `CODECOV_TOKEN` | Code Coverage Upload | ✅ Gesetzt |
| `JulesAPIKey` | Jules API Authentifizierung | ⏳ Zu setzen |

## Monitoring

### Workflow Runs

Alle Workflow-Runs einsehen:
```bash
gh run list
```

Spezifischen Workflow:
```bash
gh run list --workflow=flow-autotask_01-start.yml
```

### Workflow-Status prüfen

```bash
# Alle Workflows
gh workflow list

# Spezifischer Workflow
gh workflow view flow-autotask_01-start.yml
```

### Workflow manuell starten

```bash
gh workflow run flow-autotask_01-start.yml
gh workflow run jules-api/flow-jules_01-submit-task.yml
```

## Migration Checklist

Wenn Legacy-Workflows umbenannt werden:

- [ ] Workflow-Datei umbenennen
- [ ] README.md Badges aktualisieren
- [ ] Dokumentation aktualisieren
- [ ] Branch-Protection-Rules anpassen
- [ ] Abhängige Workflows aktualisieren
- [ ] Testen mit manuellem Workflow-Run
- [ ] Team benachrichtigen

## Zusammenfassung

| Kategorie | Anzahl | Status |
|-----------|--------|--------|
| **Issue-basierte Workflows** | 4 | ✅ Alle aktiv |
| **Jules API Workflows** | 3 | ✅ Bereit (verschoben) |
| **CI/CD Workflows** | 3 | ✅ Alle aktiv |
| **Legacy Workflows** | 1 | ⚠️ Umbenennung empfohlen |
| **Gesamt** | 11 | ✅ Alle funktionsfähig |

### Naming Compliance

| Status | Anzahl | Workflows |
|--------|--------|-----------|
| ✅ Konform | 9 | flow-autotask_*, flow-jules_*, flow-ci_*, flow-release_* |
| ⚠️ Legacy | 1 | build.yml |
| ✅ Ausnahme | 1 | copilot-assign-agent.yml |

**Konformität**: 91% (10/11 Workflows)

---

**Erstellt**: 2025-11-12  
**Version**: 1.0  
**Status**: Workflows organisiert und überprüft  
**Nächste Aktion**: Jules API testen, Legacy-Workflow optional umbenennen
