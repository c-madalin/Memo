using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MemoryGame.Commands;
using MemoryGame.Model;
using MemoryGame.Services;
using MemoryGame.View;

namespace MemoryGame.ViewModel
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly UserDataService _userDataService;
        private ObservableCollection<User> _users;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }

        public StatisticsViewModel()
        {
            _userDataService = new UserDataService();
            Users = _userDataService.LoadUsers();
            CloseCommand = new RelayCommand(Close);
        }

        private void Close()
        {
            foreach (var window in System.Windows.Application.Current.Windows)
            {
                if (window is StatisticsWindow statisticsWindow)
                {
                    statisticsWindow.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}