namespace NServiceBus.PowerShell
{
    using System.Management.Automation;

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusDTCInstallation")]
    public class ValidateDtc : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var dtcIsGood =  new DtcSetup(Host).IsDtcWorking();
            var status = dtcIsGood
                ? "DTC is setup and ready for use with NServiceBus."
                : "DTC is not properly configured.";

            var p = new PSObject();
            p.Properties.Add(new PSNoteProperty("Message", status));
            p.Properties.Add(new PSNoteProperty("Installed", dtcIsGood));
            WriteObject(p);
        }
    }
}