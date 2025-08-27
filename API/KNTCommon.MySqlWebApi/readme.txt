
ASP.NET Core Web API service: KNTCommon.MySqlWebApi

service:

SC CREATE KNTCommon.MySqlWebApi binPath="C:\Programs\MySqlWebAPI\KNTCommon.MySqlWebApi.exe" start=auto

SC START KNTCommon.MySqlWebApi

Firewall: open Port 5025 in Inbound rules.

using: CMD (key = "mojskrivnikljuc"), ip address of service:

curl -H "X-API-Key: mojskrivnikljuc" http://<ip address>:5025/data/last-events
curl -H "X-API-Key: mojskrivnikljuc" http://<ip address>:5025/data/last-events?format=text
curl -H "X-API-Key: mojskrivnikljuc" http://<ip address>:5025/data/last-transaction
curl -H "X-API-Key: mojskrivnikljuc" http://<ip address>:5025/data/last-transaction?format=text

set key from localhost (set key "novMojKljuč123"):

curl -X POST http://localhost:5025/admin/set-key -d "{\"newKey\":\"novMojKljuč123\"}" -H "Content-Type: application/json"
