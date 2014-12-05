namespace NServiceBus.PowerShell
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Uninstall, "NServiceBusPerformanceCounters", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class UninstallPerformanceCounters : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var performanceCounterSetup = new PerformanceCounterSetup(Host);
            if (performanceCounterSetup.DoesCategoryExist())
            {
                performanceCounterSetup.DeleteCategory();
            }
            else
            {
                WriteWarning("NServiceBus Performance Counters were not installed");
            }
        }
    }
}
