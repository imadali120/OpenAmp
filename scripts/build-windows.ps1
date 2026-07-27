param(
    [string]$ApiUrl = "http://127.0.0.1:5264"
)

$ErrorActionPreference = "Stop"
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\src\OpenAmp.Mobile")).Path
$temporaryAtlRoot = Join-Path $env:TEMP "openamp-atl"
$layoutRoot = Join-Path $temporaryAtlRoot "layout"
$extractRoot = Join-Path $temporaryAtlRoot "extracted"

Push-Location $projectRoot
try {
    # Pub get može raditi bez Developer Modea dok je Windows target privremeno isključen.
    flutter config --no-enable-windows-desktop | Out-Null
    flutter pub get
    if ($LASTEXITCODE -ne 0) {
        throw "flutter pub get nije uspio."
    }
}
finally {
    flutter config --enable-windows-desktop | Out-Null
    Pop-Location
}

$pluginsFile = Join-Path $projectRoot ".flutter-plugins-dependencies"
$linkRoot = Join-Path $projectRoot "windows\flutter\ephemeral\.plugin_symlinks"
New-Item -ItemType Directory -Force -Path $linkRoot | Out-Null
$plugins = (Get-Content -Raw -LiteralPath $pluginsFile | ConvertFrom-Json).plugins.windows
foreach ($plugin in $plugins) {
    $destination = Join-Path $linkRoot $plugin.name
    if (-not (Test-Path -LiteralPath $destination)) {
        New-Item -ItemType Junction -Path $destination -Target $plugin.path | Out-Null
    }
}

$buildToolsRoot = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"
$atlHeader = Get-ChildItem $buildToolsRoot -Recurse -Filter atlbase.h -ErrorAction SilentlyContinue |
    Select-Object -First 1
$atlLibrary = Get-ChildItem $buildToolsRoot -Recurse -Filter atls.lib -ErrorAction SilentlyContinue |
    Where-Object { $_.DirectoryName -like "*\x64" } |
    Select-Object -First 1

if (-not $atlHeader -or -not $atlLibrary) {
    New-Item -ItemType Directory -Force -Path $layoutRoot, $extractRoot | Out-Null
    $bootstrapper = Join-Path $temporaryAtlRoot "vs_buildtools.exe"
    if (-not (Test-Path -LiteralPath $bootstrapper)) {
        curl.exe --silent --show-error --fail --location `
            "https://aka.ms/vs/17/release/vs_buildtools.exe" `
            --output $bootstrapper
    }

    $headersPackage = Get-ChildItem $layoutRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "ATL\.Headers\.base.*payload\.vsix$" } |
        Select-Object -First 1
    $x64Package = Get-ChildItem $layoutRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "ATL\.X64\.base.*payload\.vsix$" } |
        Select-Object -First 1

    if (-not $headersPackage -or -not $x64Package) {
        & $bootstrapper --layout $layoutRoot `
            --add Microsoft.VisualStudio.Component.VC.ATL `
            --lang en-US --quiet

        $deadline = (Get-Date).AddMinutes(5)
        do {
            Start-Sleep -Seconds 2
            $headersPackage = Get-ChildItem $layoutRoot -Recurse -File -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -match "ATL\.Headers\.base.*payload\.vsix$" } |
                Select-Object -First 1
            $x64Package = Get-ChildItem $layoutRoot -Recurse -File -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -match "ATL\.X64\.base.*payload\.vsix$" } |
                Select-Object -First 1
        } while (
            (-not $headersPackage -or -not $x64Package) -and
            (Get-Date) -lt $deadline
        )
    }

    if (-not $headersPackage -or -not $x64Package) {
        throw "Microsoft ATL paket nije moguće preuzeti."
    }
    tar -xf $headersPackage.FullName -C $extractRoot
    tar -xf $x64Package.FullName -C $extractRoot
    $atlHeader = Get-ChildItem $extractRoot -Recurse -Filter atlbase.h | Select-Object -First 1
    $atlLibrary = Get-ChildItem $extractRoot -Recurse -Filter atls.lib |
        Where-Object { $_.DirectoryName -like "*\x64" } |
        Select-Object -First 1
}

$env:CL = "/I`"$($atlHeader.DirectoryName)`""
$env:LINK = "/LIBPATH:`"$($atlLibrary.DirectoryName)`""

Push-Location $projectRoot
try {
    flutter build windows --release --dart-define="OPENAMP_API_URL=$ApiUrl"
    if ($LASTEXITCODE -ne 0) {
        throw "Flutter Windows build nije uspio."
    }
}
finally {
    Pop-Location
}

Write-Output "Windows build: $projectRoot\build\windows\x64\runner\Release"
