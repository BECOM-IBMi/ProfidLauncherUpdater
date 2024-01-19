$assets = (get-location).path + "\assets"
$outpf = (get-location).path + "\artifacts"
$package = (get-location).path + "\installer"

$aip = $assets + "\ProfidLauncher.aip"
$icon = $assets + "\favicon.ico"

$advinst = New-Object -ComObject AdvancedInstaller

$project = $advinst.LoadProject($aip)

$tag = git describe --abbrev=0
$tag = $tag.Replace('v', '')
$tag = $tag.Substring(0, $tag.LastIndexOf('.') + 1)

if($ENV:BUILD_NUMBER) {
    $tag = $tag + $ENV:BUILD_NUMBER
} else {
    $tag = $tag + "99"
}

$project.ProductDetails.Version = $tag
$project.ProductDetails.SetIcon($icon)

$pl = $project.FilesComponent.FindFileByPath("APPDIR\ProfidLauncherUpdater.exe")

$sc = $project.ShortcutsComponent.CreateFileShortcut($project.PredefinedFolders.Desktop, $pl)
$sc.Icon($icon)

$sc = $project.ShortcutsComponent.CreateFileShortcut($project.PredefinedFolders.ShortcutFolder, $pl)
$sc.Icon($icon)

$project.BuildComponent.Builds[0].OutputFolder = $package

$project.SaveAs($aip)

$project.Build()