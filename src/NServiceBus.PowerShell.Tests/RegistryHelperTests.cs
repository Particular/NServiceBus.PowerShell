namespace NServiceBus.PowerShell.Tests
{
    using System.Diagnostics;
    using Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class RegistryHelperTests
    {
        [Test, Explicit]
        public void KeyExistsTest()
        {
            var hklm = RegistryHelper.LocalMachine(RegistryView.Default);

            Assert.IsTrue(hklm.KeyExists(@"Software\Microsoft"), @"KeyExists should be true for Software\Microsoft");
            Assert.IsFalse(hklm.KeyExists(@"Software\BogusRegistryEntry"), @"KeyExists should be false for Software\BogusRegistryEntry");
        }
    }
}
