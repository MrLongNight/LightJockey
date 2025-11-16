# GitHub Actions Workflows

Dieses Verzeichnis enthält die automatisierten CI/CD-Workflows für das LightJockey-Projekt. Die Workflows sind so konzipiert, dass sie einen hohen Automatisierungsgrad ermöglichen, von der Bearbeitung von Bug-Reports bis hin zur automatischen Korrektur und dem Mergen von Pull Requests.

## Kern-Workflows

Diese Workflows bilden die Grundlage der CI/CD-Pipeline und des automatisierten Task-Managements.

### `CI.01_Build-Test_on-Push-PR_AUTO.yml`
-   **Name:** `CI.01 | Build & Test | AUTO`
-   **Zweck:** Führt bei jedem Push und Pull Request automatisch einen Build der Anwendung durch und führt alle Unit-Tests aus. Dies stellt die Code-Qualität und -Stabilität sicher.
-   **Trigger:** Push auf `main`/`develop`, Pull Request auf `main`/`develop`.

### `RELEASE.01_Create-MSI_on-Push_AUTO.yml`
-   **Name:** `RELEASE.01 | Create MSI Installer | AUTO`
-   **Zweck:** Erstellt ein MSI-Installationspaket für die Verteilung der Anwendung.
-   **Trigger:** Manuell über die GitHub Actions UI oder automatisch bei einem Push eines Version-Tags (z.B. `v1.2.3`).

## Automatisierte Issue- und PR-Prozesse

Diese Workflows automatisieren die Reaktion auf von Benutzern erstellte Issues und die Verwaltung von Pull Requests, die vom Entwicklungs-Bot "Jules" erstellt werden.

### `BUG.01_Process-Bug-Report_AUTO.yml`
-   **Name:** `BUG.01 | Process Bug Report | AUTO`
-   **Zweck:** Verarbeitet automatisch neu erstellte Bug-Reports.
-   **Trigger:** Hinzufügen des Labels `Bug Report` zu einem Issue.
-   **Logik:**
    1.  Die Gemini CLI analysiert den Inhalt des Issues.
    2.  **Wenn die Informationen ausreichen:** Das Label `jules-ready` wird hinzugefügt, um den Task für die automatische Bearbeitung durch Jules freizugeben.
    3.  **Wenn die Informationen unzureichend sind:** Der Workflow postet einen Kommentar und bittet den Ersteller um weitere Details.

### `PR.01_Self-Heal-on-Test-Failure_AUTO.yml`
-   **Name:** `PR.01 | Self-Heal on Test Failure | AUTO`
-   **Zweck:** Versucht, fehlgeschlagene Tests in einem von Jules erstellten Pull Request automatisch zu korrigieren.
-   **Trigger:** Ein Fehlschlag des `CI.01`-Workflows.
-   **Logik:**
    1.  Die Fehler-Logs des fehlgeschlagenen CI-Laufs werden heruntergeladen.
    2.  Die Gemini CLI analysiert die Logs und versucht, einen Code-Patch zu generieren.
    3.  Wenn erfolgreich, wird der Patch committet und in den PR-Branch gepusht, was den CI-Workflow erneut auslöst.
    4.  Der PR wird mit einem Kommentar über den Reparaturversuch versehen.

### `PR.02_Auto-Merge-and-Notify_AUTO.yml`
-   **Name:** `PR.02 | Auto-Merge and Notify | AUTO`
-   **Zweck:** Mergt einen erfolgreichen Pull Request von Jules automatisch und schließt das zugehörige Issue.
-   **Trigger:** Ein erfolgreicher Abschluss des `CI.01`-Workflows.
-   **Logik:**
    1.  Der Pull Request wird automatisch gemerged (Squash-Merge).
    2.  Das ursprüngliche Issue, das durch den PR gelöst wurde (z.B. via "Closes #123" im PR-Body), wird gefunden.
    3.  Ein Kommentar über den erfolgreichen Abschluss wird im Issue gepostet.
    4.  Das Issue wird automatisch geschlossen.

## Archivierte Workflows

Ältere oder nicht mehr verwendete Workflows befinden sich im Unterverzeichnis `workflow-archiv` und sind nicht mehr aktiv.

## Zusammenfassung der aktuellen Workflows

| Workflow                                       | Trigger                                           | Zweck                                                                  |
| ---------------------------------------------- | ------------------------------------------------- | ---------------------------------------------------------------------- |
| `CI.01_Build-Test_on-Push-PR_AUTO.yml`         | Push / PR                                         | Code bauen und Unit-Tests ausführen.                                   |
| `RELEASE.01_Create-MSI_on-Push_AUTO.yml`       | Manuell / Tag-Push                                | MSI-Installationspaket erstellen.                                      |
| `BUG.01_Process-Bug-Report_AUTO.yml`           | Issue-Label `Bug Report`                          | Eingehende Bug-Reports analysieren und triagieren.                     |
| `PR.01_Self-Heal-on-Test-Failure_AUTO.yml`     | Fehlschlag des `CI.01`-Workflows                  | Automatische Reparatur von fehlgeschlagenen Tests in einem PR.         |
| `PR.02_Auto-Merge-and-Notify_AUTO.yml`         | Erfolg des `CI.01`-Workflows                      | Automatischen Merge eines PRs und Schließen des verknüpften Issues.    |
