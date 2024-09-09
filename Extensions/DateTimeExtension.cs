namespace CricHeroesAnalytics.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTimeOffset ToIndiaTime(this DateTimeOffset utcTime)
        {
            return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        }
    }
}
