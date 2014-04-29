namespace NServiceBus.PowerShell.Tests
{
    using System.IO;
    using NUnit.Framework;
    

    [TestFixture]
    public class RavenDBSetupTests
    {
        [Explicit]
        [Test]
        public void Install()
        {
            new RavenDBSetup().Install();
        }

        [Test]
        public void EnsureGetRavenResourcesIsNotEmpty()
        {
            var combine = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(combine);
                new RavenDBSetup().ExportRavenResources(combine);
                Assert.IsNotEmpty(Directory.GetFiles(combine));
            }
            finally
            {
                Directory.Delete(combine, true);
            }
        }
    }
}
