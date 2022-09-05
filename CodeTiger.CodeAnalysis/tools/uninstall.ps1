param($installPath, $toolsPath, $package, $project)

if ($project.Object.SupportsPackageDependencyResolution)
{
    if ($project.Object.SupportsPackageDependencyResolution())
    {
        # Do not install analyzers via install.ps1, instead let the project system handle it.
        return
    }
}

# NuGet 3.x+ and Visual Studio 2019 do not seem to run install.ps1/uninstall.ps1 scripts. For Visual Studio 2017,
# install the Roslyn 2.6 version of the analyzer library.
$rootPath = [IO.Path]::GetDirectoryName($toolsPath)
$analyzerFilePath = [IO.Path]::Combine($rootPath, "analyzers", "dotnet", "roslyn2.6", "cs", "CodeTiger.CodeAnalysis.dll")

if ($project.Object.AnalyzerReferences)
{
    $project.Object.AnalyzerReferences.Remove($analyzerFilePath)
}
