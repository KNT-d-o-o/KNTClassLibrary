Import-Module WebAdministration

# Nastavi pot do aplikacije (znotraj IIS)
$appPath = "IIS:\Sites\SMM"

# Preveri, če aplikacija obstaja
if (Test-Path $appPath) {
    Write-Host "OK"
} else {
    Write-Host "NOTOK"
}