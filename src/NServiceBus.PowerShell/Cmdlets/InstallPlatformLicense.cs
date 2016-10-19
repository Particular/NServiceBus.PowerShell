namespace NServiceBus.PowerShell.Cmdlets
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.PowerShell.Commands;
    using Microsoft.Win32;
    using Helpers;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusPlatformLicense")]
    public class InstallPlatformLicense : CmdletBase
    {
        [Parameter(Mandatory = false, HelpMessage = "Platform license file to import", Position = 0)]
        public string LicenseFile { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Platform license string to import", Position = 1)]
        public string LicenseString { get; set; }

        protected override void ProcessRecord()
        {
            const string particular = @"Software\ParticularSoftware";
            string content;

            // LicenseFile primary option
            if(!string.IsNullOrEmpty(LicenseFile))
            {
                ProviderInfo provider;
                PSDriveInfo drive;
                var psPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(LicenseFile, out provider, out drive);
                

                if (provider.ImplementingType != typeof(FileSystemProvider))
                {
                    var ex = new ArgumentException(string.Format("{0} does not resolve to a path on the FileSystem provider.", psPath));
                    var error = new ErrorRecord(ex, "InvalidProvider", ErrorCategory.InvalidArgument, psPath);
                    WriteError(error);
                    return;
                }

                content = File.ReadAllText(psPath);
                if (!CheckFileContentIsALicenseFile(content))
                {
                    var ex = new InvalidDataException(string.Format("{0} is not a valid license file", psPath));
                    var error = new ErrorRecord(ex, "InvalidLicense", ErrorCategory.InvalidData, psPath);
                    WriteError(error);
                    return;
                }
            }
            // LicenseString secondary option
            else if (!string.IsNullOrEmpty(LicenseString))
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
            // No valid parameter supplied
            else
            {
                var ex = new ArgumentException("LicenseFile or LicenseString must be provided.");
                var error = new ErrorRecord(ex, "InvalidParameter", ErrorCategory.InvalidArgument, null);
                WriteError(error);
                return;
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
