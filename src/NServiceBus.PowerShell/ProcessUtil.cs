namespace NServiceBus.PowerShell
{
    using System;
    using System.ComponentModel;
    using System.Management.Automation.Host;
    using System.Security.Principal;
    using System.ServiceProcess;
    
    /// <summary>
    /// Utility class for changing a windows service's status.
    /// </summary>
    public class ProcessUtil : CmdletHelperBase
    {
        public ProcessUtil(PSHost Host) : base(Host)
        {

        }

        public bool IsRunningWithElevatedPrivileges()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Checks the status of the given controller, and if it isn't the requested state,
        /// performs the given action, and checks the state again.
        /// </summary>
        public  void ChangeServiceStatus(ServiceController controller, ServiceControllerStatus status, Action changeStatus)
        {
            if (controller.Status == status)
            {
                WriteLine(controller.ServiceName + " status is good: " + Enum.GetName(typeof(ServiceControllerStatus), status));
                return;
            }

            WriteLine((controller.ServiceName + " status is NOT " + Enum.GetName(typeof(ServiceControllerStatus), status) + ". Changing status..."));

            try
            {
                changeStatus();
            }
            catch (Win32Exception exception)
            {
                ThrowUnableToChangeStatus(controller.ServiceName, status, exception);
            }
            catch (InvalidOperationException exception)
            {
                ThrowUnableToChangeStatus(controller.ServiceName, status, exception);
            }

            var timeout = TimeSpan.FromSeconds(10);
            controller.WaitForStatus(status, timeout);
            if (controller.Status == status)
                WriteLine((controller.ServiceName + " status changed successfully."));
            else
                ThrowUnableToChangeStatus(controller.ServiceName, status);
        }

        private void ThrowUnableToChangeStatus(string serviceName, ServiceControllerStatus status)
        {
            ThrowUnableToChangeStatus(serviceName, status, null);
        }

        private static void ThrowUnableToChangeStatus(string serviceName, ServiceControllerStatus status, Exception exception)
        {
            var message = "Unable to change " + serviceName + " status to " + Enum.GetName(typeof(ServiceControllerStatus), status);

            if (exception == null)
            {
                throw new InvalidOperationException(message);
            }

            throw new InvalidOperationException(message, exception);
        }
    }
}
