namespace AndWeHaveAPlan.Scriptor
{
    public class LoggerSettings
    {
        public string TimestampFormat;

        public static LoggerSettings Default = new LoggerSettings
        {
            TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK"
        };
    }
}
