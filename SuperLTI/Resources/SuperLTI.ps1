$cppslt = "SuperLTI"
Write-Progress -Activity $cppslt -Status "Saving ExecutionPolicy..." -PercentComplete 0
$cppsxp = Get-ExecutionPolicy
Write-Progress -Activity $cppslt -Status "Setting ExecutionPolicy..." -PercentComplete 3
Set-ExecutionPolicy -ExecutionPolicy Bypass -Force
#Write-Progress -Activity $cppslt -Status "Creating SuperLTI Directory..." -PercentComplete 6
#New-Item -Force -Path "C:\SuperLTI\" -ItemType "Directory"
#Write-Progress -Activity $cppslt -Status "Downloading SuperLTI Files..." -PercentComplete 12
#Copy-Item -Path "SuperLTI.zip" -Destination "C:\SuperLTI\SuperLTI.zip" -Force
#Write-Progress -Activity $cppslt -Status "Extracting SuperLTI Files..." -PercentComplete 20
#Expand-Archive -Path "C:\SuperLTI\SuperLTI.zip" -DestinationPath "C:\SuperLTI\" -Force
Write-Progress -Activity $cppslt -Status "Running SuperLTI Script..." -PercentComplete 50
Set-Location -Path "C:\SuperLTI\"
&"C:\SuperLTI\SuperLTI.ps1"
Set-Location -Path "C:\"
Write-Progress -Activity $cppslt -Status "Restoring ExecutionPolicy..." -PercentComplete 90
Set-ExecutionPolicy -ExecutionPolicy $cppsxp -Force
Write-Progress -Activity $cppslt -Status "Removing SuperLTI Directory..." -PercentComplete 95
Remove-Item -Recurse -Force -Path "C:\SuperLTI\"