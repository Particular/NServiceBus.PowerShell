namespace NServiceBus.PowerShell
{
    using System.Management.Automation;
    using System.Security;
    

    public abstract class CmdletBase : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            var processUtil = new ProcessUtil(Host);
            if (!processUtil.IsRunningWithElevatedPrivileges())
            {
                var exception = new SecurityException("This command requires elevated privileges");
                ThrowTerminatingError(new ErrorRecord(exception, null, ErrorCategory.PermissionDenied, null));
            }
        }
    }
}