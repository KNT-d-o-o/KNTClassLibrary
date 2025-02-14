param (
    [string]$ServiceName
)

Stop-Service -Name $ServiceName
