# Google Jules API - Referenz

## Übersicht

Diese Referenz dokumentiert die Google Jules API, wie sie im LightJockey AutoTask-Prozess verwendet wird.

## Basis-URL

```
https://jules.googleapis.com/v1alpha
```

⚠️ **Hinweis**: Die Jules API ist derzeit in der Alpha-Phase. Spezifikationen können sich ändern.

## Authentifizierung

Alle API-Anfragen erfordern einen API-Schlüssel, der im Header übergeben wird:

```
X-Goog-Api-Key: YOUR_API_KEY
```

### API-Schlüssel erhalten

1. Besuchen Sie [jules.google.com/settings#api](https://jules.google.com/settings#api)
2. Klicken Sie auf "Create API Key"
3. Kopieren Sie den generierten Schlüssel
4. Maximal 3 API-Schlüssel gleichzeitig möglich

⚠️ **Sicherheit**: API-Schlüssel sind vertraulich zu behandeln. Exponierte Schlüssel werden automatisch deaktiviert.

## API-Endpunkte

### 1. Sources (Quellen)

#### Sources auflisten

```bash
GET /v1alpha/sources?pageSize=10

curl 'https://jules.googleapis.com/v1alpha/sources' \
  -H 'X-Goog-Api-Key: YOUR_API_KEY'
```

**Response**:
```json
{
  "sources": [
    {
      "name": "sources/github/owner/repo",
      "id": "github/owner/repo",
      "githubRepo": {
        "owner": "owner",
        "repo": "repo"
      }
    }
  ],
  "nextPageToken": "..."
}
```

**Parameter**:
- `pageSize` (optional): Anzahl der Ergebnisse (Standard: 10)
- `pageToken` (optional): Token für Pagination

### 2. Sessions (Sitzungen)

#### Session erstellen

```bash
POST /v1alpha/sessions

curl 'https://jules.googleapis.com/v1alpha/sessions' \
  -X POST \
  -H "Content-Type: application/json" \
  -H 'X-Goog-Api-Key: YOUR_API_KEY' \
  -d '{
    "prompt": "Implement feature X",
    "sourceContext": {
      "source": "sources/github/owner/repo",
      "githubRepoContext": {
        "startingBranch": "main"
      }
    },
    "automationMode": "AUTO_CREATE_PR",
    "title": "Feature X Implementation"
  }'
```

**Request Body**:
```json
{
  "prompt": "string",              // Aufgabenbeschreibung
  "sourceContext": {
    "source": "string",            // Source name
    "githubRepoContext": {
      "startingBranch": "string"   // Git branch
    }
  },
  "automationMode": "string",      // AUTO_CREATE_PR, MANUAL, oder NONE
  "title": "string",               // Session-Titel
  "requirePlanApproval": boolean   // Optional: Plan-Genehmigung erforderlich
}
```

**Response**:
```json
{
  "name": "sessions/SESSION_ID",
  "id": "SESSION_ID",
  "title": "Feature X Implementation",
  "sourceContext": {
    "source": "sources/github/owner/repo",
    "githubRepoContext": {
      "startingBranch": "main"
    }
  },
  "prompt": "Implement feature X"
}
```

**Automation Modes**:
- `AUTO_CREATE_PR`: Jules erstellt automatisch einen PR
- `MANUAL`: Kein automatischer PR
- `NONE`: Keine Automatisierung

#### Session abrufen

```bash
GET /v1alpha/sessions/{sessionId}

curl 'https://jules.googleapis.com/v1alpha/sessions/SESSION_ID' \
  -H 'X-Goog-Api-Key: YOUR_API_KEY'
```

**Response**:
```json
{
  "name": "sessions/SESSION_ID",
  "id": "SESSION_ID",
  "title": "Feature X Implementation",
  "sourceContext": { ... },
  "prompt": "...",
  "outputs": [
    {
      "pullRequest": {
        "url": "https://github.com/owner/repo/pull/123",
        "title": "Implement feature X",
        "description": "..."
      }
    }
  ]
}
```

**Outputs**: 
- Enthält erstelle PRs wenn `automationMode: AUTO_CREATE_PR` verwendet wurde

#### Sessions auflisten

```bash
GET /v1alpha/sessions?pageSize=10

curl 'https://jules.googleapis.com/v1alpha/sessions?pageSize=5' \
  -H 'X-Goog-Api-Key: YOUR_API_KEY'
```

**Response**:
```json
{
  "sessions": [ ... ],
  "nextPageToken": "..."
}
```

#### Plan genehmigen

```bash
POST /v1alpha/sessions/{sessionId}:approvePlan

curl 'https://jules.googleapis.com/v1alpha/sessions/SESSION_ID:approvePlan' \
  -X POST \
  -H 'X-Goog-Api-Key: YOUR_API_KEY'
```

Nur erforderlich wenn `requirePlanApproval: true` gesetzt wurde.

#### Nachricht senden

```bash
POST /v1alpha/sessions/{sessionId}:sendMessage

curl 'https://jules.googleapis.com/v1alpha/sessions/SESSION_ID:sendMessage' \
  -X POST \
  -H "Content-Type: application/json" \
  -H 'X-Goog-Api-Key: YOUR_API_KEY' \
  -d '{
    "prompt": "Can you add more tests?"
  }'
```

**Request Body**:
```json
{
  "prompt": "string"  // Nachricht an Jules
}
```

Die Antwort erscheint in den Activities.

### 3. Activities (Aktivitäten)

#### Activities auflisten

```bash
GET /v1alpha/sessions/{sessionId}/activities?pageSize=30

curl 'https://jules.googleapis.com/v1alpha/sessions/SESSION_ID/activities' \
  -H 'X-Goog-Api-Key: YOUR_API_KEY'
```

**Response**:
```json
{
  "activities": [
    {
      "name": "sessions/SESSION_ID/activities/ACTIVITY_ID",
      "createTime": "2025-11-12T08:00:00Z",
      "originator": "agent",
      "planGenerated": {
        "plan": {
          "id": "PLAN_ID",
          "steps": [
            {
              "id": "STEP_ID",
              "title": "Setup environment",
              "index": 0
            }
          ]
        }
      },
      "id": "ACTIVITY_ID"
    }
  ]
}
```

**Activity Types**:
- `planGenerated`: Plan wurde erstellt
- `planApproved`: Plan wurde genehmigt
- `progressUpdated`: Fortschritts-Update
- `sessionCompleted`: Session abgeschlossen

**Originator**:
- `agent`: Von Jules
- `user`: Vom Benutzer

## Verwendung im LightJockey AutoTask-Prozess

### 1. Task-Submission (flow-jules_01-submit-task.yml)

Der Workflow verwendet die API um:

1. **Source ermitteln**:
```bash
GET /v1alpha/sources
# Findet: sources/github/MrLongNight/LightJockey
```

2. **Session erstellen**:
```bash
POST /v1alpha/sessions
{
  "prompt": "Implement Task X: Description...",
  "sourceContext": {
    "source": "sources/github/MrLongNight/LightJockey",
    "githubRepoContext": {
      "startingBranch": "main"
    }
  },
  "automationMode": "AUTO_CREATE_PR",
  "title": "Task X: Title"
}
```

3. **Tracking-Issue erstellen** mit Session-ID

### 2. Session-Monitoring (flow-jules_02-monitor-and-review.yml)

Der Workflow pollt regelmäßig:

1. **Session-Status prüfen**:
```bash
GET /v1alpha/sessions/{sessionId}
# Prüft: outputs[].pullRequest
```

2. **Bei PR-Erstellung**:
   - Tracking-Issue aktualisieren
   - Copilot-Review triggern
   - Labels setzen

### 3. Activities-Überwachung

Optional kann man Activities abrufen für detailliertes Monitoring:

```bash
GET /v1alpha/sessions/{sessionId}/activities
```

Zeigt:
- Plan-Generierung
- Fortschritte
- Code-Änderungen
- Completion-Status

## Rate Limits

Die aktuellen Rate Limits sind nicht öffentlich dokumentiert (Alpha-Phase).

**Best Practices**:
- Polling-Intervall: Mindestens 15 Minuten
- Exponential Backoff bei Fehlern
- Caching von Session-Daten

## Fehlerbehandlung

### HTTP-Statuscodes

- `200 OK`: Erfolgreiche Anfrage
- `400 Bad Request`: Ungültige Parameter
- `401 Unauthorized`: Fehlender/ungültiger API-Schlüssel
- `404 Not Found`: Resource nicht gefunden
- `429 Too Many Requests`: Rate Limit überschritten
- `500 Internal Server Error`: Server-Fehler

### Fehler-Response

```json
{
  "error": {
    "code": 404,
    "message": "Session not found",
    "status": "NOT_FOUND"
  }
}
```

### Retry-Strategie

```bash
# Exponential Backoff
RETRY_DELAYS=(1 2 4 8 16)
for delay in "${RETRY_DELAYS[@]}"; do
  if curl ...; then
    break
  fi
  sleep $delay
done
```

## CLI-Tool: jules_api_helper.py

Das Projekt enthält ein Python-Tool für einfachere API-Nutzung:

```bash
# Sources auflisten
./scripts/jules_api_helper.py list-sources

# Session erstellen
./scripts/jules_api_helper.py create-session \
  --source "sources/github/MrLongNight/LightJockey" \
  --title "Task 13" \
  --prompt "Implement cloud backup"

# Session überwachen
./scripts/jules_api_helper.py monitor SESSION_ID
```

Siehe: [scripts/README.md](../../scripts/README.md)

## Beispiele

### Kompletter Workflow

```bash
# 1. API Key setzen
export JULES_API_KEY="your-key-here"

# 2. Source finden
SOURCE=$(curl -s 'https://jules.googleapis.com/v1alpha/sources' \
  -H "X-Goog-Api-Key: $JULES_API_KEY" | \
  jq -r '.sources[] | select(.githubRepo.repo=="LightJockey") | .name')

# 3. Session erstellen
SESSION=$(curl -s 'https://jules.googleapis.com/v1alpha/sessions' \
  -X POST \
  -H "Content-Type: application/json" \
  -H "X-Goog-Api-Key: $JULES_API_KEY" \
  -d '{
    "prompt": "Implement feature X",
    "sourceContext": {
      "source": "'$SOURCE'",
      "githubRepoContext": {
        "startingBranch": "main"
      }
    },
    "automationMode": "AUTO_CREATE_PR",
    "title": "Feature X"
  }' | jq -r '.id')

echo "Session ID: $SESSION"

# 4. Session überwachen
while true; do
  STATUS=$(curl -s "https://jules.googleapis.com/v1alpha/sessions/$SESSION" \
    -H "X-Goog-Api-Key: $JULES_API_KEY")
  
  PR_URL=$(echo "$STATUS" | jq -r '.outputs[]?.pullRequest?.url // empty')
  
  if [ -n "$PR_URL" ]; then
    echo "PR created: $PR_URL"
    break
  fi
  
  sleep 60
done
```

## Sicherheit

### API-Schlüssel-Verwaltung

✅ **Richtig**:
```yaml
# GitHub Actions Workflow
env:
  JULES_API_KEY: ${{ secrets.JulesAPIKey }}
```

❌ **Falsch**:
```yaml
# Niemals hardcoden!
env:
  JULES_API_KEY: "sk-abc123..."
```

### Best Practices

1. **Secrets verwenden**: API-Keys immer in GitHub Secrets speichern
2. **HTTPS only**: Nur verschlüsselte Verbindungen
3. **Key-Rotation**: Regelmäßig API-Keys erneuern (alle 90 Tage)
4. **Monitoring**: API-Nutzung überwachen
5. **Error Handling**: Fehler abfangen und loggen

## Weiterführende Links

- **Offizielle API-Dokumentation**: [developers.google.com/jules/api](https://developers.google.com/jules/api)
- **Jules Web App**: [jules.google.com](https://jules.google.com)
- **API Settings**: [jules.google.com/settings#api](https://jules.google.com/settings#api)

## Support

Bei Problemen mit der Jules API:
- Jules Support: support@jules.google.com
- API-Referenz: https://developers.google.com/jules/api/reference/rest
- GitHub Issues: Für LightJockey-spezifische Probleme

---

**Erstellt**: 2025-11-12  
**Version**: 1.0 (Alpha)  
**Status**: Produktiv im Einsatz  
**Letzte Aktualisierung**: 2025-11-12
