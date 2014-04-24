namespace NServiceBus.PowerShell
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Security;
    using Microsoft.Win32;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusLicense")]
    public class InstallLicense : PSCmdlet
    {
        internal const string LicenseRegistryKeyName = @"SOFTWARE\ParticularSoftware\NServiceBus";

        [Parameter(Mandatory = true, Position = 0, HelpMessage = "License file path", ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(Mandatory = false, HelpMessage = @"Installs license in HKEY_CURRENT_USER\SOFTWARE\ParticularSoftware\NServiceBus, by default if not specified the license is installed in HKEY_LOCAL_MACHINE\SOFTWARE\ParticularSoftware\NServiceBus")]
        public bool UseHKCU { get; set; }

        protected override void ProcessRecord()
        {
            var selectedLicenseText = ReadAllTextWithoutLocking(Path);

            if (Environment.Is64BitOperatingSystem)
            {
                TryToWriteToRegistry(selectedLicenseText, RegistryView.Registry32);
                
                TryToWriteToRegistry(selectedLicenseText, RegistryView.Registry64);
            }
            else
            {
                TryToWriteToRegistry(selectedLicenseText, RegistryView.Default);
            }
        }

        void TryToWriteToRegistry(string selectedLicenseText, RegistryView view)
        {
            var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);

            if (UseHKCU)
            {
                rootKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
            }

            using (var registryKey = rootKey.CreateSubKey(LicenseRegistryKeyName))
            {
                if (registryKey == null)
                {
                    ThrowTerminatingError(new ErrorRecord(new SecurityException("License file could not be installed."), "NotAuthorized", ErrorCategory.SecurityError, null));
                }

                registryKey.SetValue("License", selectedLicenseText, RegistryValueKind.String);
                var installedLicense = GetNServiceBusLicense.GetLicenseStoredInRegistry(view, registryKey);
                WriteObject(installedLicense);
            }
        }

        static string ReadAllTextWithoutLocking(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                return textReader.ReadToEnd();
            }
        }
    }
}