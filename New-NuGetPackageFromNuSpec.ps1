<#
.SYNOPSIS
    Creates a NuGet package from a NuSpec file.
.DESCRIPTION
    This is a basic script for building a NuGet package (.nupkg) from a NuSpec file (.nuspec). It is based on
    the C# client code from https://github.com/NuGet/NuGet.Client, but likely does not include all functionality.
.PARAMETER NuSpecFilePath
    Specifies the .nuspec file to build a NuGet package from.
.PARAMETER OutputDirectory
    Specifies the directory to write the .nupkg file to. The name of the file will be determined by the package ID
    and package version from the .nuspec file.
.EXAMPLE
    New-NuGetPackageFromNuSpec ./Calculator.nuspec
.EXAMPLE
    New-NuGetPackageFromNuSpec ./Calculator.nuspec ./publish/
.EXAMPLE
    New-NuGetPackageFromNuSpec ./Calculator.nuspec -OutputDirectory ./publish/
#>

$ErrorActionPreference = "Stop"

Add-Type -Assembly "System.IO.Compression"
Add-Type -Assembly "System.IO.Compression.FileSystem"

function New-NuGetPackageFromNuSpec {
    [CmdletBinding(DefaultParameterSetName = "Directory")]
    param (
        [Parameter(Position = 0)]
        [string]$NuSpecFilePath,
        [Parameter(Position = 1)]
        [string]$OutputDirectory = "."
    )

    # Convert-Path (which this script uses to convert paths like ".\Foo\Bar.txt" to "C:\Repo\Foo\Bar.txt") fails if
    # the path does not exist. There does not currently appear to be a proper way to get the resolved path in that
    # case. This is a hacky way. See also https://github.com/PowerShell/PowerShell/issues/2993.
    function Convert-PathWithoutValidation {
        param (
            [string]$Path
        )
        
        try {
            return Convert-Path $Path
        } catch {
            return $_.TargetObject
        }
    }

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

    if ([string]::IsNullOrEmpty($OutputDirectory)) {
        throw "The 'OutputDirectory' parameter must be specified."
    }

    $NuSpecFilePath = Convert-PathWithoutValidation $NuSpecFilePath
    $nuSpecDirectory = Split-Path $NuSpecFilePath -Parent

    Write-Host "Reading '$NuSpecFilePath'..."
    $nuSpec = [xml](Get-Content $NuSpecFilePath)
    $packageId = $nuSpec.package.metadata.id

    $packageVersion = $nuSpec.package.metadata.version
    $nuPkgFilePath = Join-Path $OutputDirectory "$packageId.$packageVersion.nupkg"
    
    if (Test-Path $nuPkgFilePath) {
        Write-Host "Deleting the existing '$nuPkgFilePath'..."
        Remove-Item $nuPkgFilePath
    }
    
    $nuPkgDirectory = Split-Path $nuPkgFilePath -Parent
    if (-not (Test-Path $nuPkgDirectory)) {
        Write-Host "Creating '$nuPkgDirectory'..."
        New-Item $nuPkgDirectory -ItemType Directory
    }

    Write-Host "Creating '$nuPkgFilePath'..."

    $nuPkgFile = $null
    try {
        $nuPkgFile = [IO.Compression.ZipFile]::Open($nuPkgFilePath, [IO.Compression.ZipArchiveMode]::Create)
    
        # NOTE: https://github.com/NuGet/Home/issues/8601 would make this filename deterministic instead of random
        $psmdcpFileName = [Guid]::NewGuid().ToString("N")
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

            $source = Convert-PathWithoutValidation (Join-Path $nuSpecDirectory $_.src)

            # NuGet packages always use forward slashes for directory separators
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
