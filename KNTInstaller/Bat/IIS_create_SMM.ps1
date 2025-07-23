$siteName = "SMM"
$appPoolName = "SMM"
$physicalPath = "C:\Programs\SMM"
$bindingPort = 5015

# Ustvari mapo, če še ne obstaja
if (!(Test-Path $physicalPath)) {
    New-Item -Path $physicalPath -ItemType Directory
}

# Ustvari nov Application Pool
if (!(Test-Path "IIS:\AppPools\$appPoolName")) {
    New-WebAppPool -Name $appPoolName
}

# Ustvari nov Site
New-Website -Name $siteName -Port $bindingPort -PhysicalPath $physicalPath -ApplicationPool $appPoolName