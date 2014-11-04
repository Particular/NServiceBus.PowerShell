namespace NServiceBus.PowerShell.Tests
{
    using System;
    using System.Runtime.InteropServices;
    using Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class TestBitNess
    {
        [Test]
        public void Am_I_A_64_Bit_OperatingSystem()
        {

            var a = EnvironmentHelper.DoesWin32MethodExist("Kernel32.dll", "IsWow64Process");
            if (a)
            {
                bool isWow64;
                var b = EnvironmentHelper.IsWow64Process(EnvironmentHelper.GetCurrentProcess(), out isWow64);
            }

            if (EnvironmentHelper.Is64BitOperatingSystem)
            {
                Console.WriteLine("64 bit");
            }
            else
            {
                Console.WriteLine("32 bit");
            }
        }
    }
}
