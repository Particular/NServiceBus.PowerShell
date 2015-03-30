namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using NServiceBus.PowerShell.Cmdlets;

    [Cmdlet(VerbsDiagnostic.Test, "NServiceBusMSMQInstallation")]
    public class ValidateMsmq : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var msmqIsGood = new MsmqSetup(Host).IsInstallationGood();

            var status = msmqIsGood
                ? "MSMQ is installed and setup for use with NServiceBus."
                : "MSMQ is not installed.";

            WriteObject(new InstallationResult { Installed = msmqIsGood, Message = status });
        }
    }
}