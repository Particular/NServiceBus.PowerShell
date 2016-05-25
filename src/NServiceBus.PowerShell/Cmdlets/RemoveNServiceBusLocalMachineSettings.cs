namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using Helpers;

    [Cmdlet(VerbsCommon.Clear, "NServiceBusLocalMachineSettings")]
    public class RemoveNServiceBusLocalMachineSettings : CmdletBase
    {
        protected override void ProcessRecord()
        {
            const string key = @"SOFTWARE\ParticularSoftware\ServiceBus";

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                RegistryHelper.LocalMachine(RegistryView.Registry64).DeleteValue(key, "AuditQueue");
                RegistryHelper.LocalMachine(RegistryView.Registry64).DeleteValue(key, "ErrorQueue");
            }

            RegistryHelper.LocalMachine(RegistryView.Registry32).DeleteValue(key, "AuditQueue");
            RegistryHelper.LocalMachine(RegistryView.Registry32).DeleteValue(key, "ErrorQueue");
        }
    }
}