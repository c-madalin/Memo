using MemoGame.Commands;
using System.Collections.Generic;
using System.Windows.Input;

namespace MemoGame.ViewModels
{
    public class NewUserDialogViewModel : BaseViewModel
    {
        private string _username;
        private string _selectedAvatar;
        private int _currentAvatarIndex;
        private List<string> _avatarPaths;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(CanCreate));
            }
        }

        public string SelectedAvatar
        {
            get => _selectedAvatar;
            set => SetProperty(ref _selectedAvatar, value);
        }

        public bool CanCreate => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(SelectedAvatar);

        public ICommand NextAvatarCommand { get; private set; }
        public ICommand PreviousAvatarCommand { get; private set; }

        public NewUserDialogViewModel()
        {
            _avatarPaths = new List<string>();
            NextAvatarCommand = new RelayCommand(_ => CycleAvatar(1));
            PreviousAvatarCommand = new RelayCommand(_ => CycleAvatar(-1));
        }

        public void SetAvatarPaths(List<string> avatarPaths)
        {
            _avatarPaths = avatarPaths;

            // Set default avatar if available
            if (_avatarPaths.Count > 0)
            {
                _currentAvatarIndex = 0;
                SelectedAvatar = _avatarPaths[0];
            }
        }

        private void CycleAvatar(object parameter)
        {
            if (_avatarPaths.Count == 0)
                return;

            int direction = (int)parameter;
            _currentAvatarIndex = (_currentAvatarIndex + direction + _avatarPaths.Count) % _avatarPaths.Count;
            SelectedAvatar = _avatarPaths[_currentAvatarIndex];
        }
    }
}