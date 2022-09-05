<#
.SYNOPSIS
    Builds and tests all CodeTiger.CodeAnalysis projects.
.DESCRIPTION
    This script builds, runs unit tests for, and performs self-analysis of both Debug and Release configurations of
    all code in the CodeTiger.CodeAnalysis product. This script can be used to verify that changes will pass
    automated checks performed for pull requests.
#>
Param(
    [System.String] $Verbosity = "minimal" # The logging level to be used for msbuild and vstest
)

if ([Environment]::Is64BitOperatingSystem)
{
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
} else {
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles)
}

If ([String]::IsNullOrWhiteSpace($ProgramFilesDir))
{
    throw "The 32-bit Program Files folder could not be found"
}

$vswherePath = $ProgramFilesDir + '\Microsoft Visual Studio\Installer\vswhere.exe'
If (-Not [System.IO.File]::Exists($vswherePath))
{
    throw "vswhere.exe could not be found"
}

Write-Host "Using $vswherePath to find MSBuild.exe..." -ForegroundColor Cyan

$vsInstallPath = & $vswherePath @("-latest", "-products", "*", "-requires", "Microsoft.VisualStudio.Component.VSSDK", "-property", "installationPath")
If ([String]::IsNullOrWhiteSpace($vsInstallPath))
{
    throw "No instances of Visual Studio with the ""Visual Studio extension development"" component could be found"
}

Write-Host "Using Visual Studio installed at $vsInstallPath" -ForegroundColor Cyan

$msbuildPath = [System.IO.Path]::Combine($vsInstallPath, 'MSBuild\Current\Bin\msbuild.exe')
If (-Not [System.IO.File]::Exists($msbuildPath))
{
    throw "MSBuild.exe was not found at $msbuildPath"
}

Write-Host "Found MSBuild at $msbuildPath" -ForegroundColor Cyan

$vstestPath = [System.IO.Path]::Combine($vsInstallPath, 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe')
If (-Not [System.IO.File]::Exists($vstestPath))
{
    throw "vstest.console.exe could not be found at $vstestPath"
}

Write-Host "Found vstest.console.exe at $vstestPath" -ForegroundColor Cyan

$configurations = @("Debug", "Release")
$configurations | Foreach-Object {

    Write-Host ""
    Write-Host "Building and testing the $_ configuration..." -ForegroundColor DarkBlue -BackgroundColor Cyan
    Write-Host "Building CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
    & $msbuildPath @("$PSScriptRoot\CodeTiger.CodeAnalysis.sln", "-restore", "-verbosity:$Verbosity", "-property:Configuration=$_")
    
    If ($LASTEXITCODE -ne 0)
    {
        throw "Building CodeTiger.CodeAnalysis.sln failed."
    }
    
    Write-Host ""
    Write-Host "Running unit tests for CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
    & $vstestPath @("$PSScriptRoot\Build\$_\roslyn4.0\net7.0\UnitTests.CodeTiger.CodeAnalysis.dll", "/Parallel", "/Logger:Console;Verbosity=$verbosity")
    
    If ($LASTEXITCODE -ne 0)
    {
        throw "Unit testing CodeTiger.CodeAnalysis.sln failed."
    }
    
    Write-Host ""
    Write-Host "Building CodeTiger.CodeAnalysis.SelfTests.sln..." -ForegroundColor Cyan
    & $msbuildPath @("$PSScriptRoot\SelfTests\CodeTiger.CodeAnalysis.SelfTests.sln", "-restore", "-verbosity:$Verbosity", "-property:Configuration=$_")
    
    If ($LASTEXITCODE -ne 0)
    {
        throw "Building CodeTiger.CodeAnalysis.SelfTests.sln failed."
    } else
    {
        Write-Host ""
        Write-Host "Build succeeded." -ForegroundColor Green
    }
}
