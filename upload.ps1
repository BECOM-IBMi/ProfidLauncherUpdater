$ver = dotnet-gitversion /output json /showvariable SemVer

#Version anlegen
$json = '{"softwareId":"50aa3fc9-4396-4624-9235-282e68ad1062","version":"' + $ver + '"}'

$resp = curl --header "Content-Type: application/json" --request POST --data $json http://srepo.becom.at/api/software/version
#$resp = curl --header "Content-Type: application/json" --request POST --data $json http://001-itsv-docke1:8111/api/software/version
echo $resp
$json = ConvertFrom-Json $resp

if ($LASTEXITCODE -ne 0)
{
	exit $LASTEXITCODE
}

$url = 'http://srepo.becom.at/api/software/version/upload/' + $json.id
#$url = 'http://001-itsv-docke1:8111/api/software/version/upload/' + $json.id

#Zip file

compress-archive .\installer\ProfidLauncherSetup.msi .\installer\ProfidLauncherSetup.zip -Force

$path = (Get-ChildItem .\installer\ProfidLauncherSetup.zip).FullName
$curlFile = 'file=@' +  $path

#File hochladen
curl -X PATCH -F "$curlFile" $url