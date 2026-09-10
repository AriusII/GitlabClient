[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $ArtifactsDirectory,

    [string] $ExpectedVersion
)

$ErrorActionPreference = 'Stop'

$expectedPackageId = 'GitLab.Client'
$expectedEntries = @(
    'lib/net10.0/GitLab.Client.dll',
    'lib/net10.0/GitLab.Client.xml',
    'lib/net10.0/GitLab.Client.Contracts.dll',
    'lib/net10.0/GitLab.Client.Contracts.xml',
    'lib/net10.0/GitLab.Client.Routing.dll',
    'lib/net10.0/GitLab.Client.Routing.xml',
    'lib/net10.0/GitLab.Client.Http.dll',
    'lib/net10.0/GitLab.Client.Http.xml'
)

$packages = @(Get-ChildItem -LiteralPath $ArtifactsDirectory -Filter '*.nupkg' -File)
if ($packages.Count -ne 1) {
    throw "Expected exactly one NuGet package in '$ArtifactsDirectory', found $($packages.Count)."
}

$package = $packages[0]
$archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)

try {
    $entryNames = @($archive.Entries | ForEach-Object FullName)
    $missingEntries = @($expectedEntries | Where-Object { $_ -notin $entryNames })
    if ($missingEntries.Count -gt 0) {
        throw "Package '$($package.Name)' is missing: $($missingEntries -join ', ')."
    }

    $nuspecEntry = $archive.Entries | Where-Object FullName -eq "$expectedPackageId.nuspec"
    if ($null -eq $nuspecEntry) {
        throw "Package '$($package.Name)' does not contain '$expectedPackageId.nuspec'."
    }

    $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
    try {
        [xml] $nuspec = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}
finally {
    $archive.Dispose()
}

$packageId = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='id']").InnerText
$packageVersion = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']").InnerText

if ($packageId -ne $expectedPackageId) {
    throw "Expected package id '$expectedPackageId', found '$packageId'."
}

if ($ExpectedVersion -and $packageVersion -ne $ExpectedVersion) {
    throw "Expected package version '$ExpectedVersion', found '$packageVersion'."
}

$internalDependencies = @($nuspec.SelectNodes("//*[local-name()='dependency' and starts-with(@id, 'GitLab.Client.')]") | ForEach-Object { $_.GetAttribute('id') })
if ($internalDependencies.Count -gt 0) {
    throw "The single-package distribution must not depend on GitLab.Client.* packages: $($internalDependencies -join ', ')."
}

Write-Host "Verified $($package.Name): one package, all module assemblies included, no GitLab.Client.* package dependencies."
