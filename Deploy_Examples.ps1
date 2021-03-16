function Update-Config()
{
    Param ([string]$configFile)

    $configData = (Get-Content $configFile | ConvertFrom-Json)
    $configData.ExampleConfig.ResourceLocation = "..\Resources\"
    $configData.ExampleConfig.PlugInLocation = "..\PlugIns\"

    (ConvertTo-Json $configData | Set-Content $configfile)
}

$artifactDir = "$env:BUILD_ARTIFACTSTAGINGDIRECTORY\\Examples"

$artifactBin = "$artifactDir\\Bin\\"
$artifactPlugIns = "$artifactDir\\PlugIns\\"
$artifactImagesFolder = "$artifactBin\\Images\\"
$baseSrcResources = "Resources\\"
$baseSrcImages = "Examples\\Gorgon.Graphics\\Images\\Images\\*.*"

if (Test-Path $artifactDir)
{
    Remove-Item $artifactDir -force -Recurse
}

mkdir $artifactDir
mkdir $artifactBin
mkdir $artifactImagesFolder
mkdir $artifactPlugIns

$gorgonExamples = (Get-ChildItem "Examples\\" -include *.dll,*.exe,appsettings.json,*.runtimeconfig.json,TextViewerContentExample_Installation.txt  -Recurse | Where-Object {$_.FullName -match "\\bin\\" -and $_.FullName -notmatch "\\net48\\" -and $_.FullName -notmatch "app.config" -and $_.FullName -notmatch ".vshost.exe" -and $_.FullName -notmatch "\\debug\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "_Test" -and $_.FullName -notmatch "\\publish\\" -and $_.FullName -notmatch "\\ref\\" -and $_.FullName -notmatch "\\runtimes\\" })
$plugInDlls = (Get-ChildItem "PlugIns\\Bin\\" -include *.dll -Recurse | Where-Object { $_.FullName -notmatch "\\debug\\" -and $_.FullName -notmatch "\\net48\\" })

Write-Output "$($gorgonExamples.Length) Example files to copy."
Write-Output "$($plugInDlls.Length) Plug in files to copy."

Copy-Item -Path $baseSrcResources -Exclude Krypton_DarkO2k10Theme.xml -Destination $artifactDir -Recurse -Container
Copy-Item -Path $baseSrcImages -Destination $artifactImagesFolder -Container: $false

$appsettingModified = $false

ForEach ($example in $gorgonExamples)
{
    # Only update the settings file once, every app shares the same file.
    if ($example.FullName.EndsWith("appsettings.json"))
    {
        if ($appsettingModified -eq $false)
        {            
            Update-Config -configFile $example.FullName
            $appsettingModified = $true        
        }
        else
        {
            continue
        }
    }
    
    # Don't waste time on files we already have.
    if (Test-Path -Path "$artifactBin\\$($example.Name)")
    {
        continue
    }

    Copy-Item $example.FullName $artifactBin
}

ForEach ($plugInDll in $plugInDlls)
{
    Copy-Item $plugInDll.FullName $artifactPlugIns
}