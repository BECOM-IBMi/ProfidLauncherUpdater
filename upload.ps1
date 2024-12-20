$ver = dotnet-gitversion /output json /showvariable SemVer

#Version anlegen
$json = '{"softwareId":"d37682b0-ac74-49f2-a209-587fc79ed7ee","version":"' + $ver + '"}'

$resp = curl --header "Content-Type: application/json" --request POST --data $json http://srepo.becom.at/api/software/version
echo $resp
$json = ConvertFrom-Json $resp

if ($LASTEXITCODE -ne 0)
{
	exit $LASTEXITCODE
}

$url = 'http://srepo.becom.at/api/software/version/upload/' + $json.id

#Zip file

compress-archive .\installer\ProfidLauncherSetup.msi .\installer\ProfidLauncherSetup.zip -Force

$path = (Get-ChildItem .\installer\ProfidLauncherSetup.zip).FullName
$curlFile = 'file=@' +  $path

#File hochladen
curl -X PATCH -F "$curlFile" $url