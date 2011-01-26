using System;

namespace unixtime
{
    public class secondParser
    {
        private long seconds;

        public secondParser(long _seconds)
        {
            seconds = _seconds;
        }

        public uint toMinutes
        {
            get { return (uint)System.Math.Round((float)(this.seconds / 60)); }
        }

        public uint toHours
        {
            get { return (uint)System.Math.Round((float)(this.toMinutes / 60)); }
        }

        public uint toDays
        {
            get { return (uint)System.Math.Round((float)(this.toHours / 24)); }
        }
    }

    public class UnixTime
    {
        private static DateTime BEGIN_UTC = new DateTime(1970, 1, 1);
        private long utValue;

        public UnixTime(long seconds)
        {
            utValue = seconds;
        }

        public UnixTime(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        public UnixTime()
        {
            utValue = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public long Value
        {
            get { return utValue; }
            set { utValue = value; }
        }

        public DateTime DateTime
        {
            get { return BEGIN_UTC.AddSeconds((long)utValue); }
            set { utValue = (long)((TimeSpan)(value - BEGIN_UTC)).TotalSeconds; }
        }

        public override string ToString()
        {
            return DateTime.ToLocalTime().ToString("yyy-MM-dd HH:mm:ss"); ;
        }
    }
}