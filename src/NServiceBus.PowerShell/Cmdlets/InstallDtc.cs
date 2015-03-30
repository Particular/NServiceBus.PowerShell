namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using Helpers;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusDTC", SupportsShouldProcess = true)]
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
