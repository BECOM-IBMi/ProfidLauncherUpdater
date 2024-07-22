$ver = dotnet-gitversion /output json /showvariable SemVer

#Version anlegen
$json = '{"softwareId":"f6cf4f14-4c16-4413-9cc9-dc012f46b7ba","version":"' + $ver + '"}'

$resp = curl --header "Content-Type: application/json" --request POST --data $json https://localhost:7112/api/software/version
echo $resp
$json = ConvertFrom-Json $resp

if ($LASTEXITCODE -ne 0)
{
	exit $LASTEXITCODE
}

$url = 'https://localhost:7112/api/software/version/upload/' + $json.id

#Zip file

compress-archive .\installer\ProfidLauncherSetup.msi .\installer\ProfidLauncherSetup.zip -Force

$path = (Get-ChildItem .\installer\ProfidLauncherSetup.zip).FullName
$curlFile = 'file=@' +  $path

#File hochladen
curl -X PATCH -F "$curlFile" $url