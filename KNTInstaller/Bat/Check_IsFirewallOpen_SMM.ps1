$rule = Get-NetFirewallRule -DisplayName "SMM" -ErrorAction SilentlyContinue

if ($rule) {
    $portFilters = $rule | Get-NetFirewallPortFilter

    foreach ($filter in $portFilters) {
        if ($filter.LocalPort -eq "5015") {
            Write-Output "OK"
            exit 0
        }
    }

    exit 1
} 