@{
	GUID = 'ABFF92C4-A8BA-4CAA-A40D-8C97F3B28934'
	Author = 'Particular Software'
	Description = 'NServiceBus PowerShell'
	ModuleVersion = '{{Version}}'
	NestedModules = 'NServiceBus.PowerShell.dll' 
	CLRVersion = '2.0'
	DotNetFrameworkVersion = '2.0'
	CompanyName = 'Particular Software'
	Copyright = '(c) 2014 NServiceBus Ltd. All rights reserved.'
	FunctionsToExport='*'
	CmdletsToExport = '*'
	VariablesToExport = '*'
	AliasesToExport = '*'
	FormatsToProcess = @(
    '.\Formats\NServiceBus.Powershell.Cmdlets.InstallationResult.format.ps1xml',
    '.\Formats\NServiceBus.Powershell.Cmdlets.MachineSettingsResult.format.ps1xml'
	)

}
