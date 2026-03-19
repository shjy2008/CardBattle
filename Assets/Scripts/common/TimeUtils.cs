using System;

namespace Assets.Scripts.common
{
    public class TimeUtils
    {
        public TimeUtils()
        {

        }

        // 获得时间戳，单位：秒 
        public static long GetTimeStampSeconds()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret = Convert.ToInt64(ts.TotalSeconds);
            return ret;
        }

        // 获得时间戳，单位：毫秒 
        public static long GetTimeStampMilliSeconds()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret = Convert.ToInt64(ts.TotalMilliseconds);
            return ret;
        }
    }
}
