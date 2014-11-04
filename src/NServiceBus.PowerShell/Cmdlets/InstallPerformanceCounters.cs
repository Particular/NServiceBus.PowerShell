namespace NServiceBus.PowerShell
{
    using System;
    using System.Management.Automation;
    

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusPerformanceCounters", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class InstallPerformanceCounters : CmdletBase
    {
        // ReSharper disable  MemberCanBePrivate.Global

        [Parameter(Mandatory = false, HelpMessage = "Force re-creation of performance counters if they already exist.")]
        public SwitchParameter Force { get; set; }

        // ReSharper enable  MemberCanBePrivate.Global

        protected override void ProcessRecord()
        {
            if (!ShouldProcess(Environment.MachineName))
            {
                return;
            }
            if (Force)
            {
                ForceCreate();
            }
            else
            {
                Create();   
            }
        }

        void Create()
        {
            var setup = new PerformanceCounterSetup(Host);
            var allCountersExist = setup.CheckCounters();
            if (allCountersExist)
            {
                return;
            }

            if (setup.DoesCategoryExist())
            {
                var exception = new Exception("Existing category is not configured correctly. Use the -Force option to delete and re-create");
                var errorRecord = new ErrorRecord(exception, "FailedToCreateCategory", ErrorCategory.NotSpecified, null);
                ThrowTerminatingError(errorRecord);
            }
            setup.SetupCounters();
        }

        void ForceCreate()
        {
            var setup = new PerformanceCounterSetup(Host);
            try
            {
               setup.DeleteCategory();
            }
            catch (Exception exception)
            {
                var errorRecord = new ErrorRecord(exception, "FailedToDeleteCategory", ErrorCategory.NotSpecified, null);
                ThrowTerminatingError(errorRecord);
            }
          
            setup.SetupCounters();
        }
    }
}
