namespace NServiceBus.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Microsoft.Win32;

    [Cmdlet(VerbsCommon.Get, "NServiceBusLicenses")]
    public class GetNServiceBusLicenses : PSCmdlet
    {
        protected override void ProcessRecord() {
            var licensesStoredInRegistry = new List<LicenseStoredInRegistry>();

            if (Environment.Is64BitOperatingSystem) {
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry32));
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.CurrentUser, RegistryView.Registry32));
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.LocalMachine, RegistryView.Registry64));
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.CurrentUser, RegistryView.Registry64));
            }
            else {
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.LocalMachine, RegistryView.Default));
                licensesStoredInRegistry.AddRange(GetLicensesFromRegistry(RegistryHive.CurrentUser, RegistryView.Default));
            }

            licensesStoredInRegistry.ForEach(WriteObject);
        }

        static IEnumerable<LicenseStoredInRegistry> GetLicensesFromRegistry(RegistryHive hive, RegistryView view) {
            var baseKey = RegistryKey.OpenBaseKey(hive, view);
            const string nservicebusKeyName = @"SOFTWARE\NServiceBus";
            var nservicebusKey = baseKey.OpenSubKey(nservicebusKeyName);
            if (nservicebusKey == null) {
                yield break;
            }
            var nservicebusLicenseKeyNames = nservicebusKey.GetSubKeyNames();
            foreach (var nservicebusLicenseKeyName in nservicebusLicenseKeyNames) {
                var nservicebusLicenseKey = nservicebusKey.OpenSubKey(nservicebusLicenseKeyName);
                if (nservicebusLicenseKey == null) {
                    continue;
                }
                var license = (string) nservicebusLicenseKey.GetValue("License");
                if (license == null) {
                    continue;
                }
                var licenseStoredInRegistry = GetLicenseStoredInRegistry(view, nservicebusLicenseKey);
                if (licenseStoredInRegistry != null) {
                    yield return licenseStoredInRegistry;
                }
            }
        }

        public static LicenseStoredInRegistry GetLicenseStoredInRegistry(RegistryView view, RegistryKey nservicebusLicenseKey) {
            var license = (string) nservicebusLicenseKey.GetValue("License");
            if (license == null) {
                return null;
            }
            var xml = XDocument.Parse(license);
            var licenseElement = xml.XPathSelectElement("/license");
            var licenseStoredInRegistry = new LicenseStoredInRegistry(view, nservicebusLicenseKey, licenseElement);
            return licenseStoredInRegistry;
        }
    }

    public class LicenseStoredInRegistry
    {
        public LicenseStoredInRegistry(RegistryView view, RegistryKey registryKey, XElement licenseElement) {
            this.licenseElement = licenseElement;
            RegistryKey = registryKey;
            RegistryView = view;
            xnm.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");
        }

        public RegistryView RegistryView { get; private set; }

        public RegistryKey RegistryKey { get; private set; }

        public string Type {
            get { return licenseElement.Attribute("LicenseType").Value; }
        }

        public string Version {
            get { return licenseElement.Attribute("LicenseVersion").Value; }
        }

        public string MaxMessageThroughputPerSecond {
            get { return GetLicenseAttribute("MaxMessageThroughputPerSecond"); }
        }

        public string UpgradeProtectionExpiration {
            get { return GetLicenseAttribute("UpgradeProtectionExpiration"); }
        }

        public string WorkerThreads {
            get { return GetLicenseAttribute("WorkerThreads"); }
        }

        public string AllowedNumberOfWorkerNodes {
            get { return GetLicenseAttribute("AllowedNumberOfWorkerNodes"); }
        }

        public string Quantity {
            get { return GetLicenseAttribute("Quantity"); }
        }

        public string FullLicense {
            get {
                var license = licenseElement;
                // remove the license signature from output to clean up output
                license.XPathSelectElement("/license/sig:Signature", xnm).Remove();
                return license.ToString();
            }
        }

        string GetLicenseAttribute(string attributeName) {
            return licenseElement.Attribute(attributeName).Value;
        }

        readonly XElement licenseElement;
        XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
    }
}