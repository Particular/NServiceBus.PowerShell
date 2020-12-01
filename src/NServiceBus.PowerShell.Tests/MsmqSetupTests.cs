namespace NServiceBus.PowerShell.Tests
{
    using System.Diagnostics;
    using NUnit.Framework;


    [TestFixture]
    public class MsmqSetupTests
    {
        [Explicit]
        [Test]
        public void IsMsmqInstalled()
        {
            Debug.WriteLine(new MsmqSetup().IsMsmqInstalled());
        }
        [Explicit]
        [Test]
        public void IsInstallationGood()
        {
            Debug.WriteLine(new MsmqSetup().IsInstallationGood());
        }

    }
}
