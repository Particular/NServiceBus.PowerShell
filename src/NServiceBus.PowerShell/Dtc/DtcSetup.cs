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
        public void StartDtcIfNecessary(string PortRange = null)
        {
            var processUtil = new ProcessUtil(Host);

            if (DoesSecurityConfigurationRequireRestart(true, PortRange))
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

        bool DoesSecurityConfigurationRequireRestart(bool doChanges, string PortRange = null)
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


            if (!StringExtensions.IsNullOrWhiteSpace(PortRange))
            {
                const string rpcKeyName = @"SOFTWARE\Microsoft\Rpc\Internet";

                if (!hklm.KeyExists(rpcKeyName))
                {
                    if (doChanges)
                    {
                        hklm.CreateSubkey(rpcKeyName);
                        WriteWarning("RPC Port configuration was fixed.");
                    }
                    requireRestart = true;
                }

                foreach (var val in RpcRegValues)
                {
                    if ((string) hklm.ReadValue(rpcKeyName, val, "N", true) == "Y")
                    {
                        continue;
                    }

                    if (doChanges)
                    {
                        WriteWarning("RPC Ports not configured correctly. Going to fix. This will require a restart of the DTC service.");
                        if (!hklm.WriteValue(rpcKeyName, val, "Y", RegistryValueKind.String))
                        {
                            throw new Exception(string.Format("Failed to set value '{0}' to '{1}' in '{2}'", val, "Y", rpcKeyName));
                        }
                        WriteWarning("RPC Port configuration was fixed.");
                    }
                    requireRestart = true;
                }

                const string RpcPortsKey = "Ports";
                string[] RpcPortsArray =
                {
                    PortRange
                };

                if (Array.IndexOf((string[]) hklm.ReadValue(rpcKeyName, RpcPortsKey, new string[]{}, true), PortRange) >= 0)
                {
                    return requireRestart;
                }

                if (doChanges)
                {
                    WriteWarning("RPC Ports not configured correctly. Going to fix. This will require a restart of the DTC service.");
                    if (!hklm.WriteValue(rpcKeyName, RpcPortsKey, RpcPortsArray, RegistryValueKind.MultiString))
                    {
                        throw new Exception(string.Format("Failed to set value '{0}' to '{1}' in '{2}'", RpcPortsKey, "Y", rpcKeyName));
                    }
                    WriteWarning("RPC Port configuration was fixed.");
                }
                requireRestart = true;
            }

            return requireRestart;
        }

        static readonly ServiceController Controller = new ServiceController {ServiceName = "MSDTC", MachineName = "."};
        static readonly List<string> RegValues = new List<string>(new[] { "NetworkDtcAccess", "NetworkDtcAccessClients", "NetworkDtcAccessInbound", "NetworkDtcAccessOutbound", "NetworkDtcAccessTransactions", "XaTransactions" });
        static readonly List<string> RpcRegValues = new List<string>(new[] { "PortsInternetAvailable", "UseInternetPorts" });
    }
}