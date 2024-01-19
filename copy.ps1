[CmdletBinding()]
Param(
	[Parameter(Mandatory=$true)]
    [String] $target
)

$package = (get-location).path + "\package"

cp $package\* $target