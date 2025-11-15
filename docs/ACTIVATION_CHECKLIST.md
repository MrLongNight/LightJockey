# Activation Checklist: User Issue Automation

Diese Checkliste hilft bei der Aktivierung der neuen User Issue Automation.

## Voraussetzungen Prüfen

### 1. Jules API Setup
- [ ] Jules API Account erstellt bei https://jules.google.com
- [ ] Repository mit Jules verbunden
- [ ] Jules API Key erhalten

### 2. GitHub Repository Setup
- [ ] PR #[NUMBER] mit Issue Templates und Workflows gemergt
- [ ] Zugriff auf Repository Settings

## Aktivierung (Schritt für Schritt)

### Schritt 1: Repository Variable setzen

1. Gehe zu: **Settings** → **Secrets and variables** → **Actions**
2. Klicke auf Tab: **Variables**
3. Klicke: **New repository variable**
4. Setze:
   - Name: `JULES_AUTOMATION_ENABLED`
   - Value: `true`
5. Klicke: **Add variable**

✅ Variable ist gesetzt

### Schritt 2: Jules API Secret hinzufügen

1. Gehe zu: **Settings** → **Secrets and variables** → **Actions**
2. Klicke auf Tab: **Secrets**
3. Klicke: **New repository secret**
4. Setze:
   - Name: `JulesAPIKey`
   - Value: `[DEIN JULES API KEY]`
5. Klicke: **Add secret**

✅ Secret ist gesetzt

### Schritt 3: Issue Templates verifizieren

1. Gehe zu: **Issues** → **New Issue**
2. Prüfe ob folgende Templates erscheinen:
   - [ ] Bug Report
   - [ ] Feature Request
3. Öffne beide Templates und prüfe Felder

✅ Templates werden angezeigt

### Schritt 4: Workflows verifizieren

1. Gehe zu: **Actions**
2. Prüfe ob folgende Workflows sichtbar sind:
   - [ ] USER-ISSUE.01_Process-Bug-Report_AUTO.yml
   - [ ] USER-ISSUE.02_Process-Feature-Request_AUTO.yml
   - [ ] USER-ISSUE.03_Notify-Test-Failures_AUTO.yml

✅ Workflows sind sichtbar

## Test der Automation

### Test 1: Bug Report (Optional aber empfohlen)

1. Gehe zu: **Issues** → **New Issue**
2. Wähle: **Bug Report**
3. Fülle Formular aus mit Test-Daten
4. Titel: `[TEST] Bug Report Automation`
5. Submit Issue
6. Warte 1-2 Minuten
7. Prüfe:
   - [ ] Bestätigungskommentar wurde gepostet
   - [ ] Label `jules-processing` wurde hinzugefügt
   - [ ] Workflow läuft in Actions Tab

**Bei Erfolg:**
- ✅ Bug Report Automation funktioniert
- Issue kann geschlossen werden

**Bei Fehler:**
- Prüfe Workflow-Logs in Actions
- Prüfe Variables/Secrets
- Siehe Troubleshooting unten

### Test 2: Feature Request (Optional aber empfohlen)

1. Gehe zu: **Issues** → **New Issue**
2. Wähle: **Feature Request**
3. Fülle Formular aus mit Test-Daten
4. Titel: `[TEST] Feature Request Automation`
5. Submit Issue
6. Warte 1 Minute
7. Prüfe:
   - [ ] Approval Request Kommentar wurde gepostet
   - [ ] Issue wurde dir zugewiesen
   - [ ] Label `needs-approval` ist gesetzt

**Genehmigung testen:**
8. Füge Label `approved-for-jules` hinzu
9. Warte 1-2 Minuten
10. Prüfe:
    - [ ] Approval Confirmation Kommentar wurde gepostet
    - [ ] Label `needs-approval` wurde entfernt
    - [ ] Label `jules-processing` wurde hinzugefügt
    - [ ] Workflow läuft in Actions Tab

**Bei Erfolg:**
- ✅ Feature Request Automation funktioniert
- Issue kann geschlossen werden

**Bei Fehler:**
- Prüfe Workflow-Logs in Actions
- Prüfe Variables/Secrets
- Siehe Troubleshooting unten

### Test 3: Test Failure Notification (Optional)

Dieser Test erfordert einen tatsächlich fehlgeschlagenen Test und ist komplexer.

**Überspringen wenn:**
- Tests 1 und 2 funktionieren
- Du Jules Automation vertraust

**Manuell testen wenn gewünscht:**
1. Warte auf ersten echten Bug Report
2. Prüfe ob Benachrichtigung kommt wenn Tests fehlschlagen

## Nach erfolgreicher Aktivierung

### Benutzern mitteilen

Erstelle eine Ankündigung:

```markdown
# 🎉 Neue Feature: Automatische Issue-Verarbeitung

Ab sofort werden Bug Reports und Feature Requests automatisch von unserem
KI-Agenten Jules verarbeitet!

## Was bedeutet das für dich?

**Bug Reports:**
- 🐛 Melde Bugs über das Bug Report Template
- ✅ Automatische Behebung in Minuten bis Stunden
- 📝 Du erhältst Updates über den Fortschritt

**Feature Requests:**
- 💡 Schlage Features über das Feature Request Template vor
- 👀 Ein Maintainer prüft deinen Vorschlag
- ✅ Bei Genehmigung: Automatische Implementierung
- 📝 Du wirst über jeden Schritt informiert

Mehr Infos: [docs/USER_ISSUE_QUICKSTART.md](docs/USER_ISSUE_QUICKSTART.md)
```

Poste in:
- [ ] GitHub Discussions
- [ ] README (optional)
- [ ] Discord/Slack (wenn vorhanden)

### Monitoring einrichten

Empfohlen (optional):
1. GitHub Actions Benachrichtigungen aktivieren
2. Workflow-Fehler per Email abonnieren
3. Regelmäßig Issues mit Labels prüfen:
   ```bash
   gh issue list --label "needs-manual-review"
   gh issue list --label "tests-failed"
   ```

## Troubleshooting

### Problem: Workflow läuft nicht

**Prüfe:**
1. Variable `JULES_AUTOMATION_ENABLED` = `"true"` (mit Anführungszeichen)
2. Secret `JulesAPIKey` ist gesetzt
3. Issue hat die richtigen Labels
4. Workflow-Datei ist im main branch

**Lösung:**
- Korrigiere Konfiguration
- Re-trigger Workflow durch Label-Änderung

### Problem: Jules Session erstellt, aber kein PR

**Ursache:**
- Jules braucht Zeit (kann Stunden dauern)
- Jules könnte Probleme haben

**Lösung:**
1. Warte 4-6 Stunden
2. Prüfe Jules Session URL (im Issue-Kommentar)
3. Wenn immer noch kein PR: Manuelle Bearbeitung

### Problem: Benutzer erhält keine Benachrichtigungen

**Ursache:**
- Workflow-Run-Trigger funktioniert nicht
- Issue ist nicht korrekt verlinkt

**Lösung:**
- Benutzer manuell benachrichtigen
- Issue mit PR verlinken

### Problem: Test-Benachrichtigungen kommen mehrfach

**Ursache:**
- CI läuft mehrmals

**Lösung:**
- Normal, wenn Tests mehrmals fehlschlagen
- Workflow ist idempotent (wiederholbar)

## Support

Bei Problemen:
1. Prüfe Workflow-Logs in Actions Tab
2. Siehe [docs/USER_ISSUE_AUTOMATION.md](docs/USER_ISSUE_AUTOMATION.md)
3. Siehe [docs/IMPLEMENTATION_SUMMARY.md](docs/IMPLEMENTATION_SUMMARY.md)
4. Erstelle Issue falls weiterhin Probleme

## Nach 1 Woche: Review

Nach einer Woche Betrieb prüfen:

- [ ] Wie viele Bug Reports wurden automatisch behoben?
- [ ] Wie viele Feature Requests wurden eingereicht?
- [ ] Wie viele wurden genehmigt/abgelehnt?
- [ ] Wie oft schlagen Tests fehl?
- [ ] Funktionieren Benachrichtigungen zuverlässig?
- [ ] Ist Benutzer-Feedback positiv?

**Bei Problemen:**
- Automation kann temporär deaktiviert werden (Variable auf `false`)
- Anpassungen an Workflows können gemacht werden
- Dokumentation kann verbessert werden

**Bei Erfolg:**
- ✅ System läuft stabil
- Kann so weiterlaufen
- Evtl. weitere Optimierungen

## Deaktivierung (falls nötig)

Falls Automation deaktiviert werden muss:

1. Gehe zu: **Settings** → **Secrets and variables** → **Actions** → **Variables**
2. Bearbeite `JULES_AUTOMATION_ENABLED`
3. Setze auf: `false`
4. Speichern

**Auswirkung:**
- Workflows laufen nicht mehr
- Issue Templates bleiben verfügbar
- Kann jederzeit wieder aktiviert werden

## Fazit

Nach Abschluss dieser Checkliste ist die User Issue Automation aktiv und bereit:

✅ Issue Templates verfügbar
✅ Bug Reports werden automatisch verarbeitet  
✅ Feature Requests haben Approval-Workflow
✅ Test-Fehler-Benachrichtigungen funktionieren
✅ Dokumentation ist verfügbar
✅ Benutzer können loslegen

**Viel Erfolg mit der automatisierten Issue-Verarbeitung! 🎉**
