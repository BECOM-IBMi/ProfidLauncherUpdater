[CmdletBinding()]
Param(
	[Parameter(Mandatory=$true)]
    [String] $target
)

$package = (get-location).path + "\installer"

cp $package\* $target