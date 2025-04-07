using System;
using System.Windows;
using System.Windows.Controls;
using MemoGame.Models;

namespace MemoGame.Services
{
    // Interface for the navigation service
    public interface INavigationService
    {
        void NavigateToSignIn();
        void NavigateToGame(User user);
        void NavigateToGame(User user, GameState savedGame);
        void NavigateToStatistics();
    }

    // Implementation of the navigation service
    public class NavigationService : INavigationService
    {
        // Helper method to get the main frame from the application's main window
        private Frame GetMainFrame()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                return mainWindow.SignIn; // Changed from 'Main' to 'SignIn'
            }

            throw new InvalidOperationException("Main frame not found.");
        }

        // Navigate to the sign-in page
        public void NavigateToSignIn()
        {
            GetMainFrame().Navigate(new Views.SignInPage());
        }

        // Navigate to the game page with a user
        public void NavigateToGame(User user)
        {
            // You'll implement the GamePage later
            // GetMainFrame().Navigate(new Views.GamePage(user));

            // For now, just show a message
            MessageBox.Show($"Starting new game for user: {user.Username}", "Navigation");
        }

        // Navigate to the game page with a user and a saved game
        public void NavigateToGame(User user, GameState savedGame)
        {
            // You'll implement the GamePage later
            // GetMainFrame().Navigate(new Views.GamePage(user, savedGame));

            // For now, just show a message
            MessageBox.Show($"Loading saved game for user: {user.Username}", "Navigation");
        }

        // Navigate to the statistics page
        public void NavigateToStatistics()
        {
            // You'll implement the StatisticsPage later
            // GetMainFrame().Navigate(new Views.StatisticsPage());

            // For now, just show a message
            MessageBox.Show("Viewing statistics", "Navigation");
        }
    }
}