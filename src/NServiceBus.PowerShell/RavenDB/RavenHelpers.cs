namespace NServiceBus.PowerShell
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation.Host;
    using System.Net;
    using System.Security.Principal;

    /// <summary>
    /// Copied from RavenDB and modified for out needs
    /// </summary>
    public class RavenHelpers : CmdletHelperBase
    {
        public RavenHelpers()
        {
        }

         public RavenHelpers(PSHost Host) : base(Host)
        {
        }


        public bool EnsureCanListenToWhenInNonAdminContext(int port)
        {
            try
            {
                if (CanStartHttpListener(port))
                    return true;

               TryGrantingHttpPrivileges(port);

               return CanStartHttpListener(port);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void GetArgsForHttpAclCommand(int port, out string args, out string command)
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                command = "netsh";
                args = string.Format(@"http add urlacl url=http://+:{0}/ user=""{1}""", port,
                                     WindowsIdentity.GetCurrent().Name);
            }
            else
            {
                command = "httpcfg";
                args = string.Format(@"set urlacl /u http://+:{0}/ /a D:(A;;GX;;;""{1}"")", port, WindowsIdentity.GetCurrent().User);
            }
        }

        private static bool CanStartHttpListener(int port)
        {
            try
            {
                var httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://+:" + port + "/");
                httpListener.Start();
                httpListener.Stop();
                
                return true;
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode != 5) //access denies
                    throw;
            }

            return false;
        }

        private void TryGrantingHttpPrivileges(int port)
        {
            string args;
            string command;
            GetArgsForHttpAclCommand(port, out args, out command);

            WriteVerbose("Trying to grant rights for http.sys");
            try
            {
                Console.WriteLine("runas {0} {1}", command, args);
                using (var process = Process.Start(new ProcessStartInfo
                    {
                        Verb = "runas",
                        Arguments = args,
                        FileName = command,
                    }))
                {
                    process.WaitForExit();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // if we cant runas it is ok. we will verify the result later
            }
        }
    }

}