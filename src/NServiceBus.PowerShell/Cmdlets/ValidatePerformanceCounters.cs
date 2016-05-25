namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using Cmdlets;

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusPerformanceCountersInstallation")]
    public class ValidatePerformanceCounters : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var countersAreGood = new PerformanceCounterSetup(Host).CheckCounters();

            var status = countersAreGood
                ? "NServiceBus Performance Counters are setup and ready for use with NServiceBus."
                : "NServiceBus Performance Counters are not properly configured.";

            WriteObject(new InstallationResult { Installed = countersAreGood, Message = status });
        }
    }
}