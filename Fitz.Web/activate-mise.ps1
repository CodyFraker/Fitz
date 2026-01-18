# mise Activation Script for PowerShell
# Run this script to activate mise in your current PowerShell session
# Or add the command to your PowerShell profile for permanent activation

Write-Host "Activating mise..." -ForegroundColor Cyan

# Activate mise for this session
mise activate pwsh | Out-String | Invoke-Expression

Write-Host "mise activated! Verifying tools..." -ForegroundColor Green

# Verify tools are available
Write-Host "`nChecking Node.js..." -ForegroundColor Yellow
node --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Node.js is available" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js not found" -ForegroundColor Red
}

Write-Host "`nChecking npm..." -ForegroundColor Yellow
npm --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ npm is available" -ForegroundColor Green
} else {
    Write-Host "✗ npm not found" -ForegroundColor Red
}

Write-Host "`nmise is now active in this session!" -ForegroundColor Green
Write-Host "To make it permanent, add this to your PowerShell profile:" -ForegroundColor Cyan
Write-Host '  mise activate pwsh | Out-String | Invoke-Expression' -ForegroundColor Gray
