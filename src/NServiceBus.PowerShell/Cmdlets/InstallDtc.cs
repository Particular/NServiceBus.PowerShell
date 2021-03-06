﻿namespace NServiceBus.PowerShell
{
    using System;
    using System.Management.Automation;
    using Helpers;
    using System.Text.RegularExpressions;

    [Cmdlet(VerbsLifecycle.Install, "NServiceBusDTC", SupportsShouldProcess = true)]
    [Obsolete(WarningMessage)]
    public class InstallDtc : CmdletBase
    {
        const string WarningMessage = "Installing DTC is supported natively by PowerShell. See https://docs.particular.net/search?q=powershell+dtc.";
        
        [Parameter(Mandatory = false, HelpMessage = "Port Range to use for DCOM Config. The format should be two numbers separated by a dash. e.g. \"5000-6000\"")]
        public string PortRange { get; set; }

        protected override void ProcessRecord()
        {
            WriteWarning(WarningMessage);
            
            if (!StringExtensions.IsNullOrWhiteSpace(PortRange))
            {
                var portRangeRegex = new Regex(@"^[0-9]+\-[0-9]+$");
                var match = portRangeRegex.Match(PortRange);
                if (!match.Success) ThrowTerminatingError(new ErrorRecord(new Exception("Invalid value for PortRange parameter. The format should be two numbers separated by a dash. e.g. \"5000-6000\""), "1", ErrorCategory.InvalidArgument, ""));
            }

            if (ShouldProcess(EnvironmentHelper.MachineName))
            {
                new DtcSetup(Host).StartDtcIfNecessary(PortRange);
            }
        }
    }
}
