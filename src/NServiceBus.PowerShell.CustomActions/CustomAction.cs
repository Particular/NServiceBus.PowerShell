namespace NServiceBus.PowerShell.CustomActions
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Deployment.WindowsInstaller;

    public class CustomAction
    {
        const string PSModulePath = "PSMODULEPATH";

        [DllImport("User32.DLL")]
        static extern int SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);



        [CustomAction()]
        public static ActionResult AddToPSModuleEnvironmentVar(Session session)
        {
            //Advanced Installer doesn't notify of enviroment changes on system environment varibles

            var appDir = session["PowerShellModules_Dir"];

            var environmentVariable = Environment.GetEnvironmentVariable(PSModulePath, EnvironmentVariableTarget.Machine);
            if (environmentVariable != null)
            {
                var parts = environmentVariable.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!parts.Any(p => p.Equals(appDir, StringComparison.OrdinalIgnoreCase)))
                {
                    parts.Add(appDir);
                    var newValue = string.Join(";", parts);
                    Environment.SetEnvironmentVariable(PSModulePath, newValue, EnvironmentVariableTarget.Machine);
                }
            }
            return ActionResult.Success;
        }

        [CustomAction()]
        public static ActionResult RemoveFromPSModuleEnvironmentVar(Session session)
        {
            //Advanced Installer doesn't notify of enviroment changes on system environment varible

            var appDir = session["PowerShellModules_Dir"];

            var environmentVariable = Environment.GetEnvironmentVariable(PSModulePath, EnvironmentVariableTarget.Machine);
            if (environmentVariable != null)
            {
                var parts = environmentVariable.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                if (parts.Any(p => p.Equals(appDir, StringComparison.OrdinalIgnoreCase)))
                {
                    var newParts = parts.Where(p => !p.Equals(appDir, StringComparison.OrdinalIgnoreCase)).ToList();
                    var newValue = string.Join(";", newParts);
                    Environment.SetEnvironmentVariable(PSModulePath, newValue, EnvironmentVariableTarget.Machine);
                }
            }
            return ActionResult.Success;
        }

        static void Log(Session session, string message, params object[] args)
        {
            LogAction(session, string.Format(message, args));
        }

        public static Action<Session, string> LogAction = (s, m) => s.Log(m);

        public static Func<Session, string, string> GetAction = (s, key) => s[key];

        public static Action<Session, string, string> SetAction = (s, key, value) => s[key] = value;
    }

    public static class SessionExtensions
    {
        public static string Get(this Session session, string key)
        {
            return CustomAction.GetAction(session, key);
        }

        public static void Set(this Session session, string key, string value)
        {
            CustomAction.SetAction(session, key, value);
        }
    }
}
