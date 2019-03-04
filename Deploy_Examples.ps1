function Patch-Config()
{
    Param ([string]$configFile)

    [xml]$xmlFile = [xml](Get-Content $configFile)

    $plugInPathNode = $xmlFile.SelectSingleNode("//setting[@name='PlugInLocation']")

    if ($plugInPathNode -ne $null)
    {
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
$artifactResources = "$artifactDir\Resources\"


if (Test-Path $artifactDir)
{
    rmdir $artifactDir -force -Recurse
}

mkdir $artifactDir
mkdir $artifactBin
mkdir $artifactPlugIns
mkdir $artifactResources

$baseSrcResources = "Resources\"
$resourceList = (
    "Textures\Balls\BallsTexture.dds",
    "FileSystems\BZipFileSystem.gorPack",
    "FileSystems\FolderSystem\<<DIRECTORY>>",
    "FileSystems\VFSRoot\<<DIRECTORY>>",
    "FileSystems\VFSRoot.Zip",
    "FileSystems\FileSystem.Zip"
)

$gorgonExamples = (Get-ChildItem "Examples\\" -include *.dll,*.exe, *.config -Recurse | where { $_.FullName -notmatch "app.config" -and $_.FullName -notmatch ".vshost.exe" -and $_.FullName -notmatch "\\DemoLauncher\\" -and $_.FullName -notmatch "\\Primitives\\" -and $_.FullName -notmatch "\\debug\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\Fonts\\" -and $_.FullName -notmatch "_Test"})
$plugInDlls = (Get-ChildItem "PlugIns\\Bin\\" -include *.dll -Recurse | where { $_.FullName -notmatch "\\debug\\" -and $_.BaseName -notmatch "sharpdx"})
$ballTexture

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

ForEach ($resource in $resourceList)
{
    [string]$destDir = "$artifactResources{0}" -f (Split-Path $resource)

    if (-not (Test-Path $destDir))
    {
        mkdir $destDir
    }

    if ($resource.EndsWith("<<DIRECTORY>>"))
    {
        $srcDir = "$baseSrcResources{0}" -f (Split-Path $resource)
        Get-ChildItem -Path $srcDir | Copy-Item -Destination $destDir -Recurse -Container         
    } Else
    {
        Copy-Item $baseSrcResources$resource $destDir
    }
}