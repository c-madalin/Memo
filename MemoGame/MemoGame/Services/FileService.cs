using MemoGame.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using static Newtonsoft.Json.Formatting; // Add this line

namespace MemoGame.Services
{
    // Implementation of the file service
    public class FileService : IFileService
    {
        private readonly string _dataDirectory;
        private readonly string _usersFilePath;
        private readonly string _statsFilePath;
        private readonly string _gameStatesDirectory;
        private readonly string _avatarsDirectory;

        public FileService()
        {
            // Set up file paths
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _usersFilePath = Path.Combine(_dataDirectory, "users.json");
            _statsFilePath = Path.Combine(_dataDirectory, "statistics.json");
            _gameStatesDirectory = Path.Combine(_dataDirectory, "GameStates");
            _avatarsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "avatar");

            // Create directories if they don't exist
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_gameStatesDirectory);
        }

        // Load the list of users from the users.json file
        public async Task<List<User>> LoadUsersAsync()
        {
            if (!File.Exists(_usersFilePath))
                return new List<User>();

            string json = await File.ReadAllTextAsync(_usersFilePath);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        // Save the list of users to the users.json file
        public async Task SaveUsersAsync(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Indented);
            await File.WriteAllTextAsync(_usersFilePath, json);
        }

        // Load a saved game state for a specific user
        public async Task<GameState> LoadGameStateAsync(string username)
        {
            string filePath = Path.Combine(_gameStatesDirectory, $"{username}.json");
            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<GameState>(json);
        }

        // Save a game state for a specific user
        public async Task SaveGameStateAsync(GameState gameState)
        {
            string filePath = Path.Combine(_gameStatesDirectory, $"{gameState.Username}.json");
            string json = JsonConvert.SerializeObject(gameState, Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        // Delete a saved game state for a specific user
        public Task DeleteGameStateAsync(string username)
        {
            string filePath = Path.Combine(_gameStatesDirectory, $"{username}.json");
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        // Load the statistics for all users
        public async Task<List<UserStatistics>> LoadStatisticsAsync()
        {
            if (!File.Exists(_statsFilePath))
                return new List<UserStatistics>();

            string json = await File.ReadAllTextAsync(_statsFilePath);
            return JsonConvert.DeserializeObject<List<UserStatistics>>(json) ?? new List<UserStatistics>();
        }

        // Save the statistics for all users
        public async Task SaveStatisticsAsync(List<UserStatistics> statistics)
        {
            string json = JsonConvert.SerializeObject(statistics, Indented);
            await File.WriteAllTextAsync(_statsFilePath, json);
        }

        // Update the statistics for a specific user after a game
        public async Task UpdateUserStatisticsAsync(string username, bool won)
        {
            var statistics = await LoadStatisticsAsync();
            var userStats = statistics.FirstOrDefault(s => s.Username == username);

            if (userStats == null)
            {
                userStats = new UserStatistics(username);
                statistics.Add(userStats);
            }

            userStats.AddGamePlayed(won);
            await SaveStatisticsAsync(statistics);
        }

        // Get a list of all available avatar images
        public List<string> GetAvailableAvatars()
        {
            if (!Directory.Exists(_avatarsDirectory))
                return new List<string>();

            return Directory.GetFiles(_avatarsDirectory, "*.jpg")
                .Concat(Directory.GetFiles(_avatarsDirectory, "*.png"))
                .Concat(Directory.GetFiles(_avatarsDirectory, "*.jpeg"))
                .Concat(Directory.GetFiles(_avatarsDirectory, "*.gif"))
                .ToList();
        }
    }
}