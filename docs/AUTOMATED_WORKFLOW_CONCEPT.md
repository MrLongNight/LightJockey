# Konzept: Autonomer, selbstheilender Pull-Request-Workflow

## 1. Zielsetzung

Das primäre Ziel dieses Konzepts ist die vollständige Automatisierung des Entwicklungsprozesses für Pull Requests (PRs), die von KI-Agenten (wie "Jules") erstellt werden. Wenn die in einem solchen PR enthaltenen Code-Änderungen zu fehlgeschlagenen CI-Tests (Continuous Integration) führen, soll das System in der Lage sein, diesen Fehler autonom zu erkennen, eine Korrektur zu erarbeiten und diese in den bestehenden PR zu integrieren, ohne dass ein menschlicher Eingriff erforderlich ist.

## 2. Architektur & Komponenten

Die Umsetzung basiert auf dem `gemini-cli` Toolstack, der innerhalb von GitHub Actions ausgeführt wird. Die einzelnen Komponenten sind:

- **GitHub Actions:** Dient als Laufzeitumgebung für den gesamten Prozess.
- **Gemini CLI:** Das zentrale Kommandozeilen-Tool zur Interaktion mit den Gemini-Sprachmodellen.
- **`@clduab11/gemini-flow` (Orchestrator):** Diese Extension agiert als "Dirigent" des Prozesses. Sie definiert und steuert den Ablauf, von der Fehlererkennung bis zur Anwendung des Fixes.
- **`GitHub MCP Server` (Analyse-Agent):** Eine spezialisierte Extension, die über die GitHub-API Informationen aus dem Repository liest (z.B. Fehler-Logs, Code-Diffs, PR-Details).
- **`Jules MCP Server` (Korrektur-Agent):** Ein konzeptioneller, spezialisierter Agent, der die Fehleranalyse-Ergebnisse erhält und die Aufgabe hat, konkreten Code zur Behebung des Problems zu generieren.
- **`Git MCP Server` (Ausführungs-Agent):** Eine Extension, die Git-Operationen durchführt, wie das Anwenden eines Patches und das Pushen der Änderungen in den Branch des PRs.

## 3. Visualisierung des Prozesses

```mermaid
graph TD
    A[Start: PR von Jules erstellt] --> B{CI-Workflow startet};
    B --> C{Tests erfolgreich?};
    C -- Ja --> D[PR wird zum Merge freigegeben];
    C -- Nein --> E[Fehler-Workflow wird getriggert];

    subgraph "Autonomer Korrektur-Prozess (gemini-flow)"
        E --> F[1. Analyse (GitHub MCP)];
        F -- Fehlerlogs & Code --> G[2. Korrektur (Jules MCP)];
        G -- Code-Patch --> H[3. Anwendung (Git MCP)];
        H -- git push --> I[CI-Workflow wird erneut getriggert];
    end

    I --> J{Tests erfolgreich?};
    J -- Ja --> D;
    J -- Nein --> K{Versuch > 3?};
    K -- Nein --> F;
    K -- Ja --> L[Eskalation: Manuelle Prüfung erforderlich];
```

## 4. Detaillierter Ablauf

1.  **Trigger:** Ein von "Jules" erstellter PR löst den `CI.01_Build-Test_on-Push-PR_AUTO.yml` Workflow aus. Dieser schlägt fehl.
2.  **Initiierung:** Der `USER-ISSUE.03_Notify-Test-Failures_AUTO.yml` Workflow wird durch den Fehlschlag aktiviert.
3.  **Übergabe an den Orchestrator:** Anstatt eine Benachrichtigung zu senden, ruft dieser Workflow den neuen, auf `gemini-flow` basierenden Korrektur-Workflow auf und übergibt die `workflow_run_id` des fehlgeschlagenen Laufs.
4.  **Schritt 1: Analyse (GitHub MCP):**
    -   Der `gemini-flow` startet.
    -   Der `GitHub MCP Server` wird aktiviert.
    -   Er nutzt die `workflow_run_id`, um die genauen Test-Logs herunterzuladen.
    -   Er identifiziert den zugehörigen PR und den Branch-Namen.
    -   Er extrahiert das Code-Diff des PRs.
5.  **Schritt 2: Korrektur (Jules MCP):**
    -   `gemini-flow` übergibt die gesammelten Daten (Logs, Diff, Code) an den `Jules MCP Server`.
    -   Der Prompt für diesen Agenten ist präzise und aufgabenorientiert, z.B.:
        > "Die Unit-Tests im Branch `[branch-name]` sind fehlgeschlagen. Die Fehlerlogs sind im Anhang. Analysiere den bereitgestellten Code und die Fehler. Erstelle einen Code-Patch, der ausschließlich die notwendigen Änderungen zur Behebung dieser Fehler enthält."
6.  **Schritt 3: Anwendung (Git MCP):**
    -   Der vom `Jules MCP` generierte Patch wird vom `gemini-flow` empfangen.
    -   Der `Git MCP Server` wird aktiviert.
    -   Er checkt den Branch des PRs aus.
    -   Er wendet den Patch auf den Code an.
    -   Er committet und pusht die Änderungen in den Branch.
7.  **Schritt 4: Wiederholung & Eskalation:**
    -   Der Push löst den CI-Workflow erneut aus.
    -   **Bei Erfolg:** Der Prozess endet, der PR ist "grün" und kann gemerged werden.
    -   **Bei erneutem Fehler:** `gemini-flow` prüft einen Zähler. Wenn weniger als 3 Versuche unternommen wurden, wird der Prozess (Analyse, Korrektur, Anwendung) wiederholt.
    -   **Bei 3+ Fehlern:** Der Prozess wird abgebrochen. Es wird eine Benachrichtigung zur manuellen Prüfung an den Repository-Owner gesendet, und der PR wird mit einem entsprechenden Label (`needs-manual-review`) versehen.

## 5. Implementierungsschritte

1.  **Vorbereitung:** Ein neuer, wiederverwendbarer GitHub Actions Workflow wird erstellt, der `gemini-cli` und die notwendigen Extensions installiert und konfiguriert.
2.  **Konfiguration:** Eine `gemini-flow`-Konfigurationsdatei (`.geminiflow/workflow.yml`) wird im Repository angelegt, um den oben beschriebenen Ablauf zu definieren.
3.  **Integration:** Der bestehende `USER-ISSUE.03_Notify-Test-Failures_AUTO.yml` Workflow wird angepasst, um den neuen `gemini-flow`-Workflow aufzurufen.
4.  **Testing & Validierung:** Der gesamte Prozess wird mit einem absichtlich fehlerhaften PR getestet, um die Funktionalität der Selbstheilung zu validieren.
