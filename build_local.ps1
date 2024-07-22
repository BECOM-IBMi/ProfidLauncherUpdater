.\build.ps1 --configuration "Release"

if ($LASTEXITCODE -ne 0)
{
	exit $LASTEXITCODE
}

rm .\installer\*

.\update_aip.ps1

if ($LASTEXITCODE -ne 0)
{
	exit $LASTEXITCODE
}

echo "Hello"