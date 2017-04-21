﻿namespace NServiceBus.PowerShell
{
    using System;
    using System.Management.Automation;
    
    [Cmdlet(VerbsLifecycle.Install, "NServiceBusPerformanceCounters")]
    public class InstallPerformanceCounters : CmdletBase
    {
        // ReSharper disable  MemberCanBePrivate.Global
        [Parameter(Mandatory = false, HelpMessage = "Force re-creation of performance counters if they already exist.")]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing()
        {
            WriteWarning("This cmdlet is obsolete.  Performance counter registration is now handled via the NServiceBus.Metrics.PerformanceMonitor nuget package.  For legacy installations this cmdlet can still be used. Please refer to the NServiceBus.Metrics.PerformanceMonitor documentation for further information");
        }
        
        // ReSharper enable  MemberCanBePrivate.Global
        protected override void ProcessRecord()
        {
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
