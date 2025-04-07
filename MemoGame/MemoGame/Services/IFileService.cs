using MemoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemoGame.Services
{
    // Interface for the file service
    public interface IFileService
    {
        // User operations
        Task<List<User>> LoadUsersAsync();
        Task SaveUsersAsync(List<User> users);

        // Game state operations
        Task<GameState> LoadGameStateAsync(string username);
        Task SaveGameStateAsync(GameState gameState);
        Task DeleteGameStateAsync(string username);

        // Statistics operations
        Task<List<UserStatistics>> LoadStatisticsAsync();
        Task SaveStatisticsAsync(List<UserStatistics> statistics);
        Task UpdateUserStatisticsAsync(string username, bool won);

        // List all available avatars
        List<string> GetAvailableAvatars();
    }
}