using System;

namespace Nippin
{
    public static class TimeExtensions
    {
        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);

    }
}
