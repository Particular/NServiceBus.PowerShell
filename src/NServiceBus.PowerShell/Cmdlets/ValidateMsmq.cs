namespace NServiceBus.PowerShell
{
    using System.Management.Automation;

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusMSMQInstallation")]
    public class ValidateMsmq : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var msmqIsGood = new MsmqSetup(Host).IsInstallationGood();

            var status = msmqIsGood
                ? "MSMQ is installed and setup for use with NServiceBus."
                : "MSMQ is not installed.";

            var p = new PSObject();
            p.Properties.Add(new PSNoteProperty("Message", status));
            p.Properties.Add(new PSNoteProperty("Installed", msmqIsGood));
            WriteObject(p);
        }
    }
}