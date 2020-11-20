namespace NServiceBus.PowerShell
{
    using System.Management.Automation;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusMSMQ")]
    public class InstallMsmq : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var msmqIsGood = new MsmqSetup(Host).StartMsmqIfNecessary();

            if (!msmqIsGood)
            {
                WriteWarning("MSMQ may need to be reinstalled manually. Please ensure MSMQ is running properly.");
            }
        }
    }
}
