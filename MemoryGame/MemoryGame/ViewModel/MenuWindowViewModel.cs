using MemoryGame.View;
using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MemoryGame.Commands;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryGame.ViewModel
{
    public class MenuWindowViewModel : INotifyPropertyChanged
    {
        #region Properties

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
        private List<int> _gridSizes = new List<int> { 1, 2, 3, 4, 5, 6 };
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
            // Initialize commands
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenExistingGame);
            StatisticsCommand = new RelayCommand(ShowStatistics);
            LogoutCommand = new RelayCommand(Logout);
            AboutCommand = new RelayCommand(ShowAbout);
        }
        #endregion

        #region Game Methods
        private void StartNewGame()
        {
            // TODO: Implement new game logic using SelectedCategory, SelectedRows, and SelectedColumns
            MessageBox.Show($"Starting new game with category: {SelectedCategory}, Grid: {SelectedRows}x{SelectedColumns}");
        }

        private void OpenExistingGame()
        {
            // TODO: Implement open game logic
            MessageBox.Show("Open game feature to be implemented");
        }

        private void ShowStatistics()
        {
            // TODO: Implement statistics view
            MessageBox.Show("Statistics feature to be implemented");
        }
        #endregion

        #region About
        private void ShowAbout()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
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
            menuWindow.Close();
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