namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using Helpers;


    [Cmdlet(VerbsLifecycle.Install, "NServiceBusDTC", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InstallDtc : CmdletBase
    {
        protected override void ProcessRecord()
        {
            if (ShouldProcess(EnvironmentHelper.MachineName))
            {
                new DtcSetup(Host).StartDtcIfNecessary();
            }
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusDTCInstallation")]
    public class ValidateDtc : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var dtcIsGood =  new DtcSetup(Host).IsDtcWorking();
            WriteVerbose(dtcIsGood
                             ? "DTC is setup and ready for use with NServiceBus."
                             : "DTC is not properly configured.");

            WriteObject(dtcIsGood);
        }
    }
}
