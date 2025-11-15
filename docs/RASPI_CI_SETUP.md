# Schritt-für-Schritt-Anleitung: CI-Pipeline mit Raspberry Pi für Hue Bridge Integrationstests

Dieses Dokument beschreibt die Einrichtung einer dauerhaften Continuous Integration (CI) Pipeline. Ziel ist es, die `HueService`-Integrationstests automatisch auf einem Raspberry Pi auszuführen, der mit einer echten Philips Hue Bridge im selben Netzwerk verbunden ist.

## Übersicht der Komponenten

*   **Raspberry Pi:** Ein kleiner Computer, der als dedizierter Testserver dient.
*   **GitHub Self-Hosted Runner:** Eine Software, die auf dem Pi läuft, ihn mit GitHub Actions verbindet und ihm erlaubt, CI/CD-Jobs auszuführen.
*   **GitHub Actions:** Die Automatisierungsplattform von GitHub, die den Workflow definiert (Code auschecken, bauen, testen).
*   **GitHub Secrets:** Eine sichere Möglichkeit, sensible Daten wie die IP-Adresse der Hue Bridge und den App-Key zu speichern.
*   **GitHub CLI (`gh`):** Ein Kommandozeilen-Tool zur Interaktion mit GitHub, nützlich für die Überwachung und Verwaltung von Workflows.

---

## Schritt 1: Vorbereitung des Raspberry Pi

1.  **Betriebssystem installieren:**
    *   Installieren Sie die neueste Version von **Raspberry Pi OS (64-bit)** auf einer SD-Karte. Dies ist wichtig, da das .NET SDK eine 64-bit-Architektur erfordert.
    *   Verbinden Sie den Pi mit Ihrem Netzwerk (WLAN oder Ethernet) und stellen Sie sicher, dass er eine stabile Verbindung hat und auf die Hue Bridge zugreifen kann.

2.  **System aktualisieren:**
    *   Öffnen Sie ein Terminal auf dem Pi (oder verbinden Sie sich per SSH) und führen Sie folgende Befehle aus:
        ```bash
        sudo apt-get update
        sudo apt-get upgrade -y
        ```

3.  **.NET SDK installieren:**
    *   LightJockey verwendet .NET 9. Installieren Sie das .NET 9 SDK auf dem Raspberry Pi.
    *   Folgen Sie der offiziellen Anleitung von Microsoft für ARM64-Systeme oder verwenden Sie das Installationsskript:
        ```bash
        curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
        ```
    *   Fügen Sie die .NET-Tools zum `PATH` hinzu. Öffnen Sie `~/.bashrc` (oder `~/.zshrc`) und fügen Sie folgende Zeilen am Ende hinzu:
        ```bash
        export DOTNET_ROOT=$HOME/.dotnet
        export PATH=$PATH:$HOME/.dotnet
        ```
    *   Laden Sie die Konfiguration neu mit `source ~/.bashrc` und überprüfen Sie die Installation mit `dotnet --version`.

---

## Schritt 2: Einrichtung des GitHub Self-Hosted Runners

1.  **Runner-Software herunterladen:**
    *   Navigieren Sie in Ihrem GitHub-Repository zu `Settings > Actions > Runners`.
    *   Klicken Sie auf `New self-hosted runner`.
    *   Wählen Sie als Betriebssystem `Linux` und als Architektur `ARM64`.
    *   Folgen Sie den angezeigten Befehlen, um den Runner herunterzuladen, zu entpacken und zu konfigurieren.

2.  **Runner als Dienst einrichten:**
    *   Nach der Konfiguration müssen Sie den Runner als permanenten Dienst installieren, damit er auch nach einem Neustart des Pi automatisch startet.
    *   Führen Sie im Runner-Verzeichnis folgenden Befehl aus:
        ```bash
        sudo ./svc.sh install
        sudo ./svc.sh start
        ```
    *   Überprüfen Sie den Status mit `sudo ./svc.sh status`. Der Runner sollte nun als "active" angezeigt werden und in Ihrem GitHub-Repository als "Idle" erscheinen.

---

## Schritt 3: Konfiguration der Tests für die Hue Bridge

Die Tests müssen auf die IP-Adresse und den App-Key der Bridge zugreifen können, ohne diese sensiblen Daten direkt in den Code zu schreiben.

1.  **GitHub Secrets erstellen:**
    *   Gehen Sie in Ihrem GitHub-Repository zu `Settings > Secrets and variables > Actions`.
    *   Erstellen Sie zwei neue "Repository secrets":
        *   `HUE_BRIDGE_IP`: Die lokale IP-Adresse Ihrer Hue Bridge (z.B., `192.168.1.50`).
        *   `HUE_APP_KEY`: Ein gültiger App-Key, den Sie durch Drücken des Link-Buttons auf der Bridge und Ausführen eines Registrierungsskripts oder der Anwendung selbst erhalten.

2.  **Integrationstest-Datei erstellen:**
    *   Erstellen Sie eine neue Testdatei unter `tests/LightJockey.Tests/Services/HueServiceIntegrationTests.cs`. Dieser Test wird nur in der CI-Umgebung ausgeführt.
    *   **Inhalt der `HueServiceIntegrationTests.cs`:**
        ```csharp
        using LightJockey.Models;
        using LightJockey.Services;
        using Microsoft.Extensions.Logging.Abstractions;
        using System;
        using System.Threading.Tasks;
        using Xunit;

        namespace LightJockey.Tests.Services
        {
            public class HueServiceIntegrationTests
            {
                private readonly string? _bridgeIp;
                private readonly string? _appKey;

                public HueServiceIntegrationTests()
                {
                    // Liest die Secrets aus den Umgebungsvariablen, die von GitHub Actions gesetzt werden.
                    _bridgeIp = Environment.GetEnvironmentVariable("HUE_BRIDGE_IP");
                    _appKey = Environment.GetEnvironmentVariable("HUE_APP_KEY");
                }

                [Fact]
                public async Task ConnectAndGetLights_WithRealBridge_Succeeds()
                {
                    // Diesen Test nur ausführen, wenn die Secrets in der CI-Umgebung vorhanden sind.
                    if (string.IsNullOrEmpty(_bridgeIp) || string.IsNullOrEmpty(_appKey))
                    {
                        // In Xunit gibt es kein Assert.Skip, daher verlassen wir den Test einfach.
                        // Alternativ kann man eine separate Testkonfiguration verwenden.
                        return;
                    }

                    // Arrange
                    var logger = new NullLogger<HueService>();
                    var configServiceMock = new Moq.Mock<IConfigurationService>();
                    var service = new HueService(logger, configServiceMock.Object);
                    var bridge = new HueBridge { IpAddress = _bridgeIp };

                    // Act
                    var connected = await service.ConnectAsync(bridge, _appKey);
                    var lights = await service.GetLightsAsync();

                    // Assert
                    Assert.True(connected, "Verbindung zur Hue Bridge fehlgeschlagen.");
                    Assert.NotNull(lights);
                    Assert.NotEmpty(lights); // Es sollte mindestens ein Licht gefunden werden.
                }
            }
        }
        ```

---

## Schritt 4: Erstellung des GitHub Actions Workflows

Erstellen Sie eine neue Workflow-Datei unter `.github/workflows/hue-integration-tests.yml`.

*   **Inhalt der `hue-integration-tests.yml`:**
    ```yaml
    name: Hue Bridge Integration Tests

    on:
      push:
        branches: [ main, develop ]
      pull_request:

    jobs:
      build-and-test-on-pi:
        name: Run Integration Tests on Raspberry Pi
        runs-on: self-hosted # Wählt automatisch den Runner auf dem Pi aus

        steps:
        - name: Checkout code
          uses: actions/checkout@v4

        - name: Setup .NET
          uses: actions/setup-dotnet@v4
          with:
            dotnet-version: '9.0.x'

        - name: Restore dependencies
          run: dotnet restore

        - name: Build project
          run: dotnet build --no-restore --configuration Release

        - name: Run Hue Integration Tests
          env:
            HUE_BRIDGE_IP: ${{ secrets.HUE_BRIDGE_IP }}
            HUE_APP_KEY: ${{ secrets.HUE_APP_KEY }}
          run: dotnet test --no-build --configuration Release --filter "FullyQualifiedName~HueServiceIntegrationTests"
    ```
    **Wichtige Hinweise:**
    *   `runs-on: self-hosted`: Dieser Schlüssel stellt sicher, dass der Job nur auf Ihrem Raspberry Pi läuft.
    *   `env:`: Hier werden die GitHub Secrets sicher an den Testprozess als Umgebungsvariablen übergeben.
    *   `--filter`: Dieser Parameter sorgt dafür, dass `dotnet test` nur die neuen Integrationstests ausführt und die schnellen Unit-Tests überspringt.

---

## Schritt 5: Verwaltung und Überwachung per CLI

Anstelle einer "Gemini CLI" verwenden wir die offizielle und leistungsstarke **GitHub CLI (`gh`)**.

1.  **Installation:**
    *   Installieren Sie die `gh` CLI auf Ihrem lokalen Entwicklungsrechner. Anleitungen finden Sie auf [cli.github.com](https://cli.github.com/).
    *   Authentifizieren Sie sich mit `gh auth login`.

2.  **Nützliche Befehle:**
    *   **Workflows überwachen:** Sehen Sie den Status der letzten Workflow-Läufe.
        ```bash
        gh run list --workflow="hue-integration-tests.yml"
        ```
    *   **Details eines Laufs anzeigen:**
        ```bash
        gh run view <RUN_ID>
        ```
    *   **Logs eines Jobs live verfolgen:**
        ```bash
        gh run watch <RUN_ID>
        ```
    *   **Einen Workflow manuell auslösen (erfordert `workflow_dispatch` im YAML):**
        ```bash
        gh workflow run hue-integration-tests.yml --ref main
        ```

Mit dieser Einrichtung haben Sie eine vollautomatische, professionelle CI-Pipeline, die echte Hardware-Tests durchführt und bequem von der Kommandozeile aus verwaltet werden kann.
