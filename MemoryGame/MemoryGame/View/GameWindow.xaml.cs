using System.Windows;
using MemoryGame.Model;
using MemoryGame.ViewModel;

namespace MemoryGame.View
{
    public partial class GameWindow : Window
    {
        public GameWindow(User currentUser, string category, int rows, int columns, int gameTime = 120, bool loadSavedGame = false)
        {
            InitializeComponent();
            DataContext = new GameViewModel(currentUser, category, rows, columns, gameTime, loadSavedGame);
        }
    }
}