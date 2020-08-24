function Patch-Config()
{
    Param ([string]$configFile)

    [xml]$xmlFile = [xml](Get-Content $configFile)

    $plugInPathNode = $xmlFile.SelectSingleNode("//setting[@name='PlugInLocation']")

    while (($plugInPathNode -ne $null) -and ($plugInPathNode.Value.Contains("\\")))
    {
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("\\", "\")
    }

    if ($plugInPathNode -ne $null)
    {
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("\\", "\")
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("..\", [string]::Empty)
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("Bin\{0}", [string]::Empty)
        $plugInPathNode.Value = "..\{0}" -f $plugInPathNode.Value
    }

    $plugInPathNode = $xmlFile.SelectSingleNode("//setting[@name='InputPlugInPath']")

    if ($plugInPathNode -ne $null)
    {
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("..\", [string]::Empty)
        $plugInPathNode.Value = $plugInPathNode.Value.Replace("Bin\{0}", [string]::Empty)
        $plugInPathNode.Value = "..\{0}" -f $plugInPathNode.Value
    }

    $resourcePathNode = $xmlFile.SelectSingleNode("//setting[@name='ResourceLocation']")

    if ($resourcePathNode -ne $null)
    {
        $resourcePathNode.Value = $resourcePathNode.Value.Replace("..\", [string]::Empty)
        $resourcePathNode.Value = "..\{0}" -f $resourcePathNode.Value
    }

    $xmlFile.Save($configFile)
}

$artifactDir = "$env:BUILD_ARTIFACTSTAGINGDIRECTORY\\Examples"
$artifactBin = "$artifactDir\\Bin\\"
$artifactPlugIns = "$artifactDir\\PlugIns\\"
$artifactResources = "$artifactDir\\Resources\\"
$artifactImagesFolder = "$artifactBin\\Images\\"
$baseSrcResources = "Resources\\"
$baseSrcImages = "Examples\\Gorgon.Graphics\\Images\\Images\\*.*"

if (Test-Path $artifactDir)
{
    rmdir $artifactDir -force -Recurse
}

mkdir $artifactDir
mkdir $artifactBin
mkdir $artifactImagesFolder
mkdir $artifactPlugIns

$gorgonExamples = (Get-ChildItem "Examples\\" -include *.dll,*.exe, *.config, TextViewerContentExample_Installation.txt  -Recurse | where { $_.FullName -notmatch "app.config" -and $_.FullName -notmatch ".vshost.exe" -and $_.FullName -notmatch "\\debug\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "_Test" })
$plugInDlls = (Get-ChildItem "PlugIns\\Bin\\" -include *.dll -Recurse | where { $_.FullName -notmatch "\\debug\\" })

Write-Output "$($gorgonExamples.Length) Example files to copy."
Write-Output "$($plugInDlls.Length) Plug in files to copy."

Copy-Item -Path $baseSrcResources -Exclude Krypton_DarkO2k10Theme.xml -Destination $artifactDir -Recurse -Container
Copy-Item -Path $baseSrcImages -Destination $artifactImagesFolder -Container: $false

ForEach ($example in $gorgonExamples)
{
    if ($example.FullName.EndsWith(".config"))
    {
        Patch-Config -configFile $example.FullName
    }

    Copy-Item $example.FullName $artifactBin
}

ForEach ($plugInDll in $plugInDlls)
{
    Copy-Item $plugInDll.FullName $artifactPlugIns
}