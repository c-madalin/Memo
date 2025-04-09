using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MemoryGame.Commands;
using MemoryGame.Model;
using MemoryGame.Services;
using MemoryGame.View;

namespace MemoryGame.ViewModel
{
    public class GameViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private readonly UserDataService _userDataService;
        private readonly string _category;
        private readonly int _rows;
        private readonly int _columns;
        private readonly User _currentUser;
        private readonly DispatcherTimer _gameTimer;
        private readonly List<string> _imageFiles;
        private ObservableCollection<CardViewModel> _cards;
        private CardViewModel _firstCard;
        private CardViewModel _secondCard;
        private bool _canFlipCard = true;
        private bool _isGameOver;
        private int _matchesFound;
        private int _totalMatches;
        private int _remainingTime;
        private int _totalTime;
        private bool _isTimerRunning;
        #endregion

        #region Properties
        public ObservableCollection<CardViewModel> Cards
        {
            get => _cards;
            private set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        public int RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }

        public string TimeDisplay => $"Time: {RemainingTime} seconds";

        public string GameStatus
        {
            get
            {
                if (_isGameOver)
                {
                    if (_matchesFound == _totalMatches)
                        return "You Won!";
                    else
                        return "You Lost!";
                }
                return $"Matches: {_matchesFound}/{_totalMatches}";
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                _isGameOver = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GameStatus));
            }
        }

        public ICommand SaveGameCommand { get; }
        public ICommand ExitGameCommand { get; }
        #endregion

        #region Constructor
        public GameViewModel(User currentUser, string category, int rows, int columns, int gameTime = 120)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _rows = rows;
            _columns = columns;
            _totalTime = gameTime;
            RemainingTime = gameTime;
            _userDataService = new UserDataService();

            // Set up timer
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _gameTimer.Tick += GameTimer_Tick;

            // Load images
            string absoluteImagesPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..\..\..\res\Images\{_category}"));

            if (Directory.Exists(absoluteImagesPath))
            {
                _imageFiles = Directory.GetFiles(absoluteImagesPath, "*.jpg")
                    .Concat(Directory.GetFiles(absoluteImagesPath, "*.png"))
                    .Concat(Directory.GetFiles(absoluteImagesPath, "*.jpeg"))
                    .ToList();
            }
            else
            {
                MessageBox.Show($"The directory {absoluteImagesPath} does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _imageFiles = new List<string>();
            }

            _totalMatches = (_rows * _columns) / 2;

            // Initialize commands
            SaveGameCommand = new RelayCommand(SaveGame);
            ExitGameCommand = new RelayCommand(ExitGame);

            InitializeGame();
        }
        #endregion

        #region Game Logic
        private void InitializeGame()
        {
            // Create pairs of cards
            var cardPairs = new List<CardViewModel>();

            // Take as many images as needed for pairs
            var neededPairs = (_rows * _columns) / 2;
            var selectedImages = new List<string>();

            if (_imageFiles.Count > 0)
            {
                // We need to make sure we have enough images
                if (_imageFiles.Count < neededPairs)
                {
                    // If not enough images, we'll repeat some
                    while (selectedImages.Count < neededPairs)
                    {
                        selectedImages.AddRange(_imageFiles);
                    }
                    // Trim to exact count needed
                    selectedImages = selectedImages.Take(neededPairs).ToList();
                }
                else
                {
                    // Randomly select images
                    var random = new Random();
                    selectedImages = _imageFiles.OrderBy(x => random.Next()).Take(neededPairs).ToList();
                }
            }
            else
            {
                MessageBox.Show($"No image files found for category {_category}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create card pairs
            for (int i = 0; i < selectedImages.Count; i++)
            {
                var imagePath = selectedImages[i];

                // Create first card
                var card1 = new CardViewModel(i * 2, imagePath);
                card1.CardClicked += Card_Clicked;

                // Create second card (matching pair)
                var card2 = new CardViewModel(i * 2 + 1, imagePath);
                card2.CardClicked += Card_Clicked;

                cardPairs.Add(card1);
                cardPairs.Add(card2);
            }

            // Randomize card positions
            var random2 = new Random();
            Cards = new ObservableCollection<CardViewModel>(cardPairs.OrderBy(x => random2.Next()));

            // Start timer
            _isTimerRunning = true;
            _gameTimer.Start();
        }

        private void Card_Clicked(object sender, EventArgs e)
        {
            if (!_canFlipCard || IsGameOver) return;

            var clickedCard = sender as CardViewModel;
            if (clickedCard == null || clickedCard.IsFlipped || clickedCard.IsMatched) return;

            // Flip the card
            clickedCard.IsFlipped = true;

            // Check if this is the first or second card flipped
            if (_firstCard == null)
            {
                _firstCard = clickedCard;
            }
            else
            {
                _secondCard = clickedCard;
                _canFlipCard = false;

                // Check for match
                CheckForMatch();
            }
        }

        private void CheckForMatch()
        {
            if (_firstCard.ImagePath == _secondCard.ImagePath)
            {
                // Match found
                _firstCard.IsMatched = true;
                _secondCard.IsMatched = true;
                _matchesFound++;

                OnPropertyChanged(nameof(GameStatus));

                // Reset for next turn
                _firstCard = null;
                _secondCard = null;
                _canFlipCard = true;

                // Check if game is over
                if (_matchesFound == _totalMatches)
                {
                    GameOver(true);
                }
            }
            else
            {
                // No match, flip cards back after a delay
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    _firstCard.IsFlipped = false;
                    _secondCard.IsFlipped = false;

                    // Reset for next turn
                    _firstCard = null;
                    _secondCard = null;
                    _canFlipCard = true;
                }), DispatcherPriority.Background);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (RemainingTime > 0)
            {
                RemainingTime--;
            }
            else
            {
                GameOver(false);
            }
        }

        private void GameOver(bool isWin)
        {
            _gameTimer.Stop();
            IsGameOver = true;

            // Update user statistics
            _currentUser.GamesPlayed++;
            if (isWin)
            {
                _currentUser.GamesWon++;
            }

            // Save user statistics
            _userDataService.SaveUser(_currentUser);

            string message = isWin ? "Congratulations! You won!" : "Time's up! You lost.";
            MessageBox.Show(message, "Game Over", MessageBoxButton.OK,
                isWin ? MessageBoxImage.Information : MessageBoxImage.Exclamation);
        }
        #endregion

        #region Game Actions
        private void SaveGame()
        {
            // TODO: Implement game saving functionality
            MessageBox.Show("Game saving is not implemented yet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitGame()
        {
            if (!IsGameOver)
            {
                var result = MessageBox.Show("Are you sure you want to quit the current game?",
                    "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            _gameTimer.Stop();

            var gameWindow = Application.Current.Windows.OfType<GameWindow>().FirstOrDefault();
            MenuWindow menuWindow = new MenuWindow();
            menuWindow.Show();
            gameWindow?.Close();
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

    public class CardViewModel : INotifyPropertyChanged
    {
        private bool _isFlipped;
        private bool _isMatched;

        public int Id { get; }
        public string ImagePath { get; }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClickCommand { get; }

        public event EventHandler CardClicked;

        public CardViewModel(int id, string imagePath)
        {
            Id = id;
            ImagePath = imagePath;
            ClickCommand = new RelayCommand(OnCardClick);
        }

        private void OnCardClick()
        {
            if (!IsFlipped && !IsMatched)
            {
                CardClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}