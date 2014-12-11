namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using Helpers;
    using NServiceBus.PowerShell.Cmdlets;


    [Cmdlet(VerbsCommon.Get, "NServiceBusLocalMachineSettings")]
    public class GetNServiceBusLocalMachineSettings : CmdletBase
    {
        List<MachineSettingsResult> results = new List<MachineSettingsResult>();

        protected override void ProcessRecord()
        {
            const string key = @"SOFTWARE\ParticularSoftware\ServiceBus";

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                var key64Exists = (RegistryHelper.LocalMachine(RegistryView.Registry64).KeyExists(key));
                var result64 = new MachineSettingsResult
                {
                    Registry = "64 Bit",
                    AuditQueue = key64Exists ? (string) RegistryHelper.LocalMachine(RegistryView.Registry64).ReadValue(key, "AuditQueue", null, false) : null,
                    ErrorQueue = key64Exists ? (string) RegistryHelper.LocalMachine(RegistryView.Registry64).ReadValue(key, "ErrorQueue", null, false) : null
                };
                results.Add(result64);
            }

            var key32Exists = (RegistryHelper.LocalMachine(RegistryView.Registry32).KeyExists(key));
            var result32 = new MachineSettingsResult
            {
                Registry = "32 Bit",
                AuditQueue = key32Exists ? (string)RegistryHelper.LocalMachine(RegistryView.Registry32).ReadValue(key, "AuditQueue", null, false) : null,
                ErrorQueue = key32Exists ? (string)RegistryHelper.LocalMachine(RegistryView.Registry32).ReadValue(key, "ErrorQueue", null, false) : null
            };
            results.Add(result32);

            if (results.Count == 2)
            {
                if (string.Compare(results[0].AuditQueue, results[1].AuditQueue, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    WriteWarning("AuditQueue value is different for 32 bit and 64 bit applications");
                }

                if (string.Compare(results[0].ErrorQueue, results[1].ErrorQueue, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    WriteWarning("ErrorQueue value is different for 32 bit and 64 bit applications");
                }

            }

            foreach (var result in results)
            {
                WriteObject(result);    
            }
        }
    }
}