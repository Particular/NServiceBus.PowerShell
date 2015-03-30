namespace NServiceBus.PowerShell.Helpers
{
    /// <summary>
    /// Registry Views for .Net 2 
    /// See RegistryHelper.cs
    /// </summary>
    internal enum RegistryView
    {
        Default = 0,
        Registry64 = 0x0100,
        Registry32 = 0x0200,
    }
}