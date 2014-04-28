namespace NServiceBus.PowerShell
{
    using System.Collections.Generic;
    using System.Net.NetworkInformation;

    public class PortUtils
    {
        public static int FindAvailablePort(int startPort)
        {
            var activeTcpListeners = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners();

            var activePorts = new List<int>();
            foreach (var activeTcpListener in activeTcpListeners)
            {
                activePorts.Add(activeTcpListener.Port);
            }


            for (var port = startPort; port < startPort + 1024; port++)
            {
                if (!activePorts.Contains(port))
                {
                    return port;
                }
            }
            return startPort;
        }

        public static bool IsPortAvailable(int port)
        {
            var activeTcpListeners = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners();

            foreach (var listener in activeTcpListeners)
            {
                if (listener.Port == port)
                {
                    return false;
                }
            }
            return true;
        }
    }
}