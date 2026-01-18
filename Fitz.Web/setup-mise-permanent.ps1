# Script to permanently add mise activation to PowerShell profile

$profilePath = $PROFILE.CurrentUserAllHosts
$activationCommand = 'mise activate pwsh | Out-String | Invoke-Expression'

Write-Host "Setting up mise for permanent activation..." -ForegroundColor Cyan
Write-Host "Profile path: $profilePath" -ForegroundColor Gray

# Check if profile exists
if (-not (Test-Path $profilePath)) {
    Write-Host "Creating PowerShell profile..." -ForegroundColor Yellow
    $profileDir = Split-Path $profilePath -Parent
    if (-not (Test-Path $profileDir)) {
        New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
    }
    New-Item -ItemType File -Path $profilePath -Force | Out-Null
}

# Check if mise activation is already in profile
$profileContent = Get-Content $profilePath -ErrorAction SilentlyContinue
if ($profileContent -match [regex]::Escape('mise activate pwsh')) {
    Write-Host "mise activation is already in your PowerShell profile!" -ForegroundColor Green
    Write-Host "No changes needed." -ForegroundColor Gray
} else {
    Write-Host "Adding mise activation to PowerShell profile..." -ForegroundColor Yellow
    Add-Content -Path $profilePath -Value "`n# mise activation"
    Add-Content -Path $profilePath -Value $activationCommand
    Write-Host "✓ Added mise activation to profile" -ForegroundColor Green
    Write-Host "`nTo activate mise in your current session, run:" -ForegroundColor Cyan
    Write-Host "  . $profilePath" -ForegroundColor Gray
    Write-Host "`nOr restart your PowerShell terminal." -ForegroundColor Gray
}
