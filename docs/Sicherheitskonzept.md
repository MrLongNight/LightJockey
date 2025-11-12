# Sicherheitskonzept — LightJockey Configuration Encryption

## Übersicht

LightJockey implementiert Verschlüsselung für sensible Konfigurationsdaten wie API-Schlüssel und Zugangsdaten. Dieses Dokument beschreibt das Sicherheitskonzept und die Implementierung.

## Verschlüsselungstechnologie

### DPAPI (Data Protection API)

LightJockey verwendet die Windows Data Protection API (DPAPI) für die Verschlüsselung sensibler Daten:

- **Technologie**: Windows DPAPI (`System.Security.Cryptography.ProtectedData`)
- **Scope**: `DataProtectionScope.CurrentUser`
- **Entropy**: Keine zusätzliche Entropy (nutzt Windows-User-Kontext)

### Vorteile von DPAPI

1. **Keine Schlüsselverwaltung erforderlich**: Windows verwaltet die Verschlüsselungsschlüssel automatisch
2. **Benutzergebunden**: Daten können nur vom gleichen Windows-Benutzer entschlüsselt werden
3. **Betriebssystem-integriert**: Nutzt Windows-Sicherheitsmechanismen
4. **Transparent**: Keine Notwendigkeit für separate Schlüsselspeicherung

### Einschränkungen

- **Plattform**: Nur Windows (passt zu LightJockey als WPF-Anwendung)
- **Benutzerkontext**: Verschlüsselte Daten können nicht zwischen Benutzern geteilt werden
- **Maschinen-Migration**: Bei Windows-Neuinstallation gehen verschlüsselte Daten verloren

## Architektur

### ConfigurationService

Der `ConfigurationService` ist verantwortlich für:

1. **Verschlüsselung**: Wandelt Klartext in verschlüsselte Bytes um
2. **Speicherung**: Persistiert verschlüsselte Daten als Base64-kodierte JSON-Datei
3. **Entschlüsselung**: Dekodiert gespeicherte Daten zurück zu Klartext
4. **Verwaltung**: CRUD-Operationen für verschlüsselte Konfigurationswerte

### Datenfluss

```
Klartext (z.B. AppKey)
    ↓
Encoding.UTF8.GetBytes()
    ↓
ProtectedData.Protect()
    ↓
Convert.ToBase64String()
    ↓
JSON-Serialisierung
    ↓
Datei-Speicherung (secure-config.dat)
```

### Speicherort

Verschlüsselte Konfigurationsdaten werden gespeichert unter:
```
%APPDATA%\LightJockey\secure-config.dat
```

## Verwendung

### Registrierung im DI-Container

```csharp
services.AddSingleton<IConfigurationService, ConfigurationService>();
```

### Speichern sensibler Daten

```csharp
await configService.SetSecureValueAsync("HueBridge_AppKey", appKey);
```

### Abrufen sensibler Daten

```csharp
var appKey = await configService.GetSecureValueAsync("HueBridge_AppKey");
```

### Löschen sensibler Daten

```csharp
await configService.RemoveValueAsync("HueBridge_AppKey");
```

## Geschützte Daten

Folgende sensible Daten sollten verschlüsselt gespeichert werden:

1. **Hue Bridge AppKey**: API-Schlüssel für Philips Hue Bridge-Verbindung
2. **Cloud-Credentials**: Zukünftige Cloud-Synchronisations-Zugangsdaten
3. **API-Tokens**: Externe Service-Tokens
4. **Passwörter**: Jegliche Benutzer-Credentials

## Sicherheitsmaßnahmen

### Implementierte Schutzmaßnahmen

1. **Verschlüsselung at Rest**: Alle sensiblen Daten werden verschlüsselt auf der Festplatte gespeichert
2. **Keine Klartext-Logs**: AppKeys und Passwörter werden nicht in Logs ausgegeben
3. **Thread-Safe**: Verwendung von `SemaphoreSlim` für Thread-sichere Operationen
4. **Fehlerbehandlung**: Graceful Degradation bei Verschlüsselungsfehlern

### Best Practices

1. **Minimal Memory Exposure**: Entschlüsselte Werte nur bei Bedarf im Speicher halten
2. **Keine Übertragung**: AppKeys niemals über unsichere Kanäle übertragen
3. **Regelmäßige Rotation**: Bei Sicherheitsvorfällen AppKeys neu generieren
4. **Backup-Strategie**: Benutzer sollten ihre Bridge-Zugangsdaten notieren

## Tests

### Unit Tests

Der `ConfigurationServiceTests` verifiziert:

- ✅ Verschlüsselung und Speicherung von Werten
- ✅ Korrekte Entschlüsselung gespeicherter Werte
- ✅ Persistenz über Service-Instanzen hinweg
- ✅ Behandlung von Sonderfällen (Unicode, Sonderzeichen)
- ✅ CRUD-Operationen (Create, Read, Update, Delete)
- ✅ Fehlerbehandlung bei ungültigen Eingaben

### Testabdeckung

- 18+ Unit Tests
- Alle öffentlichen API-Methoden getestet
- Edge-Cases abgedeckt (leere Strings, Null-Werte, etc.)

## Sicherheits-Audit Ergebnisse

### ✅ Stärken

1. Verwendung von etablierter Windows-Verschlüsselung (DPAPI)
2. Keine hardkodierten Schlüssel im Code
3. Thread-sichere Implementierung
4. Umfassende Fehlerbehandlung

### ⚠️ Einschränkungen

1. **Platform-Locked**: Nur Windows (akzeptabel für WPF-App)
2. **User-Locked**: Keine Datenportabilität zwischen Benutzern
3. **Recovery**: Keine Master-Password-Recovery-Funktion

### 🔄 Zukünftige Verbesserungen

1. **Backup-Export**: Verschlüsselter Export für Backup-Zwecke
2. **Key Rotation**: Automatische Rotation von AppKeys
3. **Cloud-Sync**: Sichere Cloud-Synchronisation verschlüsselter Configs

## Compliance

### DSGVO-Relevanz

- Persönliche Daten (AppKeys mit potenzieller Nutzer-Zuordnung) werden verschlüsselt
- Lokale Speicherung ohne Cloud-Übertragung (Privacy by Design)
- Nutzer hat volle Kontrolle über seine Daten

### Security Standards

- Entspricht Microsoft Security Best Practices für Desktop-Anwendungen
- Verwendung von OS-integrierten Sicherheitsmechanismen
- Defense in Depth: Verschlüsselung als zusätzliche Schutzebene

## Wartung und Support

### Monitoring

Logging erfolgt auf folgenden Ebenen:
- **Debug**: Verschlüsselungs-/Entschlüsselungs-Operationen
- **Information**: Erfolgreiche Speicherung/Laden von Daten
- **Error**: Verschlüsselungsfehler, Dateizugriffsfehler

### Troubleshooting

**Problem**: Daten können nicht entschlüsselt werden
- **Ursache**: Anderer Windows-Benutzer oder OS-Neuinstallation
- **Lösung**: Hue Bridge neu registrieren (Link-Button drücken)

**Problem**: `CryptographicException` beim Entschlüsseln
- **Ursache**: Korrupte Konfigurationsdatei
- **Lösung**: `secure-config.dat` löschen und neu konfigurieren

## Changelog

### Version 1.0 (Task 14)
- ✅ Initiale Implementierung von `ConfigurationService`
- ✅ DPAPI-basierte Verschlüsselung
- ✅ Unit Tests (18+ Tests)
- ✅ Dokumentation

---

**Autor**: LightJockey Team  
**Datum**: 2025-11-12  
**Version**: 1.0  
**Status**: Implementiert
