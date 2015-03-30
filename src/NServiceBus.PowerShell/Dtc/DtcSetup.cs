namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Host;
    using System.ServiceProcess;
    using Microsoft.Win32;
    using Helpers;
    using RegistryView = Helpers.RegistryView;


    public class DtcSetup : CmdletHelperBase
    {
        public DtcSetup()
        {

        }

        public DtcSetup(PSHost Host) : base(Host)
        {
            
        }

        /// <summary>
        ///     Checks that the MSDTC service is running and configured correctly, and if not
        ///     takes the necessary corrective actions to make it so.
        /// </summary>
        public void StartDtcIfNecessary()
        {
            var processUtil = new ProcessUtil(Host);

            if (DoesSecurityConfigurationRequireRestart(true))
            {
                processUtil.ChangeServiceStatus(Controller, ServiceControllerStatus.Stopped, Controller.Stop);
            }

            processUtil.ChangeServiceStatus(Controller, ServiceControllerStatus.Running, Controller.Start);
        }

        public  bool IsDtcWorking()
        {
         
            if (DoesSecurityConfigurationRequireRestart(false))
            {
                return false;
            }

            if (Controller.Status != ServiceControllerStatus.Running)
            {
                WriteWarning("MSDTC isn't currently running and needs to be started");
                return false;
            }

            return true;
        }

        bool DoesSecurityConfigurationRequireRestart(bool doChanges)
        {
            var regview = EnvironmentHelper.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
            var hklm = RegistryHelper.LocalMachine(regview);

            const string keyName = @"SOFTWARE\Microsoft\MSDTC\Security";
            var requireRestart = false;
            foreach (var val in RegValues)
            {
                if ((int)hklm.ReadValue(keyName, val, 0, true) != 0)
                {
                    continue;
                }

                if (doChanges)
                {
                    WriteWarning("DTC not configured correctly. Going to fix. This will require a restart of the DTC service.");
                    if (!hklm.WriteValue(keyName, val, 1, RegistryValueKind.DWord))
                    {
                        throw new Exception(string.Format("Failed to set value '{0}' to '{1}' in '{2}'", val, 1, keyName));
                    }
                    WriteWarning("DTC configuration was fixed.");
                }
                requireRestart = true;
            }
            return requireRestart;
        }

        static readonly ServiceController Controller = new ServiceController {ServiceName = "MSDTC", MachineName = "."};
        static readonly List<string> RegValues = new List<string>(new[] {"NetworkDtcAccess", "NetworkDtcAccessOutbound", "NetworkDtcAccessTransactions", "XaTransactions"});
    }
}