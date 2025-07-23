$shell = New-Object -ComObject WScript.Shell
Get-ChildItem "C:\Programs\Edge" -Filter *.lnk | ForEach-Object {
    $s = $shell.CreateShortcut($_.FullName)
    $s.Arguments = "-ExecutionPolicy Unrestricted $($s.Arguments)"
    $s.Save()
}

Write-Host "OK"