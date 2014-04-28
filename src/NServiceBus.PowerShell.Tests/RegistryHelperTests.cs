namespace NServiceBus.PowerShell.Tests
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using Helpers;
    using Microsoft.Win32;
    using NUnit.Framework;

    [TestFixture]
    public class RegistryHelperTests
    {
        const string testkey = @"Software\ParticularSoftware-Test";

        [SetUp]
        public void Setup()
        {
            RegistryHelper.CurrentUser(RegistryView.Default).DeleteKeyTree(testkey);
        }

        [Test, Explicit]
        public void KeyExistsTest()
        {
            var hklm = RegistryHelper.LocalMachine(RegistryView.Default);
            Assert.IsTrue(hklm.KeyExists(@"Software\Microsoft"), @"KeyExists should be true for Software\Microsoft");
            Assert.IsFalse(hklm.KeyExists(@"Software\BogusRegistryEntry"), @"KeyExists should be false for Software\BogusRegistryEntry");
        }

        [Test,Explicit]
        public void TestSubKeyFunctions()
        {
            var hkcu = RegistryHelper.CurrentUser(RegistryView.Default);
            hkcu.CreateSubkey(testkey);
            Assert.IsTrue(hkcu.KeyExists(testkey), "Failed to create or verify test reg key");
        }

        [Test, Explicit]
        public void TestReadAndWriteString()
        {
            const string valueName = "teststring";
            var hkcu = RegistryHelper.CurrentUser(RegistryView.Default);
            var ticks = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            hkcu.WriteValue(testkey, valueName , ticks, RegistryValueKind.String);
            var val = (string) hkcu.ReadValue(testkey, valueName, null, true);
            Assert.IsTrue(string.Equals(val, ticks),  "The written string does match what was read" );
            Assert.IsTrue(hkcu.GetRegistryValueKind(testkey, valueName) == RegistryValueKind.String, "Failed to assert that written data was a string");
            
        }

        [Test, Explicit]
        public void TestReadAndWriteBinary()
        {
            const string valueName = "testbinary";
            var hkcu = RegistryHelper.CurrentUser(RegistryView.Default);
            var ticks = Encoding.Unicode.GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            hkcu.WriteValue(testkey, valueName, ticks, RegistryValueKind.Binary);
            var val = (byte[]) hkcu.ReadValue(testkey, valueName, null, true);
            Assert.IsTrue(BytesMatch(ticks, val), "The written byte array does match what was read");
            Assert.IsTrue(hkcu.GetRegistryValueKind(testkey, valueName) == RegistryValueKind.Binary, "Failed to assert that written data was binary");
            
        }

        [Test, Explicit]
        public void TestReadAndWriteDWord()
        {
            const string valueName = "testdword";
            var hkcu = RegistryHelper.CurrentUser(RegistryView.Default);
            var num = 1 + DateTime.Now.Millisecond; 
            hkcu.WriteValue(testkey, valueName, num, RegistryValueKind.DWord);
            var val = (Int32) hkcu.ReadValue(testkey, valueName, null, true);
            Assert.IsTrue(num == val, "The written dword does match what was read");
            Assert.IsTrue(hkcu.GetRegistryValueKind(testkey, valueName) == RegistryValueKind.DWord, "Failed to assert that written data was a dword");
        }

        [Test, Explicit]
        public void TestDeleteValue()
        {
            const string valueName = "delete-me";
            var hkcu = RegistryHelper.CurrentUser(RegistryView.Default);
            hkcu.WriteValue(testkey, valueName, "test", RegistryValueKind.String);
            Assert.IsTrue(hkcu.ValueExists(testkey, valueName), "Failed to assert that value exists ");
            hkcu.DeleteValue(testkey, valueName);
            Assert.IsFalse(hkcu.ValueExists(testkey, valueName), "Failed to assert that value was deleted");
            const string nonexistantValue = "I_Should_Not_Be_Present";
            Assert.IsTrue(hkcu.DeleteValue(testkey,nonexistantValue), "Deletion of a non-existent value should return true");
        }

        [TearDown]
        public void TearDown()
        {
            RegistryHelper.CurrentUser(RegistryView.Default).DeleteKeyTree(testkey);
        }

        static bool BytesMatch(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) return false;
            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) 
                    return false;
            }
            return true;
        }
    }
}
