# Quick Start: User Issue Automation

Diese Anleitung zeigt, wie die neuen Issue-Templates und die Jules-Automation verwendet werden.

## Für Benutzer

### Einen Bug melden

1. Gehe zu **Issues** → **New Issue**
2. Wähle **Bug Report**
3. Fülle das Formular aus:
   - Fehlerbeschreibung
   - Schritte zur Reproduktion
   - Erwartetes vs. tatsächliches Verhalten
   - Umgebung (optional)
   - Logs/Screenshots (optional)
4. Klicke auf **Submit new issue**

**Was passiert dann?**
- ✅ Du erhältst sofort eine Bestätigung
- 🤖 Jules beginnt automatisch mit der Fehleranalyse
- 📝 Ein Pull Request wird automatisch erstellt
- ✅ Nach erfolgreichen Tests wird der Fix automatisch gemergt
- ⏱️ **Dauer:** Meist innerhalb von Minuten bis Stunden

**Wenn Tests fehlschlagen:**
- ⚠️ Du erhältst eine Benachrichtigung
- 💬 Ein Maintainer wird benachrichtigt
- ⏳ Es kann zu einer Wartezeit kommen (1-3 Tage)

### Ein Feature vorschlagen

1. Gehe zu **Issues** → **New Issue**
2. Wähle **Feature Request**
3. Fülle das Formular aus:
   - Problem/Bedarf
   - Vorgeschlagene Lösung
   - Vorteile
   - Priorität (deine Einschätzung)
   - Anwendungsfälle
   - Alternative Lösungen (optional)
   - Mockups (optional)
4. Klicke auf **Submit new issue**

**Was passiert dann?**
- 📋 Du erhältst eine Bestätigung
- 👀 Ein Maintainer prüft deinen Vorschlag
- ⏳ **Wartezeit:** 1-7 Tage für Genehmigung

**Wenn genehmigt:**
- ✅ Du wirst benachrichtigt
- 🤖 Jules beginnt mit der Implementierung
- 📝 Ein Pull Request wird automatisch erstellt
- ✅ Nach erfolgreichen Tests wird das Feature automatisch gemergt

**Wenn abgelehnt:**
- ❌ Issue wird geschlossen mit Begründung

## Für Maintainer

### Bug Reports genehmigen

**Keine Aktion erforderlich!** Bug Reports werden vollautomatisch verarbeitet.

**Nur eingreifen wenn:**
- Tests fehlschlagen (du wirst benachrichtigt)
- Jules die Aufgabe nicht versteht
- Benutzer zusätzliche Informationen braucht

### Feature Requests genehmigen

1. **Issue-Benachrichtigung erhalten**
   - Du wirst automatisch erwähnt
   - Issue wird dir zugewiesen

2. **Prüfe den Feature Request**
   - Ist es machbar?
   - Passt es zur Roadmap?
   - Ist es sinnvoll?

3. **Genehmigung erteilen**
   ```bash
   # Option 1: Via GitHub Web UI
   # Gehe zum Issue → Labels → "approved-for-jules" hinzufügen
   
   # Option 2: Via GitHub CLI
   gh issue edit <issue-number> --add-label "approved-for-jules"
   ```

4. **Ablehnung**
   - Schließe das Issue
   - Füge einen Kommentar mit Begründung hinzu
   - Sei freundlich und konstruktiv!

**Nach Genehmigung:**
- Jules startet automatisch
- Du musst nichts weiter tun
- Bei Test-Fehlern wirst du benachrichtigt

### Test-Fehler beheben

Wenn Tests fehlschlagen:

1. **Benachrichtigung erhalten**
   - Im Issue
   - Im Pull Request
   - Via GitHub-Benachrichtigung

2. **Logs prüfen**
   - Klicke auf den Workflow-Link
   - Prüfe welche Tests fehlgeschlagen sind
   - Analysiere die Ursache

3. **Fehler beheben**
   - Korrigiere den Code im PR
   - Pushe die Änderungen
   - Tests laufen automatisch erneut

4. **Benutzer informieren**
   - Kommentiere im Issue
   - Erkläre was passiert ist
   - Gib eine Zeiteinschätzung

## Labels

### Automatisch vergeben

**Bug Reports:**
- `bug` - Von Template
- `jules-auto-process` - Von Template
- `jules-processing` - Von Workflow
- `tests-failed` - Bei Test-Fehler
- `needs-manual-review` - Bei Test-Fehler

**Feature Requests:**
- `enhancement` - Von Template
- `needs-approval` - Von Template
- `approved-for-jules` - Von Maintainer (triggert Jules)
- `jules-processing` - Von Workflow
- `tests-failed` - Bei Test-Fehler
- `needs-manual-review` - Bei Test-Fehler

### Manuell vergeben

- `approved-for-jules` - Genehmigung für Feature Request

## Monitoring

### Aktive Issues überwachen

```bash
# Alle Bug Reports in Bearbeitung
gh issue list --label "bug,jules-processing"

# Alle Feature Requests, die auf Genehmigung warten
gh issue list --label "enhancement,needs-approval"

# Alle Issues mit fehlgeschlagenen Tests
gh issue list --label "tests-failed"

# Alle Issues, die manuelle Prüfung benötigen
gh issue list --label "needs-manual-review"
```

### PRs überwachen

```bash
# Alle Jules-PRs
gh pr list --label "jules-pr"

# Alle PRs mit fehlgeschlagenen Tests
gh pr list --label "tests-failed"
```

## Troubleshooting

### Problem: Bug Report wird nicht automatisch verarbeitet

**Prüfe:**
1. Hat das Issue die richtigen Labels? (`bug`, `jules-auto-process`)
2. Ist `JULES_AUTOMATION_ENABLED` auf `true` gesetzt?
3. Ist `JulesAPIKey` Secret konfiguriert?
4. Prüfe Workflow-Logs in GitHub Actions

**Lösung:**
- Labels manuell hinzufügen
- Variables/Secrets prüfen
- Workflow manuell neu triggern

### Problem: Feature Request wird nach Genehmigung nicht verarbeitet

**Prüfe:**
1. Wurde das Label `approved-for-jules` hinzugefügt?
2. Ist `JULES_AUTOMATION_ENABLED` auf `true` gesetzt?
3. Ist `JulesAPIKey` Secret konfiguriert?
4. Prüfe Workflow-Logs

**Lösung:**
- Label erneut hinzufügen (triggert Workflow)
- Variables/Secrets prüfen

### Problem: Benutzer erhält keine Benachrichtigungen bei Test-Fehlern

**Prüfe:**
1. Ist der Workflow `USER-ISSUE.03` aktiv?
2. Hat der PR die richtigen Labels?
3. Ist das Issue noch offen?

**Lösung:**
- Benutzer manuell benachrichtigen
- Issue-Verlinkung im PR prüfen

### Problem: Jules versteht die Aufgabe nicht

**Symptome:**
- PR enthält irrelevanten Code
- Tests schlagen fehl
- Implementierung passt nicht zur Beschreibung

**Lösung:**
1. Schließe den PR
2. Kommentiere im Original-Issue
3. Bitte Benutzer um mehr Details
4. Wenn genug Info: Label `approved-for-jules` erneut hinzufügen

## Best Practices

### Für Benutzer

**Bug Reports:**
- Sei so detailliert wie möglich
- Füge Screenshots/Logs hinzu
- Beschreibe Reproduktionsschritte klar
- Gib Umgebungsinformationen an

**Feature Requests:**
- Erkläre WARUM du die Funktion brauchst
- Beschreibe konkrete Anwendungsfälle
- Sei realistisch bei der Priorität
- Füge Mockups hinzu wenn möglich

### Für Maintainer

**Feature Requests prüfen:**
- Antworte innerhalb von 3 Tagen
- Sei transparent bei Ablehnung
- Frage nach wenn Details fehlen
- Priorisiere nach Roadmap

**Bei Test-Fehlern:**
- Reagiere schnell (innerhalb 1 Tag)
- Kommuniziere klar
- Gib Zeiteinschätzungen
- Erkläre was schiefgelaufen ist

## Weitere Hilfe

- 📚 [Vollständige Dokumentation](./USER_ISSUE_AUTOMATION.md)
- 💬 [GitHub Discussions](https://github.com/MrLongNight/LightJockey/discussions)
- 🐛 [Issues](https://github.com/MrLongNight/LightJockey/issues)
