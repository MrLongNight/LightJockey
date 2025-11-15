using System.Collections.Generic;

namespace LightJockey.Models
{
    public class LightJockeyEntertainmentConfig
    {
        public string? AppKey { get; set; }
        public double TargetFrameRate { get; set; } = 30;
        public bool AudioReactive { get; set; } = true;
        public double MinBrightness { get; set; } = 0.2;
        public double MaxBrightness { get; set; } = 1.0;
        public Dictionary<string, string> SecureValues { get; set; } = new();
    }
}
