Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Starting NorthWind Shipping - DEV MODE (Windows)" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "[0/3] Cleaning up zombie processes..."
Get-Process -Name "Northwind*", "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
# Try to kill node if running dev
Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*vite*" } | Stop-Process -Force

Write-Host "[1/3] Starting SQL Server Database via Docker..."
docker compose up -d sqlserver

Write-Host "[2/3] Starting .NET Web API with Hot Reload..."
# Using Start-Job for background execution
Start-Job -Name "NorthwindAPI" -ScriptBlock {
    Set-Location $using:PWD
    dotnet watch run --project project_API/Northwind.WebAPI
}

Write-Host "[3/3] Starting Vue Frontend with Hot Reload..."
Start-Job -Name "NorthwindFrontend" -ScriptBlock {
    Set-Location "$using:PWD/frontend"
    npm run dev
}

Write-Host "=========================================" -ForegroundColor Green
Write-Host " All services are starting!" -ForegroundColor Green
Write-Host " - API will be at: http://localhost:8080/swagger"
Write-Host " - Web will be at: http://localhost:5173"
Write-Host " Note: Logs are running in background jobs."
Write-Host " To stop everything, run: Stop-Job NorthwindAPI, NorthwindFrontend"
Write-Host "=========================================" -ForegroundColor Green

# Optional: keep window open and show logs
Write-Host "Showing API logs (Press Ctrl+C to stop viewing logs, jobs will continue)..."
Receive-Job -Name "NorthwindAPI" -Keep -Wait
