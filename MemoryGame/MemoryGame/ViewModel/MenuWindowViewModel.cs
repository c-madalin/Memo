using MemoryGame.View;
using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using MemoryGame.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MemoryGame.Services;

namespace MemoryGame.ViewModel
{
    public class MenuWindowViewModel : INotifyPropertyChanged
    {
        #region Properties

        private readonly UserDataService _userDataService;
        private readonly GameSaveService _gameSaveService;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        // Game Time in seconds
        private int _gameTime = 120;
        public int GameTime
        {
            get => _gameTime;
            set
            {
                _gameTime = value;
                OnPropertyChanged();
            }
        }

        // Game time options
        private List<int> _timeOptions = new List<int> { 60, 120, 180, 240, 300 };
        public List<int> TimeOptions
        {
            get => _timeOptions;
            set
            {
                _timeOptions = value;
                OnPropertyChanged();
            }
        }

        // Categories collection
        private List<string> _categories = new List<string> { "Animals", "Flags", "Fruits" };
        public List<string> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        // Selected category
        private string _selectedCategory = "Animals";
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        // Grid sizes collection (1-6)
        private List<int> _gridSizes = new List<int> { 2, 3, 4, 5, 6 };
        public List<int> GridSizes
        {
            get { return _gridSizes; }
            set
            {
                _gridSizes = value;
                OnPropertyChanged();
            }
        }

        // Selected rows
        private int _selectedRows = 4;
        public int SelectedRows
        {
            get { return _selectedRows; }
            set
            {
                _selectedRows = value;
                OnPropertyChanged();
            }
        }

        // Selected columns
        private int _selectedColumns = 4;
        public int SelectedColumns
        {
            get { return _selectedColumns; }
            set
            {
                _selectedColumns = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand NewGameCommand { get; private set; }
        public ICommand OpenGameCommand { get; private set; }
        public ICommand StatisticsCommand { get; private set; }
        public ICommand AboutCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        #region Constructor
        public MenuWindowViewModel()
        {
            _userDataService = new UserDataService();
            _gameSaveService = new GameSaveService();

            // Get the current user from SignInViewModel
            var signInWindow = Application.Current.Windows.OfType<SignInWindow>().FirstOrDefault();
            if (signInWindow != null && signInWindow.DataContext is SignInViewModel signInViewModel)
            {
                CurrentUser = signInViewModel.SelectedUser;
            }

            // Initialize commands
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenExistingGame, CanOpenExistingGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            LogoutCommand = new RelayCommand(Logout);
            AboutCommand = new RelayCommand(ShowAbout);
        }
        #endregion

        #region Game Methods
        private void StartNewGame()
        {
            // Validate grid size (make sure it's even for matching pairs)
            int totalCards = SelectedRows * SelectedColumns;
            if (totalCards % 2 != 0)
            {
                MessageBox.Show("The total number of cards must be even for matching pairs.",
                    "Invalid Grid Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create and show the game window
            GameWindow gameWindow = new GameWindow(CurrentUser, SelectedCategory, SelectedRows, SelectedColumns, GameTime);
            gameWindow.Show();

            // Close the menu window
            var menuWindow = Application.Current.Windows.OfType<MenuWindow>().FirstOrDefault();
            menuWindow?.Close();
        }

        private bool CanOpenExistingGame()
        {
            return CurrentUser != null && _gameSaveService.HasSavedGame(CurrentUser);
        }

        private void OpenExistingGame()
        {
            if (CurrentUser == null)
            {
                MessageBox.Show("Please select a user first.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_gameSaveService.HasSavedGame(CurrentUser))
            {
                MessageBox.Show("No saved game found for this user.", "No Saved Game", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Load saved game data
            var savedGame = _gameSaveService.LoadGame(CurrentUser);

            if (savedGame != null)
            {
                // Create and show the game window with saved game data
                GameWindow gameWindow = new GameWindow(CurrentUser, savedGame.Category, savedGame.Rows, savedGame.Columns, savedGame.TotalTime, true);
                gameWindow.Show();

                // Close the menu window
                var menuWindow = Application.Current.Windows.OfType<MenuWindow>().FirstOrDefault();
                menuWindow?.Close();
            }
            else
            {
                MessageBox.Show("Failed to load saved game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics()
        {
            StatisticsWindow statisticsWindow = new StatisticsWindow();
            statisticsWindow.Owner = Application.Current.MainWindow;
            statisticsWindow.ShowDialog();
        }
        #endregion

        #region About
        private void ShowAbout()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = Application.Current.MainWindow;
            aboutWindow.ShowDialog();
        }
        #endregion

        #region Logout
        private void Logout()
        {
            var menuWindow = Application.Current.Windows
                    .OfType<MenuWindow>()
                    .FirstOrDefault();
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Show();
            menuWindow?.Close();
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}