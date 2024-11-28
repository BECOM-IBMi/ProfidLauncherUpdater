$assets = (get-location).path + "\assets"
$outpf = (get-location).path + "\artifacts"
$package = (get-location).path + "\installer"

$aipBase = $assets + "\ProfidLauncherBase.aip"
$aip = $assets + "\ProfidLauncher.aip"
$icon = $assets + "\favicon.ico"

cp $aipBase $aip

$advinst = New-Object -ComObject AdvancedInstaller

$project = $advinst.LoadProject($aip)

$ver = $args[0]
echo $ver

$project.ProductDetails.Version = $ver
$project.ProductDetails.SetIcon($icon)

$pl = $project.FilesComponent.FindFileByPath("APPDIR\ProfidLauncherUpdater.exe")

$sc = $project.ShortcutsComponent.CreateFileShortcut($project.PredefinedFolders.Desktop, $pl)
$sc.Name = "Profid Launcher"
$sc.Icon($icon)
$sc.Arguments = "run ATRIUMP"

$sc = $project.ShortcutsComponent.CreateFileShortcut($project.PredefinedFolders.ShortcutFolder, $pl)
$sc.Icon($icon)
$sc.Name = "Profid Launcher"
$sc.Arguments = "run ATRIUMP"

$project.BuildComponent.Builds[0].OutputFolder = $package

$project.Build()
