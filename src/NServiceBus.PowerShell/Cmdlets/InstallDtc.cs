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
}
