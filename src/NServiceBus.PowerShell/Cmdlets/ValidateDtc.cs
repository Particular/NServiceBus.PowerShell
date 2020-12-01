﻿namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using Cmdlets;

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusDTCInstallation")]
    public class ValidateDtc : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var dtcIsGood = new DtcSetup(Host).IsDtcWorking();
            var status = dtcIsGood
                ? "DTC is setup and ready for use with NServiceBus."
                : "DTC is not properly configured.";

            WriteObject(new InstallationResult { Installed = dtcIsGood, Message = status });
        }
    }
}