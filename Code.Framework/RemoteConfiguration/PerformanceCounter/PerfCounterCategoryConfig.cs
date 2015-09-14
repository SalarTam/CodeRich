using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Code.Framework.RemoteConfiguration
{
    internal class PerformanceCounterCollection
    {
        private Dictionary<string, PerformanceCounter> counters = new Dictionary<string, PerformanceCounter>();

        public void AddCounter(PerformanceCounter counter)
        {
            counters.Add(counter.CounterName, counter);
        }

        public PerformanceCounter GetCounter(string name)
        {
            PerformanceCounter counter;
            counters.TryGetValue(name, out  counter);
            return counter;
        }
    }

    public class PerfCounterCategoryConfig
    {
        private static bool CounterExists(string counterName, string category)
        {
            try
            {
                return PerformanceCounterCategory.CounterExists(counterName, category);
            }
            catch
            {
                return false;
            }
        }

        private static void RemoveCategory(string category)
        {
            try
            {
                if (PerformanceCounterCategory.Exists(category))
                    PerformanceCounterCategory.Delete(category);
            }
            catch //no permission
            {
            }
        }

        internal void EnsureCounters()
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
                    if (counterConfigs != null)
                    {
                        foreach (PerfCounterConfig config in counterConfigs)
                        {
                            if (!CounterExists(config.Name, Category))
                            {
                                same = false;
                                break;
                            }
                        }
                    }
                }
                if (!same)
                {
                    RemoveCategory(Category);
                    InstallCounters();
                }
            }
            else
                InstallCounters();

            if (counterConfigs != null)
            {
                defaultInstanceCounters = CreatePerfCounters(null);
            }
        }

        private PerformanceCounterCollection CreatePerfCounters(string instance)
        {
            PerformanceCounterCollection counters = new PerformanceCounterCollection();
            foreach (PerfCounterConfig config in counterConfigs)
            {
                var perfCounter = CounterFromConfig(instance, config);
                if (perfCounter != null)
                    counters.AddCounter(perfCounter);
            }
            return counters;
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
                try
                {
                    PerformanceCounterCategory.Create(Category, Help, CategoryType, col);
                }
                catch (Exception ex)
                {
                }
            }
        }

        [XmlAttribute("name")]
        public string Category;

        [XmlAttribute("help")]
        public string Help;

        [XmlAttribute("multipleInstance")]
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
            return defaultInstanceCounters.GetCounter(name);
        }

        /// <summary>
        /// default counters
        /// </summary>
        private PerformanceCounterCollection defaultInstanceCounters = new PerformanceCounterCollection();

        /// <summary>
        /// instance counters
        /// </summary>
        private Dictionary<string, PerformanceCounterCollection> dtInstanceCounters = new Dictionary<string, PerformanceCounterCollection>();

        public PerformanceCounter GetCounter(string counterName, string instance)
        {
            PerformanceCounterCollection perfCat = defaultInstanceCounters;
            if (instance != null)
            {
                lock (dtInstanceCounters)
                {
                    if (!dtInstanceCounters.TryGetValue(instance, out perfCat))
                    {
                        perfCat = CreatePerfCounters(instance);
                        dtInstanceCounters.Add(instance, perfCat);
                        //Dictionary<string, PerformanceCounterCollection> cats = new Dictionary<string, PerformanceCounterCollection>(dtInstanceCounters);
                        //cats[instance] = perfCat;
                        //dtInstanceCounters = cats;
                    }
                }
            }
            return perfCat.GetCounter(counterName);
        }

        private PerformanceCounter CounterFromConfig(string instance, PerfCounterConfig config)
        {
            try
            {
                PerformanceCounter counter;
                if (MultipleInstance)
                {
                    if (instance == null)
                        counter = new PerformanceCounter(Category, config.Name, ConfigUtility.ApplicationName, false);
                    else
                        counter = new PerformanceCounter(Category, config.Name, instance, false);
                }
                else
                    counter = new PerformanceCounter(Category, config.Name, false);
                counter.RawValue = config.RawValue;
                return counter;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    [XmlRoot("PerfCounterCategories")]
    public class PerfCounterCategoryConfigCollection
    {
        private PerfCounterCategoryConfig[] categories;

        [XmlElement("Category")]
        public PerfCounterCategoryConfig[] Categories
        {
            get
            {
                return categories;
            }
            set
            {
                categories = value;
                if (value != null)
                {
                    foreach (PerfCounterCategoryConfig cat in value)
                    {
                        dt.Add(cat.Category, cat);
                    }
                }
            }
        }

        private Dictionary<string, PerfCounterCategoryConfig> dt = new Dictionary<string, PerfCounterCategoryConfig>();

        static PerfCounterCategoryConfigCollection()
        {
            instance = ConfigManager.GetSection<PerfCounterCategoryConfigCollection>();
            instance.EnsureCategories();
        }

        private static PerfCounterCategoryConfigCollection instance;

        public static PerfCounterCategoryConfigCollection Instance
        {
            get
            {
                return instance;
            }
            set
            {
                value.EnsureCategories();
                instance = value;
            }
        }

        private void EnsureCategories()
        {
            if (categories != null)
            {
                foreach (PerfCounterCategoryConfig cat in categories)
                {
                    cat.EnsureCounters();
                }
            }
        }

        public PerformanceCounter GetCounter(string name)
        {
            if (categories != null && categories.Length > 0)
                return categories[0].GetCounter(name);
            else
                return null;
        }

        public PerformanceCounter GetCounter(string category, string name)
        {
            PerfCounterCategoryConfig cat;
            if (dt.TryGetValue(category, out cat))
                return cat.GetCounter(name);
            else
                return null;
        }

        public PerformanceCounter GetCounter(string category, string name, string instance)
        {
            PerfCounterCategoryConfig cat;
            if (dt.TryGetValue(category, out cat))
                return cat.GetCounter(name, instance);
            else
                return null;
        }
    }
}