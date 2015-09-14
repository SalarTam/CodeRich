using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Wintellect.Threading.ResourceLocks.Diagnostics
{
    public sealed class StatisticsGatheringResourceLock : DiagnosticResourceLock
    {
        [AttributeUsage(AttributeTargets.Property)]
        private sealed class StatisticPropertyAttribute : Attribute
        {
        }

        private static List<PropertyInfo> s_statisticProperties;
        private long m_ReadRequests;
        private long m_WriteRequests;
        private long m_ReadersReading;
        private long m_WritersWriting;
        private long m_ReadersDone;
        private long m_WritersDone;
        private long m_ReadersWaiting;
        private long m_WritersWaiting;
        private long m_ReaderMaxWaitTime;
        private long m_WriterMaxWaitTime;
        private long m_ReaderMinHoldTime;
        private long m_ReaderMaxHoldTime;
        private Dictionary<int, long> m_ReaderStartHoldTime = new Dictionary<int, long>();
        private long m_WriterMinHoldTime;
        private long m_WriterMaxHoldTime;
        private long m_WriterStartHoldTime;
        private ResourceLock m_lock = new ExclusiveSpinResourceLock();

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long ReadRequests
        {
            get
            {
                return this.m_ReadRequests;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long WriteRequests
        {
            get
            {
                return this.m_WriteRequests;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long ReadersReading
        {
            get
            {
                return this.m_ReadersReading;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long WritersWriting
        {
            get
            {
                return this.m_WritersWriting;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long ReadersDone
        {
            get
            {
                return this.m_ReadersDone;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long WritersDone
        {
            get
            {
                return this.m_WritersDone;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long ReadersWaiting
        {
            get
            {
                return this.m_ReadersWaiting;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public long WritersWaiting
        {
            get
            {
                return this.m_WritersWaiting;
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan ReaderMaxWaitTime
        {
            get
            {
                return new TimeSpan(this.m_ReaderMaxWaitTime);
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan WriterMaxWaitTime
        {
            get
            {
                return new TimeSpan(this.m_WriterMaxWaitTime);
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan ReaderMinHoldTime
        {
            get
            {
                return new TimeSpan(this.m_ReaderMinHoldTime);
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan ReaderMaxHoldTime
        {
            get
            {
                return new TimeSpan(this.m_ReaderMaxHoldTime);
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan WriterMinHoldTime
        {
            get
            {
                return new TimeSpan(this.m_WriterMinHoldTime);
            }
        }

        [StatisticsGatheringResourceLock.StatisticPropertyAttribute]
        public TimeSpan WriterMaxHoldTime
        {
            get
            {
                return new TimeSpan(this.m_WriterMaxHoldTime);
            }
        }

        static StatisticsGatheringResourceLock()
        {
            StatisticsGatheringResourceLock.s_statisticProperties = new List<PropertyInfo>();
            PropertyInfo[] properties = typeof(StatisticsGatheringResourceLock).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                if (Attribute.IsDefined(propertyInfo, typeof(StatisticsGatheringResourceLock.StatisticPropertyAttribute)))
                {
                    StatisticsGatheringResourceLock.s_statisticProperties.Add(propertyInfo);
                }
            }
        }

        public StatisticsGatheringResourceLock(ResourceLock resLock)
            : base(resLock, resLock.IsMutualExclusive)
        {
        }

        public StatisticsGatheringResourceLock(IResourceLock resLock)
            : base(resLock, false)
        {
        }

        public StatisticsGatheringResourceLock(IResourceLock resLock, bool isMutualExclusive)
            : base(resLock, isMutualExclusive)
        {
        }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder stringBuilder = new StringBuilder(base.ToString(format, formatProvider));
            if (string.Compare(format, "extra", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                stringBuilder.AppendLine();
                foreach (PropertyInfo current in StatisticsGatheringResourceLock.s_statisticProperties)
                {
                    stringBuilder.AppendLine(string.Concat(new object[]
					{
						"   ",
						current.Name,
						"=",
						current.GetValue(this, null)
					}));
                }
            }
            return stringBuilder.ToString();
        }

        protected override void OnWaitToRead()
        {
            using (this.m_lock)
            {
                this.m_ReadRequests += 1L;
                this.m_ReadersWaiting += 1L;
            }
            long timestamp = Stopwatch.GetTimestamp();
            base.InnerLock.WaitToRead();
            using (this.m_lock)
            {
                this.m_ReaderMaxWaitTime = Math.Max(this.m_ReaderMaxWaitTime, checked(Stopwatch.GetTimestamp() - timestamp));
                this.m_ReadersWaiting -= 1L;
                this.m_ReadersReading += 1L;
                this.m_ReaderStartHoldTime.Add(Thread.CurrentThread.ManagedThreadId, Stopwatch.GetTimestamp());
            }
        }

        protected override void OnDoneReading()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            long val = checked(Stopwatch.GetTimestamp() - this.m_ReaderStartHoldTime[managedThreadId]);
            using (this.m_lock)
            {
                this.m_ReaderStartHoldTime.Remove(managedThreadId);
                this.m_ReaderMinHoldTime = Math.Min(this.m_ReaderMinHoldTime, val);
                this.m_ReaderMaxHoldTime = Math.Max(this.m_ReaderMaxHoldTime, val);
                this.m_ReadersReading -= 1L;
                this.m_ReadersDone += 1L;
            }
            base.InnerLock.DoneReading();
        }

        protected override void OnWaitToWrite()
        {
            using (this.m_lock)
            {
                this.m_WriteRequests += 1L;
                this.m_WritersWaiting += 1L;
            }
            long timestamp = Stopwatch.GetTimestamp();
            base.InnerLock.WaitToWrite();
            this.m_WriterMaxWaitTime = Math.Max(this.m_WriterMaxWaitTime, checked(Stopwatch.GetTimestamp() - timestamp));
            this.m_WritersWaiting -= 1L;
            this.m_WritersWriting += 1L;
            this.m_WriterStartHoldTime = Stopwatch.GetTimestamp();
        }

        protected override void OnDoneWriting()
        {
            long val = checked(Stopwatch.GetTimestamp() - this.m_WriterStartHoldTime);
            this.m_WriterMinHoldTime = Math.Min(this.m_WriterMinHoldTime, val);
            this.m_WriterMaxHoldTime = Math.Max(this.m_WriterMaxHoldTime, val);
            this.m_WritersWriting -= 1L;
            this.m_WritersDone += 1L;
            base.InnerLock.DoneWriting();
        }
    }
}