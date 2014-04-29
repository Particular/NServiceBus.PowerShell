namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
   

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusPerformanceCountersInstallation")]
    public class ValidatePerformanceCounters : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var countersAreGood = new PerformanceCounterSetup(Host).CheckCounters();

            WriteVerbose(countersAreGood
                ? "NServiceBus Performance Counters are setup and ready for use with NServiceBus."
                : "NServiceBus Performance Counters are not properly configured.");

            WriteObject(countersAreGood);
        }
    }
}