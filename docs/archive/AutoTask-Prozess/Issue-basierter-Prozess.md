# AutoTask-Prozess: Issue-basierte Automatisierung

## Übersicht

Dieser Prozess beschreibt den ersten AutoTask-Ansatz für LightJockey, der Tasks automatisch basierend auf GitHub Issues bearbeitet.

## Prozess-Architektur

```
┌─────────────────────────────────────────────────────────┐
│ 1. Push to main → Trigger Workflow                     │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 2. flow-autotask_01-start.yml                          │
│    - Development Plan parsen                           │
│    - Nächsten Task finden                              │
│    - Feature-Branch erstellen                          │
│    - GitHub Issue erstellen                            │
│    - Draft Pull Request erstellen                      │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 3. flow-autotask_01b-notify-copilot.yml                │
│    - Benachrichtigung auf PR                           │
│    - Copilot-Anweisungen hinzufügen                    │
│    - Repository-Owner zuweisen                         │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 4. ⚠️ MANUELL: Copilot aktivieren                      │
│    - Issue öffnen                                       │
│    - "Assign to Copilot" klicken                       │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 5. Copilot Agent implementiert                         │
│    - Code schreiben                                     │
│    - Tests erstellen                                    │
│    - Dokumentation aktualisieren                        │
│    - Commits pushen                                     │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 6. flow-ci_01-build-and-test.yml                       │
│    - Solution bauen                                     │
│    - Unit-Tests ausführen                               │
│    - Code-Coverage prüfen                               │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 7. flow-autotask_02-merge.yml                          │
│    - PR von Draft zu Ready konvertieren                │
│    - Automerge-Label hinzufügen                        │
│    - PR automatisch mergen                             │
│    - Issue schließen                                   │
└───────────────────────┬─────────────────────────────────┘
                        ▼
                    ⟲ REPEAT
```

## Workflow-Details

### 1. flow-autotask_01-start.yml

**Datei**: `.github/workflows/flow-autotask_01-start.yml`

**Trigger**:
- Push auf `main` Branch
- Manueller Workflow-Dispatch

**Aufgaben**:

1. **Automatisierungs-Check**:
```yaml
- name: Check if automation is enabled
  run: |
    if [ "${{ vars.AUTOMATION_ENABLED }}" != "true" ]; then
      echo "Automation is disabled"
      exit 0
    fi
```

2. **Development Plan parsen**:
```python
# Python-Script im Workflow
# Liest LIGHTJOCKEY_Entwicklungsplan.md
# Findet ersten unerledigten Task
# Extrahiert Task-Nummer und Titel
```

3. **Feature-Branch erstellen**:
```bash
BRANCH_NAME="feature/task-${TASK_NUM}-autogenerated"
git checkout -b "$BRANCH_NAME"
git push origin "$BRANCH_NAME"
```

4. **GitHub Issue erstellen**:
```javascript
const issue = await github.rest.issues.create({
  title: `Task ${taskNum}: ${taskTitle}`,
  body: `## Auto-generated Task\n\n${taskText}`,
  labels: ['autogenerated', 'copilot-task']
});
```

5. **Draft Pull Request erstellen**:
```javascript
const pr = await github.rest.pulls.create({
  title: `Task ${taskNum}: ${taskTitle}`,
  head: branchName,
  base: 'main',
  body: `Closes #${issueNumber}`,
  draft: true
});
```

**Outputs**:
- Issue-Nummer
- PR-Nummer
- Branch-Name
- Task-Informationen

### 2. flow-autotask_01b-notify-copilot.yml

**Datei**: `.github/workflows/flow-autotask_01b-notify-copilot.yml`

**Trigger**:
- PR opened mit `autogenerated` Label

**Aufgaben**:

1. **PR-Kommentar mit Anweisungen**:
```javascript
await github.rest.issues.createComment({
  issue_number: prNumber,
  body: `## 🤖 Ready for Copilot
  
  @copilot Please implement this task according to:
  - MVVM pattern
  - Add unit tests
  - Update documentation
  - Follow C# best practices
  `
});
```

2. **PR-Beschreibung aktualisieren**:
```javascript
await github.rest.pulls.update({
  pull_number: prNumber,
  body: enhancedDescription
});
```

3. **Repository-Owner zuweisen**:
```javascript
await github.rest.issues.addAssignees({
  issue_number: prNumber,
  assignees: [context.repo.owner]
});
```

### 3. Manuelle Copilot-Aktivierung

**Status**: ⚠️ Manueller Schritt erforderlich

**Schritte**:
1. Benutzer erhält Benachrichtigung über neues Issue
2. Öffnet das Issue in GitHub
3. Klickt "Assign to Copilot" Button
4. Copilot Agent startet automatisch

**Zeitaufwand**: ~5 Sekunden (1 Klick)

**Alternative Methoden**:
```bash
# Via GitHub CLI
gh copilot issue assign ISSUE_NUMBER

# Via Label
gh issue edit ISSUE_NUMBER --add-label "copilot:implement"
```

### 4. Copilot Agent Implementierung

**Automatisch**: Copilot arbeitet vollständig autonom

**Aktionen**:
1. Analysiert Task-Beschreibung
2. Erstellt Implementierungsplan
3. Schreibt Production Code
4. Erstellt Unit-Tests
5. Aktualisiert Dokumentation
6. Pusht Commits zum PR-Branch

**Dauer**: 10-20 Minuten (je nach Komplexität)

**Qualitätskriterien**:
- MVVM-Pattern einhalten
- Unit-Tests >80% Coverage
- XML-Dokumentation
- Best Practices befolgen

### 5. flow-ci_01-build-and-test.yml

**Datei**: `.github/workflows/flow-ci_01-build-and-test.yml`

**Trigger**:
- Push zu PR-Branch
- Pull Request

**Aufgaben**:

1. **Build**:
```bash
dotnet build --configuration Release
```

2. **Tests**:
```bash
dotnet test --configuration Release --no-build
```

3. **Coverage**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

4. **Codecov Upload**:
```bash
curl -s https://codecov.io/bash | bash
```

**Erfolgs-Kriterien**:
- Build erfolgreich (0 Errors)
- Alle Tests grün (179+ Tests)
- Coverage >80%

### 6. flow-autotask_02-merge.yml

**Datei**: `.github/workflows/flow-autotask_02-merge.yml`

**Trigger**:
- Workflow-Run completed (flow-ci_01)
- Conclusion: success

**Aufgaben**:

1. **PR-Informationen abrufen**:
```javascript
const prs = await github.rest.pulls.list({
  state: 'open',
  head: `${owner}:${branch}`
});
```

2. **Autogenerated-Label prüfen**:
```javascript
const hasAutoLabel = pr.labels.some(
  label => label.name === 'autogenerated'
);
```

3. **Draft zu Ready konvertieren**:
```javascript
await github.graphql(`
  mutation($pullRequestId: ID!) {
    markPullRequestReadyForReview(
      input: {pullRequestId: $pullRequestId}
    ) { pullRequest { id } }
  }
`, { pullRequestId: pr.data.node_id });
```

4. **Automerge-Label hinzufügen**:
```javascript
await github.rest.issues.addLabels({
  issue_number: prNumber,
  labels: ['automerge']
});
```

5. **PR mergen**:
```javascript
await github.rest.pulls.merge({
  pull_number: prNumber,
  merge_method: 'squash',
  commit_title: `Auto-merge: PR #${prNumber}`
});
```

6. **Issue schließen** (automatisch via "Closes #X")

## Konfiguration

### Repository Variables

```bash
# Automation aktivieren
AUTOMATION_ENABLED=true
```

Setzen unter: `Settings` → `Secrets and variables` → `Actions` → `Variables`

### Repository Secrets

Keine speziellen Secrets für Issue-basierten Prozess erforderlich.

Standard GitHub Token wird automatisch bereitgestellt:
```yaml
permissions:
  contents: write
  issues: write
  pull-requests: write
```

### Branch Protection

Empfohlene Settings für `main` Branch:
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
  - flow-ci_01-build-and-test
- ✅ Require branches to be up to date before merging

## Metriken

### Automatisierungsgrad

| Phase | Automatisiert | Manuell | Status |
|-------|---------------|---------|--------|
| Issue/PR-Erstellung | 100% | 0% | ✅ Auto |
| Copilot-Aktivierung | 0% | 100% | ⚠️ Manuell |
| Implementierung | 100% | 0% | ✅ Auto |
| CI/CD | 100% | 0% | ✅ Auto |
| Merge | 100% | 0% | ✅ Auto |
| **Gesamt** | **~95%** | **~5%** | ⚠️ 1 Klick |

### Zeitaufwand pro Task

| Aktivität | Zeit | Typ |
|-----------|------|-----|
| Issue/PR erstellen | 30 Sek | Auto |
| Copilot aktivieren | 5 Sek | Manuell |
| Implementierung | 10-20 Min | Auto |
| CI Tests | 2-3 Min | Auto |
| Merge | 30 Sek | Auto |
| **Total** | **13-24 Min** | **~95% Auto** |

Vs. Manuell: 2-4 Stunden → **Zeitersparnis: 85%**

## Labels

### Automatische Labels

- `autogenerated`: Von Workflow erstellt
- `copilot-task`: Für Copilot-Aktivierung
- `automerge`: Bereit für Auto-Merge

### Manuelle Labels (optional)

- `priority:high`: Hohe Priorität
- `documentation`: Nur Doku-Änderungen
- `bug`: Bugfix statt Feature

## Troubleshooting

### Problem: "Automation is disabled"

**Lösung**:
```bash
# Repository Variable setzen
gh variable set AUTOMATION_ENABLED --body "true"
```

### Problem: Issue wurde erstellt, aber nichts passiert

**Ursache**: Copilot wurde nicht manuell aktiviert

**Lösung**:
1. Issue öffnen
2. "Assign to Copilot" klicken
3. Warten auf Copilot-Implementierung

### Problem: PR wird nicht gemerged

**Mögliche Ursachen**:
1. CI-Tests fehlgeschlagen
   - Logs in Actions-Tab prüfen
   - Tests lokal ausführen
2. Kein `autogenerated` Label
   - Label manuell hinzufügen
3. Branch-Protection-Rules
   - Settings prüfen

### Problem: Falscher Task wird erstellt

**Ursache**: Development Plan nicht aktuell

**Lösung**:
1. `LIGHTJOCKEY_Entwicklungsplan.md` prüfen
2. Checklist aktualisieren (✅ für erledigte Tasks)
3. Workflow erneut ausführen

## Vergleich: Issue-basiert vs. Jules API

| Aspekt | Issue-basiert | Jules API |
|--------|---------------|-----------|
| Automatisierung | ~95% | ~98% |
| Setup-Komplexität | Niedrig | Mittel |
| Manueller Schritt | 1 Klick | Keine |
| API-Abhängigkeit | Keine | Jules API |
| Kosten | $0 + Copilot | $0 + Copilot + Jules |
| Monitoring | GitHub UI | API + GitHub |
| Fehlerbehandlung | GitHub Native | Custom |

**Empfehlung**:
- **Issue-basiert**: Für Teams, die GitHub-Native-Lösungen bevorzugen
- **Jules API**: Für maximale Automatisierung und API-Integration

## Nächste Schritte

### Aktivierung

1. **Repository Variable setzen**:
```bash
gh variable set AUTOMATION_ENABLED --body "true"
```

2. **Push to main**:
```bash
git commit -m "Enable automation"
git push origin main
```

3. **Warten auf Workflow**:
   - Issue wird erstellt
   - PR wird erstellt
   - Benachrichtigung erhalten

4. **Copilot aktivieren**:
   - Issue öffnen
   - "Assign to Copilot" klicken

5. **Zurücklehnen**:
   - Copilot implementiert
   - CI testet
   - PR wird gemerged
   - Nächster Task startet

### Optimierungen

Mögliche Verbesserungen:
- [ ] Slack/Discord Benachrichtigungen
- [ ] Automatische Screenshots
- [ ] Performance-Metriken
- [ ] Custom Prompts pro Task-Typ
- [ ] Rollback bei Fehlern

## Links

- **Workflows**: [.github/workflows/](../../.github/workflows/)
- **Development Plan**: [LIGHTJOCKEY_Entwicklungsplan.md](../../LIGHTJOCKEY_Entwicklungsplan.md)
- **Copilot Docs**: [GitHub Copilot](https://github.com/features/copilot)
- **Troubleshooting**: [AUTO_TASK_TROUBLESHOOTING.md](../AUTO_TASK_TROUBLESHOOTING.md)

---

**Erstellt**: 2025-11-12  
**Version**: 1.0  
**Status**: Aktiv in Produktion  
**Automatisierung**: 95% (1 manueller Klick)  
**Letzte Aktualisierung**: 2025-11-12
