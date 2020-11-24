@{
	GUID = 'ABFF92C4-A8BA-4CAA-A40D-8C97F3B28934'
	Author = 'Particular Software'
	Description = 'NServiceBus PowerShell'
	ModuleVersion = '{{Version}}'
	NestedModules = 'NServiceBus.PowerShell.dll' 
	CLRVersion = '4.0'
	DotNetFrameworkVersion = '4.5.2'
	CompanyName = 'Particular Software'
	Copyright = '(c) 2020 NServiceBus Ltd. All rights reserved.'
	FunctionsToExport='*'
	CmdletsToExport = '*'
	VariablesToExport = '*'
	AliasesToExport = '*'
	FormatsToProcess = @(
    '.\Formats\NServiceBus.Powershell.Cmdlets.InstallationResult.format.ps1xml',
    '.\Formats\NServiceBus.Powershell.Cmdlets.MachineSettingsResult.format.ps1xml'
	)
}
