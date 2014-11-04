namespace NServiceBus.PowerShell
{
    using System;
    using System.Management.Automation;
    
    [Cmdlet(VerbsLifecycle.Install, "NServiceBusMSMQ", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class InstallMsmq : CmdletBase
    {
        protected override void ProcessRecord()
        {
            if (ShouldProcess(Environment.MachineName))
            {
                var msmqIsGood = new MsmqSetup(Host).StartMsmqIfNecessary();

                if (!msmqIsGood)
                {
                    WriteWarning("MSMQ may need to be reinstalled manually. Please ensure MSMQ is running properly.");
                }
            }
        }
    }
}
