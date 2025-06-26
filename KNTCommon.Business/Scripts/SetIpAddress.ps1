param (
    [string]$InterfaceAlias,
    [string]$IpAddress,
    [string]$PrefixLength,
    [string]$Gateway = $null,
    [string]$RemoveExisting = "true"
)

$remove = $false
if ($RemoveExisting -eq "true" -or $RemoveExisting -eq "1") {
    $remove = $true
}

try {
    $ErrorActionPreference = 'Stop'

    if ($remove) {
        Write-Output "Removing existing IPv4 addresses from $InterfaceAlias..."
        Get-NetIPAddress -InterfaceAlias $InterfaceAlias -AddressFamily IPv4 -ErrorAction SilentlyContinue |
            Remove-NetIPAddress -Confirm:$false -ErrorAction SilentlyContinue
    }

    $existing = Get-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress $IpAddress -ErrorAction SilentlyContinue

    if ($null -eq $existing) {
        if ($Gateway) {
            New-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress $IpAddress -PrefixLength $PrefixLength -DefaultGateway $Gateway
            Write-Output "New IP set with gateway: $IpAddress / $PrefixLength → $Gateway"
        } else {
            New-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress $IpAddress -PrefixLength $PrefixLength
            Write-Output "New IP set without gateway: $IpAddress / $PrefixLength"
        }
    } else {
        Write-Output "IP address already configured: $IpAddress"
    }
}
catch {
    Write-Error "IP configuration failed: $_"
}
