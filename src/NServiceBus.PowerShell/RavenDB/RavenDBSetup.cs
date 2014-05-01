namespace NServiceBus.PowerShell
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Host;
    using System.Net;
    using System.ServiceProcess;
    using System.Xml;
    using Microsoft.Win32;
    using Helpers;

    public class RavenDBSetup : CmdletHelperBase
    {

        public RavenDBSetup()
        {
        }

        public RavenDBSetup(PSHost Host) : base(Host)
        {
        }



        public const int DefaultPort = 8080;

        public bool Check(int port = 0)
        {
            if (port == 0)
            {
                port = DefaultPort;
            }

            var url = String.Format("http://localhost:{0}", port);

            WriteVerbose("Checking if Raven is listening on {0}.", url);

            var result = IsRavenDBv2RunningOn(port);

            return result;
        }

        public int FindRavenDBPort()
        {
            var port = ReadRavenPortFromRegistry();

            if (port == 0)
                port = DefaultPort;

            if (Check(port))
                return port;

            return 0;
        }


        bool IsRavenDBv2RunningOn(int port)
        {
            var webRequest = WebRequest.Create(String.Format("http://localhost:{0}", port));
            webRequest.Timeout = 2000;

            try
            {
                var webResponse = webRequest.GetResponse();
                var serverBuildHeader = webResponse.Headers["Raven-Server-Build"];

                if (serverBuildHeader == null)
                {
                    return false;
                }

                int serverBuild;

                if (!int.TryParse(serverBuildHeader, out serverBuild))
                {
                    return false;
                }

                return serverBuild >= 2000; //at least raven v2
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Install(int port = 0, string installPath = null)
        {
            const string DefaultDirectoryName = "NServiceBus.Persistence.v4";
            if (string.IsNullOrEmpty(installPath))
            {
                installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", String.Empty), DefaultDirectoryName);
            }

            if (Directory.Exists(installPath))
            {
                WriteWarning("Path '{0}' already exists please update RavenDB manually if needed.", installPath);
                return;
            }

            var serviceName = "RavenDB";
            var service = FindService(serviceName);
            if (service != null)
            {
                WriteWarning("There is already a RavenDB service installed on this computer, the RavenDB service status is {0}.", service.Status);

                if (IsRavenDBv2RunningOn(8080)) //todo: we can improve this in the future by looking into the config file to try to figure out the port
                {
                    WriteWarning("Existing Raven is v2, NServiceBus will be configured to use it");

                    SavePortToBeUsedForRavenInRegistry(8080);

                    return;
                }

                serviceName = serviceName + "-v2";

            }

            int availablePort;

            //Check if the port is available, if so let the installer setup raven if its being run
            if (port > 0)
            {
                availablePort = PortUtils.FindAvailablePort(port);
                if (availablePort != port)
                {
                    if (IsRavenDBv2RunningOn(port))
                    {
                        WriteLine("A compatible(v2) version of Raven has been found on port {0} and will be used", port);

                        SavePortToBeUsedForRavenInRegistry(port);
                    }
                    else
                    {
                        WriteLine("Port '{0}' isn't available, please specify a different port and rerun the command.", port);
                    }

                    return;
                }
            }
            else
            {
                availablePort = PortUtils.FindAvailablePort(DefaultPort);

                //is there already a Raven2 on the default port?
                if (availablePort != DefaultPort && IsRavenDBv2RunningOn(DefaultPort))
                {
                    WriteVerbose("A compatible(v2) version of Raven has been found on port {0} and will be used", DefaultPort);

                    SavePortToBeUsedForRavenInRegistry(DefaultPort);
                    return;
                }
            }

            if (! new RavenHelpers(Host).EnsureCanListenToWhenInNonAdminContext(availablePort))
            {
                WriteWarning("Failed to grant rights for listening to http on port {0}, please specify a different port and rerun the command.", availablePort);
                return;
            }

            if (!Directory.Exists(installPath))
            {
                Directory.CreateDirectory(installPath);
            }

            WriteVerbose("Unpacking resources...");

            ExportRavenResources(installPath);

            var ravenConfigPath = Path.Combine(installPath, "Raven.Server.exe.config");
            var ravenConfig = new XmlDocument();

            ravenConfig.Load(ravenConfigPath);

            var key = (XmlElement)ravenConfig.DocumentElement.SelectSingleNode("//add[@key='Raven/Port']");

            key.SetAttribute("value", availablePort.ToString(CultureInfo.InvariantCulture));

            ravenConfig.Save(ravenConfigPath);
            WriteVerbose("Updated Raven configuration to use port {0}.", availablePort);

            SavePortToBeUsedForRavenInRegistry(availablePort);

            WriteVerbose("Installing RavenDB as a windows service.");

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = installPath,
                Arguments = "--install --service-name=" + serviceName,
                FileName = Path.Combine(installPath, "Raven.Server.exe")
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(20000);

                var line = process.StandardOutput.ReadToEnd();
                WriteLine(line);

                if (process.ExitCode != 0)
                {
                    WriteError("The RavenDB service failed to start, Raven.Server.exe exit code was {0}.", process.ExitCode);
                    return;
                }
            }

            WriteVerbose("{0} service started, listening on port: {1}", serviceName, availablePort);
        }

        static ServiceController FindService(string serviceName)
        {
            ServiceController service = null;
            foreach (var s in ServiceController.GetServices())
            {
                if (s.ServiceName == serviceName)
                {
                    service = s;
                    break;
                }
            }
            return service;
        }

        public void ExportRavenResources(string directoryPath)
        {
            var assembly = typeof(RavenDBSetup).Assembly;

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.Contains("RavenResources"))
                {
                    continue;
                }

                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    var fileName = resourceName.Replace(assembly.GetName().Name + ".RavenResources.", "");
                    var destinationPath = Path.Combine(directoryPath, fileName);
                    WriteLine("Unpacking '{0}' to '{1}'...", fileName, destinationPath);
                    using (Stream file = File.OpenWrite(destinationPath))
                    {
                        resourceStream.CopyTo(file);
                        file.Flush();
                    }
                }
            }
        }

        static void SavePortToBeUsedForRavenInRegistry(int availablePort)
        {
            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                WriteRegistry(availablePort, RegistryView.Registry32);
                WriteRegistry(availablePort, RegistryView.Registry64);
            }
            else
            {
                WriteRegistry(availablePort, RegistryView.Default);
            }
        }

        static int ReadRavenPortFromRegistry()
        {
            object portValue;

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                portValue = ReadRegistry(RegistryView.Registry32) ?? ReadRegistry(RegistryView.Registry64);
            }
            else
            {
                portValue = ReadRegistry(RegistryView.Default);
            }

            if (portValue == null)
            {
                return 0;
            }

            return (int)portValue;
        }

        static void WriteRegistry(int availablePort, RegistryView view)
        {
            var baseKey = RegistryHelper.LocalMachine(view);
            const string keyName = @"SOFTWARE\ParticularSoftware\ServiceBus";
            const string valName = "RavenPort";
            if (!baseKey.WriteValue(keyName, valName, availablePort, RegistryValueKind.DWord))
            {
                throw new Exception(string.Format("Failed to set value '{0}' to '{1}' in '{2}'", valName, availablePort, keyName));
            }
        }

        static object ReadRegistry(RegistryView view)
        {
            return RegistryHelper.LocalMachine(view).ReadValue(@"SOFTWARE\ParticularSoftware\ServiceBus", "RavenPort", null, true);
        }
    }
}