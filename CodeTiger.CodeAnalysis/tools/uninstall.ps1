param($installPath, $toolsPath, $package, $project)

$analyzersPath = join-path $toolsPath "analyzers"
$analyzerFilePath = join-path $analyzersPath "CodeTiger.CodeAnalysis.dll"

$project.Object.AnalyzerReferences.Remove("$analyzerFilePath")