namespace LightJockey.Models
{
    public class AppSettings
    {
        public string LogLevel { get; set; } = "Information";
        public int RetainedLogFileCount { get; set; } = 10;
    }
}
