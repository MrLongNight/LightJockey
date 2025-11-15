using System.Threading.Tasks;
using LightJockey.Models;

namespace LightJockey.Services
{
    public interface IConfigurationService
    {
        Task<LightJockeyEntertainmentConfig> LoadConfigAsync(string? encryptionKey = null);
        Task SaveConfigAsync(LightJockeyEntertainmentConfig config, string? encryptionKey = null);

        Task<string?> GetSecureValueAsync(string key);
        Task SetSecureValueAsync(string key, string value);

        AppSettings LoadAppSettings();
        void SaveAppSettings(AppSettings appSettings);
    }
}
