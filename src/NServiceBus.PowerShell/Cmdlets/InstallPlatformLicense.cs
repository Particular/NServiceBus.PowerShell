﻿namespace NServiceBus.PowerShell.Cmdlets
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Helpers;
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusPlatformLicense", DefaultParameterSetName = "ByLicenseFile")]
    public class InstallPlatformLicense : CmdletBase
    {
        [Parameter(Mandatory = true, HelpMessage = "Platform license file to import", Position = 0, ParameterSetName = "ByLicenseFile")]
        [ValidateNotNullOrEmpty]
        public string LicenseFile { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Platform license string to import", Position = 0, ParameterSetName = "ByLicenseString")]
        [ValidateNotNullOrEmpty]
        public string LicenseString { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Reading license information from the registry has been deprecated and will be removed in version 8.0. See the documentation for more details.  Use switch -OverrideObsolete to install license in the registry anyway", Position = 1)]
        public SwitchParameter OverrideObsolete { get; set; }

        protected override void ProcessRecord()
        {
            const string particular = @"Software\ParticularSoftware";l
            string content;

            // LicenseFile primary option
            if (ParameterSetName.Equals("ByLicenseFile"))
            {
                ProviderInfo provider;
                PSDriveInfo drive;
                var psPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(LicenseFile, out provider, out drive);

                if (!OverrideObsolete)
                {
                    // warn of impending removal of this feature - save the user from going
                    // and removing license information manually from the registry to after installing to suppress
                    // warnings on endpoints.
                    var ex = new ArgumentException("Reading license information from the registry has been deprecated and will be removed in version 8.0. See the documentation for more details.  Use switch -OverrideObsolete to install license in the registry anyway.");
                    var error = new ErrorRecord(ex, "ObsoleteFeature", ErrorCategory.InvalidArgument, null);
                    WriteError(error);

                    return;
                }

                if (provider.ImplementingType != typeof(FileSystemProvider))
                {
                    var ex = new ArgumentException($"{psPath} does not resolve to a path on the FileSystem provider.");
                    var error = new ErrorRecord(ex, "InvalidProvider", ErrorCategory.InvalidArgument, psPath);
                    WriteError(error);
                    return;
                }

                content = File.ReadAllText(psPath);
                if (!CheckFileContentIsALicenseFile(content))
                {
                    var ex = new InvalidDataException($"{psPath} is not a valid license file");
                    var error = new ErrorRecord(ex, "InvalidLicense", ErrorCategory.InvalidData, psPath);
                    WriteError(error);
                    return;
                }
            }
            // LicenseString secondary option
            else
            {
                content = LicenseString;
                if (!CheckFileContentIsALicenseFile(content))
                {
                    var ex = new InvalidDataException("The supplied LicenseString is not a valid license file");
                    var error = new ErrorRecord(ex, "InvalidLicense", ErrorCategory.InvalidData, null);
                    WriteError(error);
                    return;
                }
            }

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                RegistryHelper.LocalMachine(RegistryView.Registry64).WriteValue(particular, "License", content, RegistryValueKind.String);
            }

            RegistryHelper.LocalMachine(RegistryView.Registry32).WriteValue(particular, "License", content, RegistryValueKind.String);
        }

        bool CheckFileContentIsALicenseFile(string content)
        {
            return (content.Contains("<license") && content.Contains("<Signature"));
        }
    }
}