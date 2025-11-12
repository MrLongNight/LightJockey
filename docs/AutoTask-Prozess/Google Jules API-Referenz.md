Jules API

Lesezeichenrahmen
Erstellen und greifen Sie programmatisch auf Ihre asynchronen Codierungsaufgaben zu.

Dienst : jules.googleapis.com​​
Für die Nutzung dieses Dienstes empfehlen wir die Verwendung der von Google bereitgestellten Clientbibliotheken . Falls Ihre Anwendung eigene Bibliotheken benötigt, um diesen Dienst aufzurufen, verwenden Sie bitte die folgenden Informationen bei Ihren API-Anfragen.

Dienstendpunkt
Ein Service-Endpunkt ist eine Basis-URL, die die Netzwerkadresse eines API-Dienstes angibt. Ein Dienst kann mehrere Service-Endpunkte haben. Dieser Dienst hat den folgenden Service-Endpunkt, und alle unten stehenden URIs sind relativ zu diesem Service-Endpunkt:

https://jules.googleapis.com
REST-Ressource: v1alpha . Sitzungen
Methoden
approvePlan
POST /v1alpha/{session=sessions/*}:approvePlan
Genehmigt einen Plan in einer Sitzung.
create
POST /v1alpha/sessions
Erstellt eine neue Sitzung.
get
GET /v1alpha/{name=sessions/*}
Erhält eine einzelne Sitzung.
list
GET /v1alpha/sessions
Listet alle Sitzungen auf.
sendMessage
POST /v1alpha/{session=sessions/*}:sendMessage
Sendet eine Nachricht vom Benutzer an eine Sitzung.
REST-Ressource: v1alpha.sessions.activities
Methoden
get
GET /v1alpha/{name=sessions/*/activities/*}
Ruft eine einzelne Aktivität ab.
list
GET /v1alpha/{parent=sessions/*}/activities
Listet die Aktivitäten einer Sitzung auf.
REST-Ressource: v1alpha.sources
Methoden
get
GET /v1alpha/{name=sources/**}
Ruft eine einzelne Datenquelle ab.
list
GET /v1alpha/sources
Quellenangaben
