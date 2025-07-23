# Nastavitve
$port = 5015
$ruleName = "SMM"

# Ustvari novo pravilo v Windows Firewallu
New-NetFirewallRule `
    -DisplayName $ruleName `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort $port `
    -Action Allow `
    -Profile Any

Write-Host "Pravilo za odprtje porta $port je bilo ustvarjeno z imenom '$ruleName'."
