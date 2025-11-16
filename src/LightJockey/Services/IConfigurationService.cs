using System.Threading.Tasks;
using LightJockey.Models;

namespace LightJockey.Services
{
    public interface IConfigurationService
    {
        Task<LightJockeyEntertainmentConfig> LoadConfigAsync();
        Task SaveConfigAsync(LightJockeyEntertainmentConfig config);

        Task<string?> GetSecureValueAsync(string key);
        Task<bool> SetSecureValueAsync(string key, string value);

        Task<AppSettings> LoadAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);

        Task<bool> RemoveValueAsync(string key);
        Task<bool> ContainsKeyAsync(string key);
        Task<bool> ClearAllAsync();
    }
}
