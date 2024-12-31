param (
    [string]$DateTime
)
$command = "Set-Date -Date '$DateTime'"
Start-Process powershell -ArgumentList "-NoProfile -Command $command" -Verb RunAs
