param(
  [string]$BaseUrl = "https://localhost:44300",
  [string]$SqlServer = ".\SQLEXPRESS",
  [string]$SqlDatabase = "12COFFEE"
)

$ErrorActionPreference = "Stop"
$nodeExe = "C:\Users\Admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe"
$nodeModules = "C:\Users\Admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules"
$pnpmNodeModules = "C:\Users\Admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules\.pnpm\node_modules"
$script = Join-Path $PSScriptRoot "capture_screenshots.js"

if (!(Test-Path $nodeExe)) {
  throw "Không tìm thấy Node runtime: $nodeExe"
}
if (!(Test-Path $script)) {
  throw "Không tìm thấy script: $script"
}

$env:NODE_PATH = "$nodeModules;$pnpmNodeModules"
$env:BASE_URL = $BaseUrl
$env:SQL_SERVER = $SqlServer
$env:SQL_DATABASE = $SqlDatabase

& $nodeExe $script
