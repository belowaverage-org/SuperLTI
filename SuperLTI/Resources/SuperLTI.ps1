New-EventLog -LogName "Application" -Source "SuperLTI"
Start-Transcript -Path "C:\SuperLTI\SuperLTI.log"
$cppslt = "Preparing..."
Write-Progress -Activity $cppslt -Status "Saving ExecutionPolicy..." -PercentComplete 0
$cppsxp = Get-ExecutionPolicy
Write-Progress -Activity $cppslt -Status "Setting ExecutionPolicy..." -PercentComplete 3
Set-ExecutionPolicy -ExecutionPolicy Bypass -Force
Write-Progress -Activity $cppslt -Status "Running SuperLTI Script..." -PercentComplete 50
Set-Location -Path "C:\SuperLTI\"
&"C:\SuperLTI\SuperLTI.ps1"
Set-Location -Path "C:\"
Write-Progress -Activity $cppslt -Status "Restoring ExecutionPolicy..." -PercentComplete 90
Set-ExecutionPolicy -ExecutionPolicy $cppsxp -Force
Write-Progress -Activity $cppslt -Status "Writing Event Log..." -PercentComplete 95
Stop-Transcript
Write-EventLog -EventId 0 -LogName "Application" -Message (Get-Content -Path "C:\SuperLTI\SuperLTI.log" | Out-String) -Source "SuperLTI" -EntryType Information
Write-Progress -Activity $cppslt -Status "Removing SuperLTI Directory..." -PercentComplete 100
Remove-Item -Recurse -Force -Path "C:\SuperLTI\"