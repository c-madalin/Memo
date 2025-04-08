using MemoGame.Commands;
using MemoGame.Models;
using MemoGame.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MemoGame.ViewModels
{
    public class SignInViewModel : BaseViewModel
    {
        private readonly IFileService _fileService;
        private readonly INavigationService _navigationService;
        private User _selectedUser;
        private string _currentAvatar;
        private int _currentAvatarIndex;
        private ObservableCollection<string> _avatarPaths;

        public ObservableCollection<User> Users { get; private set; }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    OnPropertyChanged(nameof(IsUserSelected));

                    if (value != null)
                    {
                        CurrentAvatar = value.AvatarPath;
                    }
                }
            }
        }

        public bool IsUserSelected => SelectedUser != null;

        public string CurrentAvatar
        {
            get => _currentAvatar;
            set => SetProperty(ref _currentAvatar, value);
        }

        // Commands
        public ICommand NewUserCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand PreviousAvatarCommand { get; private set; }
        public ICommand NextAvatarCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public SignInViewModel()
        {
            // Initialize services
            _fileService = new FileService();
            _navigationService = new NavigationService();

            // Initialize collections
            Users = new ObservableCollection<User>();
            _avatarPaths = new ObservableCollection<string>();

            // Initialize commands
            NewUserCommand = new RelayCommand(ShowNewUserDialog);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            PlayCommand = new RelayCommand(Play, CanPlay);
            PreviousAvatarCommand = new RelayCommand(_ => CycleAvatar(-1));
            NextAvatarCommand = new RelayCommand(_ => CycleAvatar(1));
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Load data
            LoadAvatars();
            LoadUsers();
        }

        private void LoadAvatars()
        {
            _avatarPaths.Clear();
            string avatarsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "avatar");

            if (Directory.Exists(avatarsDir))
            {
                string[] files = Directory.GetFiles(avatarsDir, "*.jpg")
                    .Concat(Directory.GetFiles(avatarsDir, "*.jpeg"))
                    .Concat(Directory.GetFiles(avatarsDir, "*.png"))
                    .ToArray();

                foreach (string file in files)
                {
                    _avatarPaths.Add(file);
                }
            }

            // Set default avatar if available
            if (_avatarPaths.Count > 0)
            {
                _currentAvatarIndex = 0;
                CurrentAvatar = _avatarPaths[0];
            }
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _fileService.LoadUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CycleAvatar(object parameter)
        {
            if (_avatarPaths.Count == 0)
                return;

            int direction = Convert.ToInt32(parameter);
            _currentAvatarIndex = (_currentAvatarIndex + direction + _avatarPaths.Count) % _avatarPaths.Count;
            CurrentAvatar = _avatarPaths[_currentAvatarIndex];
        }

        private void ShowNewUserDialog(object parameter)
        {
            // Create and show the New User Dialog
            var newUserDialog = new Views.NewUserDialogWindow();
            var viewModel = newUserDialog.DataContext as ViewModels.NewUserDialogViewModel;

            // Set available avatars
            if (viewModel != null)
            {
                viewModel.SetAvatarPaths(_avatarPaths.ToList());
            }

            if (newUserDialog.ShowDialog() == true)
            {
                // Create the new user
                string username = viewModel.Username;
                string avatarPath = viewModel.SelectedAvatar;

                // Validate
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(avatarPath))
                {
                    MessageBox.Show("Username and avatar are required.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if username already exists
                if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A user with this name already exists.", "Duplicate Username", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create and add user
                var newUser = new User(username, avatarPath);
                Users.Add(newUser);

                // Save users
                _fileService.SaveUsersAsync(Users.ToList());

                // Select the new user
                SelectedUser = newUser;
            }
        }

        private bool CanDeleteUser(object parameter)
        {
            return IsUserSelected;
        }

        private async void DeleteUser(object parameter)
        {
            if (SelectedUser == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete user {SelectedUser.Username}? This will delete all saved games and statistics.",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete saved game
                    await _fileService.DeleteGameStateAsync(SelectedUser.Username);

                    // Delete statistics
                    var statistics = await _fileService.LoadStatisticsAsync();
                    var userStats = statistics.FirstOrDefault(s => s.Username == SelectedUser.Username);
                    if (userStats != null)
                    {
                        statistics.Remove(userStats);
                        await _fileService.SaveStatisticsAsync(statistics);
                    }

                    // Remove from users collection
                    Users.Remove(SelectedUser);
                    await _fileService.SaveUsersAsync(Users.ToList());

                    // Reset selection
                    SelectedUser = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanPlay(object parameter)
        {
            return IsUserSelected;
        }

        private void Play(object parameter)
        {
            if (SelectedUser == null)
                return;

            // Navigate to game page
            _navigationService.NavigateToGame(SelectedUser);
        }
    }
}