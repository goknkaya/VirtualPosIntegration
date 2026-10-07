# Kullanım (proje kökünde):  powershell -ExecutionPolicy Bypass -File .\build.ps1
# Çıktı: publish\app (sunucuya kopyalanacak site), publish\db (veritabanı scripti)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$api  = "$root\backend\src\VirtualPos.Api"
$out  = "$root\publish"

function Check($step) { if ($LASTEXITCODE -ne 0) { throw "$step başarısız oldu." } }

Write-Host "1/4 Frontend derleniyor..." -ForegroundColor Cyan
Push-Location "$root\frontend"
npm install;   Check "npm install"
npm run build; Check "npm run build"
Pop-Location

Write-Host "2/4 Frontend, API'nin wwwroot klasörüne kopyalanıyor..." -ForegroundColor Cyan
if (Test-Path "$api\wwwroot") { Remove-Item "$api\wwwroot" -Recurse -Force }
Copy-Item "$root\frontend\dist" "$api\wwwroot" -Recurse

Write-Host "3/4 Backend yayınlanıyor..." -ForegroundColor Cyan
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
dotnet publish "$api\VirtualPos.Api.csproj" -c Release -o "$out\app"; Check "dotnet publish"

Write-Host "4/4 Veritabanı scripti oluşturuluyor..." -ForegroundColor Cyan
dotnet ef migrations script --idempotent `
    --project "$root\backend\src\VirtualPos.Infrastructure" `
    --startup-project $api `
    -o "$out\db\01_schema.sql"; Check "migrations script"

Write-Host "Bitti -> $out" -ForegroundColor Green