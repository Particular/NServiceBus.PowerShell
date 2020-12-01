﻿namespace NServiceBus.PowerShell
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation.Host;

    public class PerformanceCounterSetup : CmdletHelperBase
    {

        public PerformanceCounterSetup()
        {
        }

        public PerformanceCounterSetup(PSHost Host) : base(Host)
        {
        }

        const string categoryName = "NServiceBus";

        public bool CheckCounters()
        {
            return PerformanceCounterCategory.Exists(categoryName) && CheckCountersExist();
        }

        static bool CheckCountersExist()
        {
            foreach (var counter in Counters)
            {
                if (!PerformanceCounterCategory.CounterExists(counter.CounterName, categoryName))
                    return false;
            }
            return true;
        }

        public bool DoesCategoryExist()
        {
            return PerformanceCounterCategory.Exists(categoryName);
        }

        public void DeleteCategory()
        {
            PerformanceCounterCategory.Delete(categoryName);
        }

        public void SetupCounters()
        {
            var counterCreationCollection = new CounterCreationDataCollection(Counters.ToArray());
            PerformanceCounterCategory.Create(categoryName, "NServiceBus statistics", PerformanceCounterCategoryType.MultiInstance, counterCreationCollection);
            PerformanceCounter.CloseSharedResources(); // http://blog.dezfowler.com/2007/08/net-performance-counter-problems.html
        }

        static List<CounterCreationData> Counters = new List<CounterCreationData>
                    {
                        new CounterCreationData("Critical Time", "Age of the oldest message in the queue.", PerformanceCounterType.NumberOfItems32),
                        new CounterCreationData("SLA violation countdown","Seconds until the SLA for this endpoint is breached.",PerformanceCounterType.NumberOfItems32),
                        new CounterCreationData("# of msgs successfully processed / sec", "The current number of messages processed successfully by the transport per second.",PerformanceCounterType.RateOfCountsPerSecond32),
                        new CounterCreationData("# of msgs pulled from the input queue /sec", "The current number of messages pulled from the input queue by the transport per second.", PerformanceCounterType.RateOfCountsPerSecond32),
                        new CounterCreationData("# of msgs failures / sec", "The current number of failed processed messages by the transport per second.", PerformanceCounterType.RateOfCountsPerSecond32)
                    };
    }
}