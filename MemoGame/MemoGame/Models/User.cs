using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoGame.Models
{
    // Model representing a user in the game
    public class User : INotifyPropertyChanged
    {
        private string _username;
        private string _avatarPath;

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AvatarPath
        {
            get => _avatarPath;
            set
            {
                if (_avatarPath != value)
                {
                    _avatarPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public User()
        {
        }

        public User(string username, string avatarPath)
        {
            Username = username;
            AvatarPath = avatarPath;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj is User other)
            {
                return string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Username?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }
    }
}