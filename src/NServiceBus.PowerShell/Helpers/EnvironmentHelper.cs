
namespace NServiceBus.PowerShell.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Mimics some of the items found in .Net 4 that are missing form .net System.Environment
    /// </summary>
    internal class EnvironmentHelper
    {
        const int MaxMachineNameLength = 256;

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(
                   [In]
                   IntPtr hSourceProcessHandle,
                   [Out, MarshalAs(UnmanagedType.Bool)]
                   out bool isWow64);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(String moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true, ExactSpelling = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, String methodName);
        
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int GetComputerName([Out]StringBuilder nameBuffer, ref int bufferSize);

        public static bool DoesWin32MethodExist(String moduleName, String methodName)
        {
            var hModule = GetModuleHandle(moduleName);
            if (hModule == IntPtr.Zero)
            {
                return false;
            }
            var functionPointer = GetProcAddress(hModule, methodName);
            return (functionPointer != IntPtr.Zero);
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
#if WIN32
            bool isWow64; 
            return DoesWin32MethodExist("Kernel32.dll", "IsWow64Process")
                  && IsWow64Process(Process.GetCurrentProcess().Handle, out isWow64)
                  && isWow64;
#else
            return true;
#endif
            }
        }

        public static String MachineName
        {   
            get
            {   
                var buf = new StringBuilder(MaxMachineNameLength);
                var len = MaxMachineNameLength;
                if (GetComputerName(buf, ref len) == 0)
                    throw new InvalidOperationException("InvalidOperation ComputerName");
                return buf.ToString();
            }
        }

    }
}
