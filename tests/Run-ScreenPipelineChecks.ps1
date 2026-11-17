param([string]$Configuration = 'Debug', [ValidateSet('AnyCPU','x86')] [string]$Platform = 'AnyCPU', [switch]$ChecksOnly,
    [string]$ReportName = 'turbo-delta-metrics')
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $repo ('HJJJJ.DeskReach.Windows\bin\' + $Configuration)
$compiler = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe'
}
$framework = 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2'
$exe = Join-Path $bin ('ScreenPipelineChecks-' + $Platform + '.exe')
$reportDirectory = Join-Path $repo 'output\lan-performance-20261015'
New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
$arguments = @('/nologo', '/target:exe', ('/platform:' + $Platform), ('/out:' + $exe),
    ('/reference:' + (Join-Path $bin 'HJJJJ.DeskReach.Windows.exe')),
    ('/reference:' + (Join-Path $bin 'HJJJJ.DeskReach.dll')),
    ('/reference:' + (Join-Path $bin 'HJJJJ.DeskReach.Plugins.TextMessage.Windows.dll')),
    ('/reference:' + (Join-Path $bin 'HJJJJ.DeskReach.Plugins.FileTransfer.Windows.dll')),
    ('/reference:' + (Join-Path $framework 'Facades\netstandard.dll')),
    '/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll',
    '/reference:System.Windows.Forms.dll', '/reference:System.Web.Extensions.dll',
    (Join-Path $PSScriptRoot 'ScreenPipelineChecks.cs'), (Join-Path $PSScriptRoot 'TurboDeltaChecks.cs'))
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Screen checks compilation failed.' }
Copy-Item -LiteralPath (Join-Path $bin 'HJJJJ.DeskReach.Windows.exe.config') -Destination ($exe + '.config') -Force
$runArguments = @((Join-Path $reportDirectory ($ReportName + '-' + $Platform + '.json')))
if ($ChecksOnly) { $runArguments += '--checks-only' }
& $exe @runArguments
if ($LASTEXITCODE -ne 0) { throw 'Screen checks failed.' }
