# Anleitung: Lokale Entwicklungsumgebung mit Gemini CLI einrichten

Dieses Dokument beschreibt die Schritte zur Einrichtung der Gemini CLI auf einem lokalen Windows-PC, um eine konsistente Entwicklungsumgebung für das LightJockey-Projekt zu gewährleisten.

## 1. Voraussetzungen

-   **Windows 10/11:** Ein PC mit einer aktuellen Windows-Version.
-   **Git:** [Git](https://git-scm.com/download/win) muss installiert sein.
-   **PowerShell:** Die PowerShell wird für die Ausführung der Befehle benötigt.

## 2. Gemini CLI herunterladen

1.  **Download:**
    Laden Sie die neueste Version der `gemini.exe` von der offiziellen Quelle herunter. *(Hinweis: Ein Platzhalter-Link, da die tatsächliche Quelle nicht bekannt ist. Annahme: `https://.../gemini.exe`)*

2.  **Speicherort:**
    Erstellen Sie einen dedizierten Ordner für die CLI, z.B. `C:\Tools\Gemini`. Verschieben Sie die `gemini.exe` in diesen Ordner.

## 3. Umgebungsvariablen konfigurieren

Damit die `gemini.exe` von überall aus in der PowerShell aufgerufen werden kann, muss der Pfad zur ausführbaren Datei den System-Umgebungsvariablen hinzugefügt werden.

1.  **Systemeigenschaften öffnen:**
    Drücken Sie `Win + R`, geben Sie `sysdm.cpl` ein und drücken Sie Enter.

2.  **Umgebungsvariablen:**
    -   Wechseln Sie zum Tab "Erweitert" und klicken Sie auf "Umgebungsvariablen...".
    -   Wählen Sie in der Liste "Systemvariablen" die Variable `Path` aus und klicken Sie auf "Bearbeiten...".

3.  **Pfad hinzufügen:**
    -   Klicken Sie auf "Neu" und fügen Sie den Pfad zu Ihrem Gemini-Ordner hinzu (z.B. `C:\Tools\Gemini`).
    -   Bestätigen Sie alle offenen Fenster mit "OK".

4.  **PowerShell neu starten:**
    Schließen Sie alle offenen PowerShell-Fenster und öffnen Sie ein neues, damit die Änderungen wirksam werden.

5.  **Installation überprüfen:**
    Geben Sie in der neuen PowerShell den Befehl `gemini --version` ein. Wenn eine Versionsnummer angezeigt wird, war die Einrichtung erfolgreich.

## 4. Authentifizierung und Konfiguration

Um mit dem Projekt-Repository interagieren zu können, muss die Gemini CLI authentifiziert werden.

1.  **API-Schlüssel generieren:**
    -   Erstellen Sie einen neuen Fine-Grained Personal Access Token in Ihren GitHub-Einstellungen.
    -   Der Token benötigt mindestens die Berechtigungen `read:packages` und `write:packages`.
    -   Kopieren Sie den generierten Token sicher.

2.  **Konfiguration (Beispiel):**
    Die genaue Konfiguration hängt von der CLI ab. Ein typischer Befehl könnte so aussehen:

    ```powershell
    gemini config --token DEIN_GITHUB_TOKEN
    ```

    Folgen Sie der spezifischen Dokumentation der Gemini CLI für die genauen Befehle.

## 5. Projekt klonen und arbeiten

1.  **Repository klonen:**
    Klonen Sie das LightJockey-Repository an den gewünschten Ort:

    ```powershell
    git clone https://github.com/USER/lightjockey-S-V.git
    cd lightjockey-S-V
    ```

2.  **Mit Gemini CLI arbeiten:**
    Sie können nun die Gemini CLI verwenden, um Aufgaben im Projektkontext auszuführen, z.B. um einen Task zu starten:

    ```powershell
    gemini task start "Implementiere einen neuen Regenbogen-Effekt"
    ```

Mit diesen Schritten ist Ihre lokale Entwicklungsumgebung vollständig eingerichtet und bereit für die Arbeit am LightJockey-Projekt.
