namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using Helpers;
    

    [Cmdlet(VerbsCommon.Get, "NServiceBusLocalMachineSettings")]
    public class GetNServiceBusLocalMachineSettings : CmdletBase
    {
        StringDictionary regResults = new StringDictionary();

        const string audit64 = "AuditQueue (64Bit Registry)";
        const string error64 = "ErrorQueue (64Bit Registry)";
        const string audit32 = "AuditQueue (32Bit Registry)";
        const string error32 = "ErrorQueue (32Bit Registry)";

        protected override void ProcessRecord()
        {
            const string key = @"SOFTWARE\ParticularSoftware\ServiceBus";

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                var key64Exists = (RegistryHelper.LocalMachine(RegistryView.Registry64).KeyExists(key));
                regResults.Add(audit64, key64Exists
                    ? (string) RegistryHelper.LocalMachine(RegistryView.Registry64).ReadValue(key, "AuditQueue", null, false)
                    : null);
                regResults.Add(error64, key64Exists
                    ? (string) RegistryHelper.LocalMachine(RegistryView.Registry64).ReadValue(key, "ErrorQueue", null, false)
                    : null);
            }

            var key32Exists = (RegistryHelper.LocalMachine(RegistryView.Registry32).KeyExists(key));
            regResults.Add(audit32, key32Exists
                ? (string) RegistryHelper.LocalMachine(RegistryView.Registry32).ReadValue(key, "AuditQueue", null, false)
                : null);
            regResults.Add(error32, key32Exists
                ? (string) RegistryHelper.LocalMachine(RegistryView.Registry32).ReadValue(key, "ErrorQueue", null, false)
                : null);

            if (regResults.ContainsKey(audit64))
            {
                {
                    var a1 = regResults[audit64];
                    var a2 = regResults[audit32];
                    if (!string.Equals(a1, a2))
                    {
                        WriteWarning("AuditQueue value is different for 32 bit and 64 bit applications");
                    }

                    var e1 = regResults[error64];
                    var e2 = regResults[error32];
                    if (!string.Equals(e1, e2))
                    {
                        WriteWarning("ErrorQueue value is different for 32 bit and 64 bit applications");
                    }
                }
            }

            var psObj = new PSObject();
            var keys = new string[regResults.Count];
            regResults.Keys.CopyTo(keys, 0);
            Array.Sort(keys);


            foreach (string name in keys)
            {
                psObj.Properties.Add(new PSNoteProperty(name, regResults[name]));
            }

            WriteObject(psObj);
        }
    }
}