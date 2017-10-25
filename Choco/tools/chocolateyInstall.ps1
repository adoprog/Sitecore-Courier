$srm = (Join-Path $(Split-Path -parent $MyInvocation.MyCommand.Definition) 'srm.exe')
Write-Output $MyInvocation.MyCommand.Definition
$ext =  (Join-Path $(Split-Path -parent $MyInvocation.MyCommand.Definition) 'Sitecore.Courier.ShellExtensions.dll')
Write-Output $ext
& $srm install $ext -codebase