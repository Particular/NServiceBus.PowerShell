namespace NServiceBus.PowerShell.Cmdlets
{
    public struct MachineSettingsResult
    {
        public string Registry { get; set; }
        public string AuditQueue { get; set; }
        public string ErrorQueue { get; set; }
    }
}
