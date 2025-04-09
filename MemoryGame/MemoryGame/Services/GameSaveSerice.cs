using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MemoryGame.Model;
using MemoryGame.ViewModel;

namespace MemoryGame.Services
{
    public class GameSaveService
    {
        private readonly string _savesDirectory;

        public GameSaveService()
        {
            // Create directory for game saves
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(executablePath, @"..\..\..\"));
            _savesDirectory = Path.Combine(projectRoot, @"res\saves");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_savesDirectory))
            {
                Directory.CreateDirectory(_savesDirectory);
            }
        }

        public bool SaveGame(User user, GameState gameState)
        {
            try
            {
                string filePath = Path.Combine(_savesDirectory, $"{user.Username}.json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(gameState, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
                return false;
            }
        }

        public GameState LoadGame(User user)
        {
            try
            {
                string filePath = Path.Combine(_savesDirectory, $"{user.Username}.json");

                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);
                GameState gameState = JsonSerializer.Deserialize<GameState>(json);
                return gameState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game: {ex.Message}");
                return null;
            }
        }

        public bool DeleteSavedGame(string username)
        {
            try
            {
                string filePath = Path.Combine(_savesDirectory, $"{username}.json");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting saved game: {ex.Message}");
                return false;
            }
        }

        public bool HasSavedGame(User user)
        {
            string filePath = Path.Combine(_savesDirectory, $"{user.Username}.json");
            return File.Exists(filePath);
        }
    }

    public class GameState
    {
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int RemainingTime { get; set; }
        public int ElapsedTime { get; set; }
        public int TotalTime { get; set; }
        public List<CardState> Cards { get; set; }
    }

    public class CardState
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
    }
}