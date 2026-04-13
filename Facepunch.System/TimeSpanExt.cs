using System;

namespace Facepunch
{
    public static class TimeSpanExt
    { 
        // This method allows us to create TimeSpans from ms values that are
        // < 0.5ms
        public static TimeSpan FromMicroseconds(double ms)
        {
            return new TimeSpan((long)(ms * 1000) * TimeSpan.TicksPerMillisecond / 1000);
        }
    }
}
