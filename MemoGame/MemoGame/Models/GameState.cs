using System;
using System.Collections.Generic;

namespace MemoGame.Models
{
    // Model representing a saved game state
    public class GameState
    {
        public string Username { get; set; }
        public string CategoryName { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<CardState> Cards { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public DateTime SavedAt { get; set; }

        public GameState()
        {
            Cards = new List<CardState>();
            SavedAt = DateTime.Now;
        }
    }

    // Model representing the state of a card in the game
    public class CardState
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public string ImagePath { get; set; }
        public bool IsFaceUp { get; set; }
        public bool IsMatched { get; set; }
    }
}