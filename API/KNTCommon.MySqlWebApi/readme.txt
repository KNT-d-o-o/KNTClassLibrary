
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

---

routing povazava:

server network:

ipconfig
..
Wireless LAN adapter Wi-Fi:

   Connection-specific DNS Suffix  . :
   Link-local IPv6 Address . . . . . : fe80::d3ea:db1c:afeb:48f6%18
   IPv4 Address. . . . . . . . . . . : 10.10.10.99
   Subnet Mask . . . . . . . . . . . : 255.255.255.0
   Default Gateway . . . . . . . . . : 10.10.10.1

local network:
ipconfig
..
Ethernet adapter Ethernet 4:

   Connection-specific DNS Suffix  . : knt.local
   Link-local IPv6 Address . . . . . : fe80::a584:c62:95dd:975a%50
   IPv4 Address. . . . . . . . . . . : 192.168.88.140
   Subnet Mask . . . . . . . . . . . : 255.255.255.0
   Default Gateway . . . . . . . . . : 192.168.88.1

Gateway 192.168.88.1 in 10.10.10.1 morata biti med seboj povezana (npr. v routerju);

* Statične rute:

server:

route -p add 192.168.88.0 mask 255.255.255.0 10.10.10.1

local:

route -p add 10.10.10.0 mask 255.255.255.0 192.168.88.1

* Firewall dovoljenja na 10.10.10.99: TCP 5025, ICMPv4 (ping)

* Preverjanje poti:

local: tracert 10.10.10.99

server: tracert 192.168.88.140
