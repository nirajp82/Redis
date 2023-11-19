# Navigate to the Root of the Repository
cd C:\Projects\Redis\RU102N\Redis


# Find and remove bin and obj folders
# Find and remove bin and obj folders
Get-ChildItem -Recurse | Where-Object { $_.PSIsContainer -and ($_.Name -eq 'bin' -or $_.Name -eq 'obj') } | ForEach-Object {
    Remove-Item -Path $_.FullName -Force -Recurse
}

# Display a message
Write-Host "Removed all bin and obj folders from subdirectories."

