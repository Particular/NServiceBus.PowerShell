namespace NServiceBus.PowerShell
{
    using System.IO;
    using System.Management.Automation;
    using System.Security;
    using Helpers;
    using Microsoft.Win32;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusLicense")]
    public class InstallLicense : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "License file path", ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(Mandatory = false, HelpMessage = @"Installs license in HKEY_CURRENT_USER\SOFTWARE\ParticularSoftware\NServiceBus, by default if not specified the license is installed in HKEY_LOCAL_MACHINE\SOFTWARE\ParticularSoftware\NServiceBus")]
        public bool UseHKCU { get; set; }

        protected override void ProcessRecord()
        {
            var selectedLicenseText = ReadAllTextWithoutLocking(Path);

            if (EnvironmentHelper.Is64BitOperatingSystem)
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
            var rootKey = (UseHKCU) ? RegistryHelper.CurrentUser(view) :  RegistryHelper.LocalMachine(view);
            const string subkey = @"SOFTWARE\ParticularSoftware\NServiceBus";
            if (!rootKey.CreateSubkey(subkey))
            {
                ThrowTerminatingError(new ErrorRecord(new SecurityException("License file could not be installed."), "NotAuthorized", ErrorCategory.SecurityError, null));
            }
            rootKey.WriteValue(subkey,"License", selectedLicenseText, RegistryValueKind.String);
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