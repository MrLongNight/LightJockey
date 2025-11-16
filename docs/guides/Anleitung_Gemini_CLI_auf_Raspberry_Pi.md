# Anleitung: Gemini CLI als dedizierter Bot auf einem Raspberry Pi einrichten

Dieses Dokument führt durch die Einrichtung der Gemini CLI auf einem Raspberry Pi, um diesen als eigenständigen Bot für die Automatisierung von Aufgaben im LightJockey-Projekt zu nutzen. Dieser Bot agiert als eigenständige Einheit und ist **nicht** als Self-hosted Runner für GitHub Actions gedacht.

## 1. Voraussetzungen

-   **Raspberry Pi:** Modell 3B+ oder neuer wird empfohlen.
-   **Raspberry Pi OS:** Eine aktuelle Version des Betriebssystems (früher Raspbian), idealerweise die "Lite"-Version ohne Desktop-Umgebung.
-   **SSH-Zugang:** Der Raspberry Pi muss im Netzwerk erreichbar sein und der SSH-Zugang aktiviert sein.
-   **Git:** `sudo apt install git`

## 2. Gemini CLI herunterladen und installieren

1.  **Verzeichnis erstellen:**
    Erstellen Sie ein Verzeichnis für die Gemini CLI, z.B. im Home-Verzeichnis des Standardbenutzers (`pi`):
    ```bash
    mkdir ~/gemini-cli
    cd ~/gemini-cli
    ```

2.  **Download:**
    Laden Sie die ARM-kompatible Version der Gemini CLI herunter. *(Annahme: Es gibt eine ARM-Version unter einem spezifischen Link)*.
    ```bash
    wget https://.../gemini-arm
    ```

3.  **Ausführbar machen:**
    Geben Sie der heruntergeladenen Datei Ausführungsrechte:
    ```bash
    chmod +x gemini-arm
    ```

4.  **Symbolischen Link erstellen (optional, aber empfohlen):**
    Um `gemini` global aufrufen zu können, erstellen Sie einen Symlink in einem Verzeichnis, das im `PATH` liegt:
    ```bash
    sudo ln -s ~/gemini-cli/gemini-arm /usr/local/bin/gemini
    ```

5.  **Installation überprüfen:**
    Testen Sie die Installation mit dem Befehl:
    ```bash
    gemini --version
    ```
    Wenn eine Versionsnummer erscheint, war die Installation erfolgreich.

## 3. Bot-Account und Authentifizierung

Der Bot sollte nicht mit einem persönlichen GitHub-Account laufen. Erstellen Sie stattdessen einen dedizierten Bot-Account.

1.  **Neuen GitHub-Account erstellen:**
    -   Erstellen Sie einen neuen GitHub-Account, z.B. `LightJockeyBot`.
    -   Fügen Sie diesen Account als Kollaborator zum LightJockey-Repository hinzu und erteilen Sie ihm die notwendigen Berechtigungen (mindestens `Write`).

2.  **Fine-Grained Personal Access Token generieren:**
    -   Loggen Sie sich mit dem Bot-Account ein.
    -   Gehen Sie zu "Settings" -> "Developer settings" -> "Personal access tokens" -> "Fine-grained tokens".
    -   Erstellen Sie einen neuen Token mit den für das Repository benötigten Berechtigungen (z.B. `Read & Write` für `Issues`, `Pull requests` und `Contents`).
    -   **Kopieren Sie den Token sicher!** Er wird nur einmal angezeigt.

3.  **Gemini CLI auf dem Pi konfigurieren:**
    Konfigurieren Sie die CLI mit dem Token des Bot-Accounts:
    ```bash
    gemini config --token DEIN_BOT_GITHUB_TOKEN
    ```

## 4. Bot als Service einrichten (systemd)

Damit der Bot dauerhaft läuft und nach einem Neustart automatisch startet, richten wir einen `systemd`-Service ein.

1.  **Service-Datei erstellen:**
    Erstellen Sie eine neue Service-Datei:
    ```bash
    sudo nano /etc/systemd/system/gemini-bot.service
    ```

2.  **Inhalt der Service-Datei:**
    Fügen Sie folgenden Inhalt ein. Passen Sie `User` und `ExecStart` an Ihre Konfiguration an. Das `ExecStart`-Skript muss noch erstellt werden.

    ```ini
    [Unit]
    Description=Gemini CLI Bot for LightJockey
    After=network.target

    [Service]
    User=pi
    WorkingDirectory=/home/pi/lightjockey-S-V
    ExecStart=/home/pi/gemini-bot-script.sh
    Restart=always
    RestartSec=10

    [Install]
    WantedBy=multi-user.target
    ```

3.  **Bot-Skript erstellen:**
    Erstellen Sie das Skript, das vom Service aufgerufen wird. Dieses Skript enthält die Logik des Bots (z.B. das Überwachen von Issues).
    ```bash
    nano ~/gemini-bot-script.sh
    ```

    **Beispielinhalt für `gemini-bot-script.sh`:**
    ```bash
    #!/bin/bash
    # Stellt sicher, dass das Skript im richtigen Verzeichnis läuft
    cd /home/pi/lightjockey-S-V || exit

    # Klonen des Repos, falls nicht vorhanden
    if [ ! -d ".git" ]; then
      git clone https://github.com/USER/lightjockey-S-V.git .
    fi

    # Endlosschleife für den Bot-Betrieb
    while true
    do
      git pull # Repository aktuell halten
      echo "Prüfe auf neue Aufgaben..."
      # Beispiel: Gemini-Befehl zum Lauschen auf Issues mit bestimmtem Label
      gemini listen --repo USER/lightjockey-S-V --label "jules-ready" --action "task start"
      sleep 60 # Warte 60 Sekunden bis zur nächsten Prüfung
    done
    ```
    Machen Sie das Skript ausführbar: `chmod +x ~/gemini-bot-script.sh`

4.  **Service aktivieren und starten:**
    ```bash
    sudo systemctl daemon-reload
    sudo systemctl enable gemini-bot.service
    sudo systemctl start gemini-bot.service
    ```

5.  **Status überprüfen:**
    ```bash
    sudo systemctl status gemini-bot.service
    ```
    Der Status sollte "active (running)" anzeigen. Logs können mit `journalctl -u gemini-bot.service -f` eingesehen werden.
