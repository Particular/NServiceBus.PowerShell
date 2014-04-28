namespace NServiceBus.Setup.Windows.Dtc
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using Microsoft.Win32;
    using PowerShell.Helpers;

    public class DtcSetup
    {
        /// <summary>
        ///     Checks that the MSDTC service is running and configured correctly, and if not
        ///     takes the necessary corrective actions to make it so.
        /// </summary>
        public static void StartDtcIfNecessary()
        {
            if (DoesSecurityConfigurationRequireRestart(true))
            {
                ProcessUtil.ChangeServiceStatus(Controller, ServiceControllerStatus.Stopped, Controller.Stop);
            }

            ProcessUtil.ChangeServiceStatus(Controller, ServiceControllerStatus.Running, Controller.Start);
        }

        public static bool IsDtcWorking()
        {
            if (DoesSecurityConfigurationRequireRestart(false))
            {
                return false;
            }

            if (Controller.Status != ServiceControllerStatus.Running)
            {
                Console.Out.WriteLine("MSDTC isn't currently running and needs to be started");
                return false;
            }

            return true;
        }

        static bool DoesSecurityConfigurationRequireRestart(bool doChanges)
        {
            Console.WriteLine("Checking if DTC is configured correctly.");

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
                    Console.WriteLine("DTC not configured correctly. Going to fix. This will require a restart of the DTC service.");
                    if (!hklm.WriteValue(keyName, val, 1, RegistryValueKind.DWord))
                    {
                        throw new Exception(string.Format("Failed to set value '{0}' to '{1}' in '{2}'", val, 1, keyName));
                    }
                    Console.WriteLine("DTC configuration fixed.");
                }
                requireRestart = true;
            }
            return requireRestart;
        }



        static readonly ServiceController Controller = new ServiceController {ServiceName = "MSDTC", MachineName = "."};
        static readonly List<string> RegValues = new List<string>(new[] {"NetworkDtcAccess", "NetworkDtcAccessOutbound", "NetworkDtcAccessTransactions", "XaTransactions"});
    }
}