namespace MemoGame.Models
{
    // Model representing game statistics for a user
    public class UserStatistics
    {
        public string Username { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }

        // Calculated property for win percentage
        public double WinPercentage => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;

        public UserStatistics()
        {
        }

        public UserStatistics(string username)
        {
            Username = username;
            GamesPlayed = 0;
            GamesWon = 0;
        }

        // Method to record a game result
        public void AddGamePlayed(bool won)
        {
            GamesPlayed++;
            if (won)
            {
                GamesWon++;
            }
        }
    }
}