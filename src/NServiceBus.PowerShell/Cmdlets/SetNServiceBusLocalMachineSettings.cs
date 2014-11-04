namespace NServiceBus.PowerShell
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using Helpers;
    using Microsoft.Win32;
    using RegistryView = Helpers.RegistryView;

    [Cmdlet(VerbsCommon.Set, "NServiceBusLocalMachineSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class SetNServiceBusLocalMachineSettings : PSCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "Error queue to use for all endpoints in this machine.")]
        public string ErrorQueue { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Audit queue to use for all endpoints in this machine.")]
        public string AuditQueue { get; set; }

        protected override void ProcessRecord()
        {
            if (!ShouldProcess(Environment.MachineName))
            {
                return;
            }

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                WriteRegistry(RegistryView.Registry32);
                WriteRegistry(RegistryView.Registry64);
            }
            else
            {
                WriteRegistry(RegistryView.Default);
            }
        }

        void WriteRegistry(RegistryView view)
        {
            var hklm = RegistryHelper.LocalMachine(view);
            const string key = @"SOFTWARE\ParticularSoftware\ServiceBus";
            if (!StringExtensions.IsNullOrWhiteSpace(ErrorQueue))
            {
                hklm.WriteValue(key,"ErrorQueue", ErrorQueue, RegistryValueKind.String);
            }
            if (!StringExtensions.IsNullOrWhiteSpace(AuditQueue))
            {
                hklm.WriteValue(key,"AuditQueue", AuditQueue, RegistryValueKind.String);
            }
        }
    }
}