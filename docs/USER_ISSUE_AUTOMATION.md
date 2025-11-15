# User Issue Automation mit Jules

Dieses Dokument beschreibt die automatisierte Verarbeitung von Benutzer-Issues (Bug Reports und Feature Requests) mit Google Jules.

## Überblick

LightJockey bietet zwei verschiedene Arten von Issue-Templates mit unterschiedlichen Automatisierungsprozessen:

1. **Bug Reports** → Vollautomatisch verarbeitet
2. **Feature Requests** → Benötigen manuelle Genehmigung

## Bug Reports (Vollautomatisch)

### Prozess

Wenn ein Benutzer einen Bug Report erstellt:

1. **Benutzer erstellt Bug Report**
   - Nutzt das "Bug Report" Template
   - Issue erhält automatisch die Labels `bug` und `jules-auto-process`

2. **Automatische Verarbeitung** (Workflow: `USER-ISSUE.01_Process-Bug-Report_AUTO.yml`)
   - Jules API Session wird sofort erstellt
   - Jules analysiert den Bug und entwickelt eine Lösung
   - Pull Request wird automatisch erstellt

3. **Tests und Merge**
   - CI-Tests laufen automatisch
   - Bei Erfolg: Automatisches Mergen
   - Bei Fehler: Benutzer wird benachrichtigt (siehe unten)

### Benachrichtigungen

Der Benutzer erhält automatische Updates:
- ✅ Bestätigung der Verarbeitung
- 🔄 Jules Session erstellt
- ⚠️ Falls Tests fehlschlagen
- ✅ Erfolgreiche Implementierung

## Feature Requests (Mit Genehmigung)

### Prozess

Wenn ein Benutzer einen Feature Request erstellt:

1. **Benutzer erstellt Feature Request**
   - Nutzt das "Feature Request" Template
   - Issue erhält automatisch die Labels `enhancement` und `needs-approval`

2. **Genehmigungsprozess** (Workflow: `USER-ISSUE.02_Process-Feature-Request_AUTO.yml`)
   - Maintainer erhält Benachrichtigung
   - Issue wird dem Repository Owner zugewiesen
   - Maintainer prüft den Vorschlag

3. **Bei Genehmigung**
   - Maintainer fügt Label `approved-for-jules` hinzu
   - Jules API Session wird automatisch erstellt
   - Jules implementiert die Funktion
   - Pull Request wird automatisch erstellt

4. **Bei Ablehnung**
   - Maintainer schließt das Issue mit Kommentar
   - Keine automatische Verarbeitung

5. **Tests und Merge**
   - CI-Tests laufen automatisch
   - Bei Erfolg: Automatisches Mergen
   - Bei Fehler: Benutzer wird benachrichtigt

### Benachrichtigungen

Der Benutzer erhält automatische Updates:
- 📋 Bestätigung des Eingangs
- ✅ Genehmigung oder ❌ Ablehnung
- 🔄 Jules Session erstellt (nach Genehmigung)
- ⚠️ Falls Tests fehlschlagen
- ✅ Erfolgreiche Implementierung

## Test-Fehler-Benachrichtigung

### Prozess (Workflow: `USER-ISSUE.03_Notify-Test-Failures_AUTO.yml`)

Wenn Unit-Tests bei einem User-Issue-PR fehlschlagen:

1. **Automatische Erkennung**
   - CI-Workflow schlägt fehl
   - Notification-Workflow wird getriggert
   - System findet das zugehörige Issue

2. **Benachrichtigung**
   - Benutzer erhält Kommentar auf dem Issue
   - Kommentar auf dem Pull Request
   - Labels werden hinzugefügt: `tests-failed`, `needs-manual-review`

3. **Inhalt der Benachrichtigung**
   - Welche Tests fehlgeschlagen sind
   - Link zu den Workflow-Logs
   - Hinweis auf manuelle Prüfung
   - Hinweis auf mögliche Wartezeit

4. **Manuelle Prüfung**
   - Maintainer wird benachrichtigt
   - Maintainer prüft und korrigiert
   - Tests werden erneut ausgeführt
   - Benutzer wird über Fortschritt informiert

## Workflow-Dateien

### USER-ISSUE.01_Process-Bug-Report_AUTO.yml
- **Trigger:** Issue mit Label `jules-auto-process` wird geöffnet
- **Funktion:** Erstellt sofort Jules Session für Bug Fix
- **Automation:** Vollautomatisch

### USER-ISSUE.02_Process-Feature-Request_AUTO.yml
- **Trigger:** 
  - Issue mit Label `needs-approval` wird geöffnet (→ Genehmigung anfordern)
  - Label `approved-for-jules` wird hinzugefügt (→ Jules starten)
- **Funktion:** Verwaltet Genehmigungsprozess und erstellt Jules Session
- **Automation:** Semi-automatisch (Genehmigung erforderlich)

### USER-ISSUE.03_Notify-Test-Failures_AUTO.yml
- **Trigger:** CI-Workflow schlägt fehl
- **Funktion:** Benachrichtigt Benutzer über fehlgeschlagene Tests
- **Automation:** Vollautomatisch

## Konfiguration

### Repository Variables
- `JULES_AUTOMATION_ENABLED` - Muss auf `true` gesetzt sein

### Repository Secrets
- `JulesAPIKey` - API-Schlüssel für Google Jules

## Labels

### Automatisch vergebene Labels

**Bug Reports:**
- `bug` - Kennzeichnet Bug Reports
- `jules-auto-process` - Triggert automatische Verarbeitung
- `jules-processing` - Jules arbeitet daran
- `tests-failed` - Tests sind fehlgeschlagen
- `needs-manual-review` - Manuelle Prüfung erforderlich

**Feature Requests:**
- `enhancement` - Kennzeichnet Feature Requests
- `needs-approval` - Warte auf Genehmigung
- `approved-for-jules` - Genehmigt für Jules (von Maintainer gesetzt)
- `jules-processing` - Jules arbeitet daran
- `tests-failed` - Tests sind fehlgeschlagen
- `needs-manual-review` - Manuelle Prüfung erforderlich

## Benutzererfahrung

### Für Bug Reports

1. **Benutzer meldet Bug** → Füllt Template aus
2. **Sofortige Bestätigung** → "Dein Bug wird automatisch bearbeitet"
3. **Jules arbeitet** → PR wird erstellt
4. **Erfolg** → "Bug wurde behoben" oder
5. **Tests fehlgeschlagen** → "Manuelle Prüfung erforderlich, bitte warten"

**Geschätzte Zeit:**
- Automatischer Erfolg: Minuten bis Stunden (abhängig von Jules)
- Mit manueller Prüfung: 1-3 Tage

### Für Feature Requests

1. **Benutzer schlägt Feature vor** → Füllt Template aus
2. **Warte auf Genehmigung** → "Wird geprüft"
3. **Genehmigt** → "Jules implementiert jetzt deine Funktion"
4. **Jules arbeitet** → PR wird erstellt
5. **Erfolg** → "Feature wurde implementiert" oder
6. **Tests fehlgeschlagen** → "Manuelle Prüfung erforderlich, bitte warten"

**Geschätzte Zeit:**
- Genehmigung: 1-7 Tage
- Nach Genehmigung: Minuten bis Stunden (abhängig von Jules)
- Mit manueller Prüfung: +1-3 Tage

## Vorteile

### Für Benutzer
- ✅ Einfacher Prozess über Issue-Templates
- ✅ Automatische Updates über den Fortschritt
- ✅ Transparenz über den Status
- ✅ Schnelle Bearbeitung von Bugs
- ✅ Klare Erwartungen bei Feature Requests

### Für Maintainer
- ✅ Kontrolle über Feature Requests
- ✅ Automatische Verarbeitung von Bugs
- ✅ Klare Benachrichtigungen bei Problemen
- ✅ Weniger manuelle Arbeit
- ✅ Strukturierter Prozess

## Fehlerbehandlung

### Was passiert wenn...

**...Jules die Aufgabe nicht verstehen kann?**
- Jules erstellt trotzdem einen PR
- Wahrscheinlich schlagen Tests fehl
- Benutzer und Maintainer werden benachrichtigt
- Manuelle Korrektur erforderlich

**...die Jules API nicht erreichbar ist?**
- Workflow schlägt fehl
- Issue bleibt offen
- Maintainer wird benachrichtigt
- Kann später manuell wiederholt werden

**...Tests fehlschlagen?**
- Benutzer wird automatisch benachrichtigt
- Labels werden gesetzt
- Maintainer wird erwähnt
- Manuelle Prüfung und Korrektur

**...ein Feature Request abgelehnt wird?**
- Issue wird geschlossen
- Benutzer erhält Erklärung im Kommentar
- Keine automatische Verarbeitung

## Best Practices

### Für Maintainer

**Bug Reports genehmigen:**
- Keine Aktion erforderlich - läuft automatisch
- Nur bei Problemen eingreifen

**Feature Requests genehmigen:**
1. Prüfe Relevanz und Machbarkeit
2. Bei Genehmigung: Label `approved-for-jules` hinzufügen
3. Bei Ablehnung: Issue schließen mit Begründung

**Bei fehlgeschlagenen Tests:**
1. Prüfe Workflow-Logs
2. Korrigiere Probleme im PR
3. Informiere Benutzer über den Status

### Für Benutzer

**Bug Reports erstellen:**
- Nutze das "Bug Report" Template
- Fülle alle Felder aus
- Füge Screenshots/Logs hinzu
- Warte auf automatische Verarbeitung

**Feature Requests erstellen:**
- Nutze das "Feature Request" Template
- Beschreibe Problem und Lösung detailliert
- Füge Use Cases hinzu
- Warte auf Genehmigung

## Monitoring

### Aktive Issues überwachen

```bash
# Alle offenen Bug Reports
gh issue list --label "bug,jules-auto-process"

# Alle Feature Requests, die auf Genehmigung warten
gh issue list --label "enhancement,needs-approval"

# Alle von Jules bearbeiteten Issues
gh issue list --label "jules-processing"

# Alle Issues mit fehlgeschlagenen Tests
gh issue list --label "tests-failed"
```

## Zukünftige Erweiterungen

Mögliche Verbesserungen:
- Dashboard für Issue-Status
- Automatische Prioritätserkennung
- Integration mit Projekt-Boards
- Erweiterte Metriken und Reporting
- A/B-Testing für Jules-Prompts
- Automatisches Re-Triggering bei transient failures
