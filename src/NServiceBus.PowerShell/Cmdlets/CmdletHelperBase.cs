namespace NServiceBus.PowerShell
{
    using System;
    using System.Management.Automation.Host;

    public abstract class CmdletHelperBase 
    {
        internal PSHost Host { get; set; }

        protected CmdletHelperBase()
        {

        }

        protected CmdletHelperBase(PSHost host)
        {
            Host = host;
        }

        internal void WriteLine(string message, params object[] args)
        {
            if (Host != null)
            {
                Host.UI.WriteLine(string.Format(message, args));
            }
            else
            {
                Console.WriteLine(message, args);
            }
        }

        internal void WriteVerbose(string message, params object[] args)
        {
            if (Host != null)
            {
                Host.UI.WriteVerboseLine(string.Format(message, args));
            }
            else
            {
                Console.WriteLine("VERBOSE: " + message, args);
            }
        }

        internal void WriteWarning(string message, params object[] args)
        {
            if (Host != null)
            {
                Host.UI.WriteWarningLine(string.Format(message, args));
            }
            else
            {
                Console.WriteLine("WARNING: " + message, args);
            }
        }

        internal void WriteError(string message, params object[] args)
        {
            if (Host != null)
            {
                Host.UI.WriteWarningLine(string.Format(message, args));
            }
            else
            {
                Console.WriteLine("ERROR: " + message, args);
            }
        }
    }
}
