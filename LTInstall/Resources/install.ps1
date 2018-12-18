$cppslt = "CP PS LiteTouch / Package Install Director / V.1.0"
Write-Host -Object "Installing, Please wait..."
Write-Progress -Activity $cppslt -Status "Creating Install Directory..." -PercentComplete 0
New-Item -Force -Path "C:\INSTALL\" -ItemType "Directory" | Out-Null
Write-Progress -Activity $cppslt -Status "Downloading Install Files..." -PercentComplete 5
Copy-Item -Path "install.zip" -Destination "C:\INSTALL\install.zip" -Force
Write-Progress -Activity $cppslt -Status "Extracting Install Files..." -PercentComplete 20
Expand-Archive -Path "C:\INSTALL\install.zip" -DestinationPath "C:\INSTALL\" -Force
Write-Progress -Activity $cppslt -Status "Running Install Script..." -PercentComplete 40
&"C:\INSTALL\install.ps1"
Write-Progress -Activity $cppslt -Status "Cleaning Up..." -PercentComplete 99 -SecondsRemaining 10
Remove-Item -Recurse -Force -Path "C:\INSTALL\"
Write-Host -Object "Done!"