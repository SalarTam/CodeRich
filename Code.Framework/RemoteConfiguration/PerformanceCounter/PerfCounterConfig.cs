using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Code.Framework.RemoteConfiguration
{
    [Obsolete("Legacy config, need to move to PerfCounterCategoryConfigCollection")]
    [XmlRoot("PerfCounterConfig")]
    public class PerfCounterConfigCollection
    {
        static PerfCounterConfigCollection()
        {
            instance = ConfigManager.GetSection<PerfCounterConfigCollection>();
            instance.EnsureCounters();
        }

        private static PerfCounterConfigCollection instance;

        public static PerfCounterConfigCollection Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance.EnsureCounters();
                instance = value;
            }
        }

        private void EnsureCounters()
        {
            if (PerformanceCounterCategory.Exists(Category))
            {
                bool same = true;

                try
                {
                    PerformanceCounterCategory cat = new PerformanceCounterCategory(Category);
                    same = (cat.CategoryType == CategoryType);
                }
                catch //maybe delete by others as well
                {
                }

                if (same)
                {
                    foreach (string name in dtCounters.Keys)
                    {
                        if (!PerformanceCounterCategory.CounterExists(name, Category))
                        {
                            same = false;
                            break;
                        }
                    }
                }
                if (!same)
                {
                    if (PerformanceCounterCategory.Exists(Category))
                        PerformanceCounterCategory.Delete(Category);
                    InstallCounters();
                }
            }
            else
                InstallCounters();

            if (counterConfigs != null)
            {
                foreach (PerfCounterConfig config in counterConfigs)
                {
                    dtCounters.Add(config.Name, CounterFromConfig(config));
                }
            }
        }

        private void InstallCounters()
        {
            if (counterConfigs != null)
            {
                CounterCreationDataCollection col = new CounterCreationDataCollection();

                foreach (PerfCounterConfig config in counterConfigs)
                {
                    CounterCreationData data = new CounterCreationData(config.Name, config.Help, config.CounterType);
                    col.Add(data);
                }
                PerformanceCounterCategory.Create(Category, Help, CategoryType, col);
            }
        }

        public string Category;
        public string Help;
        public bool MultipleInstance;

        private PerformanceCounterCategoryType CategoryType
        {
            get
            {
                return (MultipleInstance ? PerformanceCounterCategoryType.MultiInstance : PerformanceCounterCategoryType.SingleInstance);
            }
        }

        private PerfCounterConfig[] counterConfigs;

        [XmlArray("Counters")]
        [XmlArrayItem("Counter")]
        public PerfCounterConfig[] CounterConfigs
        {
            get
            {
                return counterConfigs;
            }
            set
            {
                counterConfigs = value;
            }
        }

        public PerformanceCounter GetCounter(string name)
        {
            PerformanceCounter counter;
            dtCounters.TryGetValue(name, out counter);
            return counter;
        }

        private Dictionary<string, PerformanceCounter> dtCounters = new Dictionary<string, PerformanceCounter>();

        private PerformanceCounter CounterFromConfig(PerfCounterConfig config)
        {
            PerformanceCounter counter;
            if (MultipleInstance)
                counter = new PerformanceCounter(Category, config.Name, ConfigUtility.ApplicationName, false);
            else
                counter = new PerformanceCounter(Category, config.Name, false);
            counter.RawValue = config.RawValue;
            return counter;
        }
    }

    [XmlRoot(Namespace = "Zhaopin.Campus.Framework.RemoteConfiguration")]
    public class PerfCounterConfig
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("help")]
        public string Help;

        [XmlAttribute("type")]
        public PerformanceCounterType CounterType;

        [XmlAttribute("rawValue")]
        public int RawValue = 0;
    }
}