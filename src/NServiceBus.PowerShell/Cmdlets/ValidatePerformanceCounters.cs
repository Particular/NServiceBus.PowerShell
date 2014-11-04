namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
   
    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusPerformanceCountersInstallation")]
    public class ValidatePerformanceCounters : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var countersAreGood = new PerformanceCounterSetup(Host).CheckCounters();

            var status = countersAreGood
                ? "NServiceBus Performance Counters are setup and ready for use with NServiceBus."
                : "NServiceBus Performance Counters are not properly configured.";

            var p = new PSObject();
            p.Properties.Add(new PSNoteProperty("Message", status));
            p.Properties.Add(new PSNoteProperty("Installed", countersAreGood));
            WriteObject(p);
        }
    }
}