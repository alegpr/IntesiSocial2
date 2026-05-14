$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Join-Path $projectDir "social_V0.0.1.csproj"
$tempPublish = Join-Path $projectDir "bin\Release\publish"
$iisPath = "C:\inetpub\wwwroot\social"

Write-Host "=== BUILD + PUBLISH ===" -ForegroundColor Cyan
dotnet publish $csproj -c Release -o $tempPublish
if ($LASTEXITCODE -ne 0) { Write-Host "FALLITO" -ForegroundColor Red; Read-Host "Premi Invio"; exit 1 }

Write-Host "`n=== COPY TO IIS ===" -ForegroundColor Cyan
if (!(Test-Path $iisPath)) { New-Item $iisPath -ItemType Directory -Force | Out-Null }
robocopy $tempPublish $iisPath /E /NP /NFL /NDL /NJH /NJS > $null

Write-Host "`n=== RESTART IIS SITE ===" -ForegroundColor Cyan
& "$env:windir\system32\inetsrv\appcmd.exe" stop site "SocialApp" 2>$null
& "$env:windir\system32\inetsrv\appcmd.exe" start site "SocialApp"

Write-Host "`n=== DONE: http://localhost:5053 ===" -ForegroundColor Green
