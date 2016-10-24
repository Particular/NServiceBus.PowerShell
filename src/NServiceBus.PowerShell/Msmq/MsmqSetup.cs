// ReSharper disable CommentTypo
namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Host;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;
    using System.Text;
    using Helpers;
    
    /// <summary>
    /// Utility class for starting and installing MSMQ.
    /// </summary>
    public class MsmqSetup : CmdletHelperBase
    {

        public MsmqSetup()
        {

        }

        public MsmqSetup(PSHost Host) : base(Host)
        {
            
        }

    /// <summary>
    /// Checks that MSMQ is installed, configured correctly, and started, and if not takes the necessary corrective actions to make it so.
    /// </summary>
    public bool StartMsmqIfNecessary()
    {
        var processUtil = new ProcessUtil(Host);

        if (!InstallMsmqIfNecessary())
        {
            return false;
        }

        try
        {
            using (var controller = new ServiceController("MSMQ"))
            {
                if (IsStopped(controller))
                {
                    processUtil.ChangeServiceStatus(controller, ServiceControllerStatus.Running, controller.Start);
                }
            }
        }
        catch (InvalidOperationException)
        {
            WriteWarning("MSMQ windows service not found! You may need to reboot after MSMQ has been installed.");
            return false;
        }

        return true;
    }

    static bool IsStopped(ServiceController controller)
    {
        return controller.Status == ServiceControllerStatus.Stopped || controller.Status == ServiceControllerStatus.StopPending;
    }

    internal bool IsMsmqInstalled()
    {
        var dll = LoadLibraryW("Mqrt.dll");
        return (dll != IntPtr.Zero);
    }

    /// <summary>
    /// Determines if the msmq installation on the current machine is ok
    /// </summary>
    public bool IsInstallationGood()
    {
        const string subkey = @"SOFTWARE\Microsoft\MSMQ\Setup";
        var regView = EnvironmentHelper.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;
        var hklm = RegistryHelper.LocalMachine(regView);
        if (!hklm.KeyExists(subkey))
        {
            return false;
        }

        return HasOnlyNeededComponents(hklm.GetValueNames(subkey));
    }

    bool InstallMsmqIfNecessary()
    {
        WriteVerbose("Checking if MSMQ is installed.");

        var os = GetOperatingSystem();

        if (IsMsmqInstalled())
        {
            WriteVerbose("MSMQ is installed.");
            WriteVerbose("Checking that only needed components are active.");

            if (IsInstallationGood())
            {
                WriteVerbose("Installation is good.");
                return true;
            }

            WriteWarning("Installation isn't good. Make sure you remove the following components: {0} and also {1}", String.Join(", ", UndesirableMsmqComponentsXp.ToArray()), String.Join(", ", UndesirableMsmqComponentsV4.ToArray()));
            return false;
        }

        WriteVerbose("MSMQ is not installed. Going to install.");

        switch (os)
        {
            case OperatingSystemEnum.XpOrServer2003:
                InstallMsmqOnXpOrServer2003();
                break;

            case OperatingSystemEnum.Vista:
                RunExe(OcSetup, OcSetupVistaInstallCommand);
                break;

            case OperatingSystemEnum.Server2008:
                RunExe(OcSetup, OcSetupInstallCommand);
                break;

            case OperatingSystemEnum.Windows7:
                RunExe(dismPath, "/Online /NoRestart /English /Enable-Feature /FeatureName:MSMQ-Container /FeatureName:MSMQ-Server");
                break;
            case OperatingSystemEnum.Windows8:
            case OperatingSystemEnum.Windows10:
            case OperatingSystemEnum.Server2012:
                RunExe(dismPath, "/Online /NoRestart /English /Enable-Feature /all /FeatureName:MSMQ-Server");
                break;

            default:
                WriteWarning("OS not supported.");
                return false;
        }

        WriteVerbose("Installation of MSMQ successful.");

        return true;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

    public  void RunExe(string filename, string args)
    {
        var startInfo = new ProcessStartInfo(filename, args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetTempPath()
        };

        WriteVerbose("Executing {0} {1}", startInfo.FileName, startInfo.Arguments);

        var ptr = new IntPtr();
        var fileSystemRedirectionDisabled = false;

        if (EnvironmentHelper.Is64BitOperatingSystem)
        {
            fileSystemRedirectionDisabled = Wow64DisableWow64FsRedirection(ref ptr);
        }

        try
        {
            using (var process = new Process())
            {
                var output = new StringBuilder();
                var error = new StringBuilder();

                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                WriteLine(output.ToString());
                WriteLine(error.ToString());
            }
        }
        finally
        {
            if (fileSystemRedirectionDisabled)
            {
                Wow64RevertWow64FsRedirection(ptr);
            }
        }
    }

    void InstallMsmqOnXpOrServer2003()
    {
        var p = Path.GetTempFileName();

        WriteVerbose("Creating installation instructions file.");

        using (var sw = File.CreateText(p))
        {
            sw.WriteLine("[Version]");
            sw.WriteLine("Signature = \"$Windows NT$\"");
            sw.WriteLine();
            sw.WriteLine("[Global]");
            sw.WriteLine("FreshMode = Custom");
            sw.WriteLine("MaintenanceMode = RemoveAll");
            sw.WriteLine("UpgradeMode = UpgradeOnly");
            sw.WriteLine();
            sw.WriteLine("[Components]");

            foreach (var s in RequiredMsmqComponentsXp)
                sw.WriteLine(s + " = ON");

            foreach (var s in UndesirableMsmqComponentsXp)
                sw.WriteLine(s + " = OFF");

            sw.Flush();
        }

        WriteVerbose("Installation instructions file created.");
        WriteVerbose("Invoking MSMQ installation.");

        RunExe("sysocmgr", "/i:sysoc.inf /x /q /w /u:%temp%\\" + Path.GetFileName(p));
    }

    // Based on http://msdn.microsoft.com/en-us/library/windows/desktop/ms724833(v=vs.85).aspx
    static OperatingSystemEnum GetOperatingSystem()
    {
        var osVersionInfoEx = new OSVersionInfoEx
        {
            OSVersionInfoSize = (UInt32) Marshal.SizeOf(typeof(OSVersionInfoEx))
        };

        GetVersionEx(osVersionInfoEx);

        switch (Environment.OSVersion.Version.Major)
        {
            case 10:
                return OperatingSystemEnum.Windows10;

            case 6:
                switch (Environment.OSVersion.Version.Minor)
                {
                    case 0:

                        if (osVersionInfoEx.ProductType == VER_NT_WORKSTATION)
                        {
                            return OperatingSystemEnum.Vista;
                        }

                        return OperatingSystemEnum.Server2008;

                    case 1:
                        if (osVersionInfoEx.ProductType == VER_NT_WORKSTATION)
                        {
                            return OperatingSystemEnum.Windows7;
                        }

                        return OperatingSystemEnum.Server2008;

                    case 2:
                    case 3:
                        if (osVersionInfoEx.ProductType == VER_NT_WORKSTATION)
                        {
                            return OperatingSystemEnum.Windows8;
                        }

                        return OperatingSystemEnum.Server2012;
                }
                break;

            case 5:
                return OperatingSystemEnum.XpOrServer2003;
        }

        return OperatingSystemEnum.Unsupported;
    }

    bool HasOnlyNeededComponents(IEnumerable<string> installedComponents)
    {
        var needed = new List<string>(RequiredMsmqComponentsXp);

        foreach (var i in installedComponents)
        {
            if (UndesirableMsmqComponentsXp.Contains(i))
            {
                WriteWarning("Undesirable MSMQ component installed: " + i);
                return false;
            }

            if (UndesirableMsmqComponentsV4.Contains(i))
            {
                WriteWarning("Undesirable MSMQ component installed: " + i);
                return false;
            }

            needed.Remove(i);
        }

        if (needed.Count == 0)
            return true;

        return false;
    }

    // Return Type: HMODULE->HINSTANCE->HINSTANCE__*
    // lpLibFileName: LPCWSTR->WCHAR*
    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW")]
    static extern IntPtr LoadLibraryW([In] [MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);


    [DllImport("Kernel32", CharSet = CharSet.Auto)]
    static extern Boolean GetVersionEx([Out] [In] OSVersionInfo versionInformation);


    // ReSharper disable UnusedField.Compiler
    // ReSharper disable NotAccessedField.Local
    // ReSharper disable UnassignedField.Compiler
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OSVersionInfoEx : OSVersionInfo
    {
        public UInt16 ServicePackMajor;
        public UInt16 ServicePackMinor;
        public UInt16 SuiteMask;
        public byte ProductType;
        public byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OSVersionInfo
    {
// ReSharper disable once NotAccessedField.Global
        public UInt32 OSVersionInfoSize =
            (UInt32) Marshal.SizeOf(typeof(OSVersionInfo));

        public UInt32 MajorVersion = 0;
        public UInt32 MinorVersion = 0;
        public UInt32 BuildNumber = 0;
        public UInt32 PlatformId = 0;
        // Attribute used to indicate marshalling for String field
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public String CSDVersion = null;
    }

    // ReSharper restore UnusedField.Compiler
    // ReSharper restore NotAccessedField.Local
    // ReSharper restore UnassignedField.Compiler

    const byte VER_NT_WORKSTATION = 1;

    static List<string> RequiredMsmqComponentsXp = new List<string>(new[]
    {
        "msmq_Core",
        "msmq_LocalStorage"
    });

    static List<string> UndesirableMsmqComponentsXp = new List<string>(new[]
    {
        "msmq_ADIntegrated",
        "msmq_TriggersService",
        "msmq_HTTPSupport",
        "msmq_RoutingSupport",
        "msmq_MQDSService"
    });

    static List<string> UndesirableMsmqComponentsV4 = new List<string>(new[]
    {
        "msmq_DCOMProxy",
        "msmq_MQDSServiceInstalled",
        "msmq_MulticastInstalled",
        "msmq_RoutingInstalled",
        "msmq_TriggersInstalled"
    });

    internal enum OperatingSystemEnum
    {
        Unsupported,
        XpOrServer2003,
        Vista,
        Server2008,
        Windows7,
        Windows8,
        Server2012,
        Windows10
    }

    const string OcSetup = "OCSETUP";
    static string dismPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "dism.exe");
    const string OcSetupInstallCommand = "MSMQ-Server /passive";
    const string OcSetupVistaInstallCommand = "MSMQ-Container;MSMQ-Server /passive";
}
}
