<#
.SYNOPSIS
    Builds and tests all CodeTiger.CodeAnalysis projects.
.DESCRIPTION
    This script builds, runs unit tests for, and performs self-analysis of both Debug and Release configurations of
    all code in the CodeTiger.CodeAnalysis product. This script can be used to verify that changes will pass
    automated checks performed for pull requests.
#>
param(
    [string] $Verbosity = "minimal", # The logging level to be used for dotnet, msbuild, and vstest
    [switch] $SkipTests = $false,
    [switch] $SkipDebug = $false,
    [switch] $SkipVsix = $false,
    [switch] $CleanBeforeBuilding = $false
)

$ErrorActionPreference = "Stop"

. ./New-NuGetPackageFromNuSpec.ps1

if ([Environment]::Is64BitOperatingSystem)
{
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
} else {
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles)
}

# If building the vsix project, find the msbuild and vstest paths
if (-not $SkipVsix) {

    if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows)) {
        throw "The VSIX project can only be built on Windows. Try running the script with the '-SkipVsix' switch."
    }
    
    if ([string]::IsNullOrWhiteSpace($ProgramFilesDir))
    {
        throw "The 32-bit Program Files folder could not be found"
    }
    
    $vswherePath = [IO.Path]::Combine($ProgramFilesDir, "Microsoft Visual Studio", "Installer", "vswhere.exe")
    if (-not [IO.File]::Exists($vswherePath))
    {
        throw "vswhere.exe could not be found"
    }
    
    Write-Host "Using $vswherePath to find MSBuild.exe..." -ForegroundColor Cyan
    
    $vsInstallPath = & $vswherePath @("-latest", "-prerelease", "-products", "*", "-requires", "Microsoft.VisualStudio.Component.VSSDK", "-property", "installationPath")
    if ([string]::IsNullOrWhiteSpace($vsInstallPath))
    {
        throw "No instances of Visual Studio with the ""Visual Studio extension development"" component could be found"
    }
    
    Write-Host "Using Visual Studio installed at $vsInstallPath" -ForegroundColor Cyan
    
    $msbuildPath = [IO.Path]::Combine($vsInstallPath, "MSBuild", "Current", "Bin", "msbuild.exe")
    if (-not [IO.File]::Exists($msbuildPath))
    {
        throw "MSBuild.exe was not found at $msbuildPath"
    }
    
    Write-Host "Found MSBuild at $msbuildPath" -ForegroundColor Cyan
    
    $vstestPath = [IO.Path]::Combine($vsInstallPath, "Common7", "IDE", "Extensions", "TestPlatform", "vstest.console.exe")
    if (-not [IO.File]::Exists($vstestPath))
    {
        throw "vstest.console.exe could not be found at $vstestPath"
    }
    
    Write-Host "Found vstest.console.exe at $vstestPath" -ForegroundColor Cyan
}

if ($SkipDebug) {
    $configurations = @("Release")
} else {
    $configurations = @("Debug", "Release")
}

$roslynVersions = @("2.6", "3.8", "4.0", "4.4")
$roslynVersions | Foreach-Object {
    $roslynVersion = $_
    $configurations | Foreach-Object {
        $configuration = $_

        Write-Host ""
        Write-Host "Building and testing the $configuration configuration for Roslyn $roslynVersion..." -ForegroundColor DarkBlue -BackgroundColor Cyan

        $slnPath = [IO.Path]::Combine($PSScriptRoot, "CodeTiger.CodeAnalysis.sln")

        if (-not $SkipVsix) {

            if ($CleanBeforeBuilding) {
                Write-Host "Cleaning CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
                & $msbuildPath @($slnPath, "-t:Clean", "-verbosity:$Verbosity", "-property:Configuration=$($configuration)WithVsix", "-property:RoslynVersion=$roslynVersion")
            }

            Write-Host "Building CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
            & $msbuildPath @($slnPath, "-restore", "-verbosity:$Verbosity", "-property:Configuration=$($configuration)WithVsix", "-property:RoslynVersion=$roslynVersion")
            
            if ($LASTEXITCODE -ne 0)
            {
                throw "Building CodeTiger.CodeAnalysis.sln failed."
            }
            
            if (-not $SkipTests) {
                Write-Host ""
                Write-Host "Running unit tests for CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
                $testDllPath = [IO.Path]::Combine($PSScriptRoot, "Build", $configuration, "roslyn$roslynVersion", "UnitTests.CodeTiger.CodeAnalysis.dll")
                & $vstestPath @($testDllPath, "/Parallel", "/Logger:Console;Verbosity=$verbosity")
                
                if ($LASTEXITCODE -ne 0)
                {
                    throw "Unit testing CodeTiger.CodeAnalysis.sln failed."
                }
            }
        } else {
            
            if ($CleanBeforeBuilding) {
                & dotnet @("clean", $slnPath, "--verbosity", "$Verbosity", "--configuration", "$configuration", "/p:RoslynVersion=$roslynVersion")
            }

            if (-not $SkipTests) {
                & dotnet @("test", $slnPath, "--verbosity", "$Verbosity", "--configuration", "$configuration", "/p:RoslynVersion=$roslynVersion")
            } else {
                & dotnet @("build", $slnPath, "--verbosity", "$Verbosity", "--configuration", "$configuration", "/p:RoslynVersion=$roslynVersion")
            }

            if ($LASTEXITCODE -ne 0) {
                throw "Building or testing CodeTiger.CodeAnalysis.sln failed."
            }
        }
    }
}

if (-not $SkipTests) {

    $configuration = 'Release'
    # Run self-testing with the latest supported version of Roslyn so that newer language features can be used
    $roslynVersion = "4.4"

    Write-Host ""
    Write-Host "Building CodeTiger.CodeAnalysis.SelfTests.sln for the $configuration configuration for Roslyn $roslynVersion..." -ForegroundColor Cyan

    $selfTestSlnPath = [IO.Path]::Combine($PSScriptRoot, "SelfTests", "CodeTiger.CodeAnalysis.SelfTests.sln")

    & dotnet @("build", $selfTestSlnPath, "--verbosity", "$Verbosity", "--configuration", "$configuration", "/p:RoslynVersion=$roslynVersion")
    
    if ($LASTEXITCODE -ne 0) {
        throw "Building CodeTiger.CodeAnalysis.SelfTests.sln failed."
    }
}

Write-Host ""
Write-Host "Building the CodeTiger.CodeAnalysis NuGet package..." -ForegroundColor DarkBlue -BackgroundColor Cyan

$nuSpecPath = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "CodeTiger.CodeAnalysis.nuspec"))
$outputDirectory = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "Build", "Release"))

New-NuGetPackageFromNuSpec -NuSpecFilePath $nuSpecPath -OutputDirectory $outputDirectory

Write-Host ""
Write-Host "Build succeeded." -ForegroundColor Green
