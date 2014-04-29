<#
    Call this via 

    Powershell.exe -V 2 -NoLogo -NoExit -File Init.PS1 

#>

if ($PSVersionTable.PSVersion.Major -lt 3) {
    $PSScriptRoot = Split-Path  $MyInvocation.MyCommand.Path
}

cd $PSScriptRoot 
ipmo ..\binaries\NServiceBus.Powershell.dll -verbose
