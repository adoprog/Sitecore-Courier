$name = "sitecore-courier"
$url = "https://github.com/adoprog/Sitecore-Courier/releases/download/1%2C2%2C2/Sitecore.Courier.Runner.zip"
$unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
Write-Output "Unzip location: $unzipLocation"

Install-ChocolateyZipPackage $name $url $unzipLocation
 
$srm = (Join-Path $(Split-Path -parent $unzipLocation) 'tools\srm.exe')
$ext =  (Join-Path $(Split-Path -parent $unzipLocation) 'tools\Sitecore.Courier.ShellExtensions.dll')
Write-Output $srm
Write-Output $ext
& $srm install $ext -codebase
 