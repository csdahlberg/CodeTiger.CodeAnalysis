<#
.SYNOPSIS
    Builds and tests all CodeTiger.CodeAnalysis projects.
.DESCRIPTION
    This script builds, runs unit tests for, and performs self-analysis of both Debug and Release configurations of
    all code in the CodeTiger.CodeAnalysis product. This script can be used to verify that changes will pass
    automated checks performed for pull requests.
#>
Param(
    [string] $Verbosity = "minimal", # The logging level to be used for dotnet, msbuild, and vstest
    [switch] $SkipTests = $false,
    [switch] $SkipDebug = $false
)

$ErrorActionPreference = "Stop"

Add-Type -Assembly "System.IO.Compression"
Add-Type -Assembly "System.IO.Compression.FileSystem"

function Get-RelationshipId {
    param (
        [string]$Path
    )

    try {
        $sha512 = New-Object Security.Cryptography.SHA512Managed
        $sha512Bytes = $sha512.ComputeHash([Text.Encoding]::UTF8.GetBytes($Path))
    } finally {
        if ($null -ne $sha512) {
            $sha512.Dispose()
        }
    }
    
    $sha512HexString = ($sha512Bytes | ForEach-Object ToString X2) -join ""

    "R" + $sha512HexString.Substring(0, 16)
}

function New-NuGetPackageFromNuSpec {
    param (
        [string]$NuSpecFilePath,
        [string]$OutputDirectory
    )

    Write-Host "Reading '$NuSpecFilePath'..."
    $nuSpec = [xml](Get-Content $NuSpecFilePath)
    $packageId = $nuSpec.package.metadata.id
    $packageVersion = $nuSpec.package.metadata.version
    $nuPkgFilePath = [IO.Path]::GetFullPath([IO.Path]::Combine($OutputDirectory, "$packageId.$packageVersion.nupkg"))
    
    if ([IO.File]::Exists($nuPkgFilePath)) {
        Write-Host "Deleting the existing '$nuPkgFilePath'..."
        [IO.File]::Delete($nuPkgFilePath)
    }
    
    if (-not [IO.Directory]::Exists($OutputDirectory)) {
        [IO.Directory]::CreateDirectory($OutputDirectory)
    }

    Write-Host "Creating '$nuPkgFilePath'..."
    $nuPkgFile = $null
    try {
        $nuPkgFile = [IO.Compression.ZipFile]::Open($nuPkgFilePath, [IO.Compression.ZipArchiveMode]::Create)
    
        $psmdcpFileName = [Guid]::NewGuid().ToString("N") # NOTE: https://github.com/NuGet/Home/issues/8601 would make this filename deterministic instead of random
        $psmdcpFilePath = "package/services/metadata/core-properties/$psmdcpFileName.psmdcp"
    
        # Write /_rels/.rels
        $relsFileDoc = New-Object Xml.XmlDocument
        $relsFileDoc.AppendChild($relsFileDoc.CreateXmlDeclaration("1.0", "utf-8", $null)) > $null
        $relsRelationshipsElement = $relsFileDoc.CreateElement("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships")
        $nuSpecRelationshipElement = $relsFileDoc.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships")
        $nuSpecRelationshipElement.SetAttribute("Type", "http://schemas.microsoft.com/packaging/2010/07/manifest")
        $nuSpecRelationshipElement.SetAttribute("Target", "/$packageId.nuspec")
        $nuSpecRelationshipElement.SetAttribute("Id", (Get-RelationshipId "/$packageId.nuspec"))
        $relsRelationshipsElement.AppendChild($nuSpecRelationshipElement) > $null
        $psmdcpRelationshipElement = $relsFileDoc.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships")
        $psmdcpRelationshipElement.SetAttribute("Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties")
        $psmdcpRelationshipElement.SetAttribute("Target", "/$psmdcpFilePath")
        $psmdcpRelationshipElement.SetAttribute("Id", (Get-RelationshipId "/$psmdcpFilePath"))
        $relsRelationshipsElement.AppendChild($psmdcpRelationshipElement) > $null
        $relsFileDoc.AppendChild($relsRelationshipsElement) > $null

        $relsFileEntry = $nuPkgFile.CreateEntry("_rels/.rels")
        try {
            $relsFileEntryWriter = New-Object Xml.XmlTextWriter ($relsFileEntry.Open(), [Text.Encoding]::UTF8)
            $relsFileEntryWriter.Formatting = [Xml.Formatting]::Indented
            $relsFileDoc.WriteTo($relsFileEntryWriter)
        }
        finally {
            if ($null -ne $relsFileEntryWriter) {
                $relsFileEntryWriter.Dispose()
            }
        }
    
        # Write /$packageId.nuspec
        Write-Host "Adding '$NuSpecFilePath' as '$packageId.nuspec'..."
        $nuSpecToPack = [xml](Get-Content $NuSpecFilePath)
        $nuSpecToPack.package.RemoveChild($nuSpecToPack.package.files) > $null
        $nuSpecFileEntry = $nuPkgFile.CreateEntry("$packageId.nuspec")
        try {
            $nuSpecFileEntryWriter = New-Object Xml.XmlTextWriter ($nuSpecFileEntry.Open(), [Text.Encoding]::UTF8)
            $nuSpecFileEntryWriter.Formatting = [Xml.Formatting]::Indented
            $nuSpecToPack.WriteTo($nuSpecFileEntryWriter)
        }
        finally {
            if ($null -ne $nuSpecFileEntryWriter) {
                $nuSpecFileEntryWriter.Dispose()
            }
        }
    
        # Write all files that are specified by the nuspec file
        $fileExtensions = New-Object 'Collections.Generic.HashSet[string]'
        $fileExtensions.Add("nuspec") > $null
        $filesWithoutExtensions = New-Object 'Collections.Generic.HashSet[string]'
        $files = $nuSpec.package.files.file
        $files | ForEach-Object {
            $source = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, $_.src.Replace("\", "/")))
            $target = $_.target.Replace("\", "/")
            Write-Host "    Adding '$source' as '$target'..."
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($nuPkgFile, $source, $target) > $null

            $fileExtension = [IO.Path]::GetExtension($source)
            if ([string]::IsNullOrEmpty($fileExtension)) {
                $filesWithoutExtensions.Add("/" + $target.Replace("\", "/")) > $null
            } else {
                $fileExtensions.Add($fileExtension.Substring(1)) > $null
            }
        }
        
        # Write /[Content_Types].xml
        $typesFileDoc = New-Object Xml.XmlDocument
        $typesFileDoc.AppendChild($typesFileDoc.CreateXmlDeclaration("1.0", "utf-8", $null)) > $null
        $typesElement = $typesFileDoc.CreateElement("Types", "http://schemas.openxmlformats.org/package/2006/content-types")
        $relsTypeElement = $typesFileDoc.CreateElement("Default", "http://schemas.openxmlformats.org/package/2006/content-types")
        $relsTypeElement.SetAttribute("Extension", "rels")
        $relsTypeElement.SetAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")
        $typesElement.AppendChild($relsTypeElement) > $null
        $psmdcpTypeElement = $typesFileDoc.CreateElement("Default", "http://schemas.openxmlformats.org/package/2006/content-types")
        $psmdcpTypeElement.SetAttribute("Extension", "psmdcp")
        $psmdcpTypeElement.SetAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml")
        $typesElement.AppendChild($psmdcpTypeElement) > $null

        $fileExtensions | ForEach-Object {
            $typeElement = $typesFileDoc.CreateElement("Default", "http://schemas.openxmlformats.org/package/2006/content-types")
            $typeElement.SetAttribute("Extension", $_)
            $typeElement.SetAttribute("ContentType", "application/octet")
            $typesElement.AppendChild($typeElement) > $null
        }

        $filesWithoutExtensions | ForEach-Object {
            $typeElement = $typesFileDoc.CreateElement("Override", "http://schemas.openxmlformats.org/package/2006/content-types")
            $typeElement.SetAttribute("PartName", $_)
            $typeElement.SetAttribute("ContentType", "application/octet")
            $typesElement.AppendChild($typeElement) > $null
        }

        $typesFileDoc.AppendChild($typesElement) > $null

        $typesFileEntry = $nuPkgFile.CreateEntry("[Content_Types].xml")
        try {
            $typesFileEntryWriter = New-Object Xml.XmlTextWriter ($typesFileEntry.Open(), [Text.Encoding]::UTF8)
            $typesFileEntryWriter.Formatting = [Xml.Formatting]::Indented
            $typesFileDoc.WriteTo($typesFileEntryWriter)
        }
        finally {
            if ($null -ne $typesFileEntryWriter) {
                $typesFileEntryWriter.Dispose()
            }
        }

        # Write /package/services/metadata/core-properties/$psmdcpFileName.psmdcp
        $psmdcpFileDoc = New-Object Xml.XmlDocument
        $psmdcpFileDoc.AppendChild($psmdcpFileDoc.CreateXmlDeclaration("1.0", "utf-8", $null)) > $null
        $corePropertiesElement = $psmdcpFileDoc.CreateElement("coreProperties", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties")
        $corePropertiesElement.SetAttribute("xmlns:dc", "http://purl.org/dc/elements/1.1/")
        $corePropertiesElement.SetAttribute("xmlns:dcterms", "http://purl.org/dc/terms/")
        $corePropertiesElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")
        $creatorElement = $psmdcpFileDoc.CreateElement("dc:creator", "http://purl.org/dc/elements/1.1/")
        $creatorElement.InnerText = $nuSpec.package.metadata.authors
        $corePropertiesElement.AppendChild($creatorElement) > $null
        $descriptionElement = $psmdcpFileDoc.CreateElement("dc:description", "http://purl.org/dc/elements/1.1/")
        $descriptionElement.InnerText = $nuSpec.package.metadata.description
        $corePropertiesElement.AppendChild($descriptionElement) > $null
        $idElement = $psmdcpFileDoc.CreateElement("dc:identifier", "http://purl.org/dc/elements/1.1/")
        $idElement.InnerText = $packageId
        $corePropertiesElement.AppendChild($idElement) > $null
        $versionElement = $psmdcpFileDoc.CreateElement("version", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties")
        $versionElement.InnerText = $nuSpec.package.metadata.version
        $corePropertiesElement.AppendChild($versionElement) > $null
        $keywordsElement = $psmdcpFileDoc.CreateElement("keywords", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties")
        $keywordsElement.InnerText = $nuSpec.package.metadata.tags
        $corePropertiesElement.AppendChild($keywordsElement) > $null
        $lastModifiedByElement = $psmdcpFileDoc.CreateElement("lastModifiedBy", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties")
        $lastModifiedByElement.InnerText = "New-NuGetPackageFromNuSpec"
        $corePropertiesElement.AppendChild($lastModifiedByElement) > $null
        $psmdcpFileDoc.AppendChild($corePropertiesElement) > $null

        $psmdcpFileEntry = $nuPkgFile.CreateEntry("package/services/metadata/core-properties/$psmdcpFileName.psmdcp")
        try {
            $psmdcpFileEntryWriter = New-Object Xml.XmlTextWriter ($psmdcpFileEntry.Open(), [Text.Encoding]::UTF8)
            $psmdcpFileEntryWriter.Formatting = [Xml.Formatting]::Indented
            $psmdcpFileDoc.WriteTo($psmdcpFileEntryWriter)
        }
        finally {
            if ($null -ne $psmdcpFileEntryWriter) {
                $psmdcpFileEntryWriter.Dispose()
            }
        }
    }
    catch {
        Write-Host "Failed to create the NuGet package: $_"
        throw
    }
    finally {
        if ($nuPkgFile -ne $null) {
            $nuPkgFile.Dispose()
        }
    }
}

if ([Environment]::Is64BitOperatingSystem)
{
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
} else {
    $ProgramFilesDir = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles)
}

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

$vsInstallPath = & $vswherePath @("-latest", "-products", "*", "-requires", "Microsoft.VisualStudio.Component.VSSDK", "-property", "installationPath")
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

if ($SkipDebug) {
    $configurations = @("Release")
} else {
    $configurations = @("Debug", "Release")
}

$roslynVersions = @("2.6", "3.8", "4.0")
$roslynVersions | Foreach-Object {
    $roslynVersion = $_
    $configurations | Foreach-Object {
        $configuration = $_

        Write-Host ""
        Write-Host "Building and testing the $configuration configuration for Roslyn $roslynVersion..." -ForegroundColor DarkBlue -BackgroundColor Cyan

        $slnPath = [IO.Path]::Combine($PSScriptRoot, "CodeTiger.CodeAnalysis.sln")

        Write-Host "Building CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
        & $msbuildPath @($slnPath, "-restore", "-verbosity:$Verbosity", "-property:Configuration=$($configuration)WithVsix", "-property:RoslynVersion=$roslynVersion")
        
        if ($LASTEXITCODE -ne 0)
        {
            throw "Building CodeTiger.CodeAnalysis.sln failed."
        }
        
        if (-not $SkipTests) {
            Write-Host ""
            Write-Host "Running unit tests for CodeTiger.CodeAnalysis.sln..." -ForegroundColor Cyan
            $testDllPath = [IO.Path]::Combine($PSScriptRoot, "Build", $configuration, "roslyn$roslynVersion", "net7.0", "UnitTests.CodeTiger.CodeAnalysis.dll")
            & $vstestPath @($testDllPath, "/Parallel", "/Logger:Console;Verbosity=$verbosity")
            
            if ($LASTEXITCODE -ne 0)
            {
                throw "Unit testing CodeTiger.CodeAnalysis.sln failed."
            }
        }
    }

    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "Building CodeTiger.CodeAnalysis.SelfTests.sln for the $configuration configuration for Roslyn $roslynVersion..." -ForegroundColor Cyan

        $selfTestSlnPath = [IO.Path]::Combine($PSScriptRoot, "SelfTests", "CodeTiger.CodeAnalysis.SelfTests.sln")

        & dotnet @("build", $selfTestSlnPath, "--verbosity", "$Verbosity", "--configuration", "$configuration", "/p:RoslynVersion=$roslynVersion")
        
        if ($LASTEXITCODE -ne 0) {
            throw "Building CodeTiger.CodeAnalysis.SelfTests.sln failed."
        }
    }
}

Write-Host ""
Write-Host "Building the CodeTiger.CodeAnalysis NuGet package..." -ForegroundColor DarkBlue -BackgroundColor Cyan

$nuSpecPath = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "CodeTiger.CodeAnalysis.nuspec"))
$outputDirectory = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "Build", "Release"))

New-NuGetPackageFromNuSpec -NuSpecFilePath $nuSpecPath -OutputDirectory $outputDirectory

Write-Host ""
Write-Host "Build succeeded." -ForegroundColor Green
