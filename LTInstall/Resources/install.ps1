$cppslt = "CP PS LiteTouch / Package Install Director / V.1.1"
Write-Progress -Activity $cppslt -Status "Saving ExecutionPolicy..." -PercentComplete 0
$cppsxp = Get-ExecutionPolicy
Write-Progress -Activity $cppslt -Status "Setting ExecutionPolicy..." -PercentComplete 3
Set-ExecutionPolicy -ExecutionPolicy Bypass -Force
Write-Progress -Activity $cppslt -Status "Creating Install Directory..." -PercentComplete 6
New-Item -Force -Path "C:\INSTALL\" -ItemType "Directory"
Write-Progress -Activity $cppslt -Status "Downloading Install Files..." -PercentComplete 12
Copy-Item -Path "install.zip" -Destination "C:\INSTALL\install.zip" -Force
Write-Progress -Activity $cppslt -Status "Extracting Install Files..." -PercentComplete 20
Expand-Archive -Path "C:\INSTALL\install.zip" -DestinationPath "C:\INSTALL\" -Force
Write-Progress -Activity $cppslt -Status "Running Install Script..." -PercentComplete 40
Set-Location -Path "C:\INSTALL\"
&"C:\INSTALL\install.ps1"
Set-Location -Path "C:\"
Write-Progress -Activity $cppslt -Status "Restoring ExecutionPolicy..." -PercentComplete 90
Set-ExecutionPolicy -ExecutionPolicy $cppsxp -Force
Write-Progress -Activity $cppslt -Status "Removing Install Directory..." -PercentComplete 95 -SecondsRemaining 10
Remove-Item -Recurse -Force -Path "C:\INSTALL\"