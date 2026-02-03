param (
    [string]$SourcePath,
    [string]$TargetPath
)

$sourceFiles = Get-ChildItem -Path $SourcePath -Recurse | Where-Object { -not $_.PSIsContainer }
$results = @()

foreach ($sFile in $sourceFiles) {
    $relativePath = $sFile.FullName.Substring($SourcePath.Length + 1)
    $tFile = Join-Path $TargetPath $relativePath
    
    if (Test-Path $tFile) {
        $localSize = (Get-Item $tFile).Length
        if ($sFile.Length -ne $localSize) {
            $results += "$relativePath - Diff (Source: $($sFile.Length), Local: $localSize)"
        }
    } else {
        $results += "$relativePath - MISSING"
    }
}

$results | Out-File "comparison_results.txt"
