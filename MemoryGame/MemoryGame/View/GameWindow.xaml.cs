using System.Windows;
using MemoryGame.Model;
using MemoryGame.ViewModel;

namespace MemoryGame.View
{
    public partial class GameWindow : Window
    {
        public GameWindow(User currentUser, string category, int rows, int columns, int gameTime = 120, bool loadSavedGame = false)
        {
            try
            {
                InitializeComponent();

                // Validate grid dimensions
                if (rows < 2 || columns < 2)
                {
                    MessageBox.Show("Grid dimensions must be at least 2x2.",
                        "Invalid Grid Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                    rows = Math.Max(2, rows);
                    columns = Math.Max(2, columns);
                }

                // Ensure we have an even number of cells
                if ((rows * columns) % 2 != 0)
                {
                    // Make adjustments to ensure even number
                    if (columns % 2 != 0)
                        columns++;

                    MessageBox.Show("Grid dimensions have been adjusted to ensure an even number of cells.",
                        "Grid Adjustment", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Set DataContext
                DataContext = new GameViewModel(currentUser, category, rows, columns, gameTime, loadSavedGame);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating game window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}