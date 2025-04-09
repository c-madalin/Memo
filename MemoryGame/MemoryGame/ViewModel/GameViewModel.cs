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
        private readonly GameSaveService _gameSaveService;
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
        private int _elapsedTime;
        private bool _isTimerRunning;
        private bool _isGameLoaded;
        #endregion

        public int RowCount => _rows;
        public int ColumnCount => _columns;
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
        public GameViewModel(User currentUser, string category, int rows, int columns, int gameTime = 120, bool loadSavedGame = false)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _rows = rows;
            _columns = columns;
            _totalTime = gameTime;
            RemainingTime = gameTime;
            _userDataService = new UserDataService();
            _gameSaveService = new GameSaveService();
            _elapsedTime = 0;

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

            if (loadSavedGame)
            {
                LoadSavedGame();
            }
            else
            {
                InitializeGame();
            }
        }
        #endregion

        #region Game Logic
        private void InitializeGame()
        {
            try
            {
                // Calculate how many pairs we need
                int neededPairs = (_rows * _columns) / 2;

                // Ensure the grid can fit pairs (total cells must be even)
                if (_rows * _columns % 2 != 0)
                {
                    MessageBox.Show("The grid must have an even number of cells to create pairs.",
                        "Invalid Grid Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if we have enough images
                if (_imageFiles == null || _imageFiles.Count == 0)
                {
                    MessageBox.Show($"No images found for category {_category}.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a list of image paths that will be used for pairs
                var selectedImages = new List<string>();

                // If we don't have enough images, reuse them as needed
                if (_imageFiles.Count < neededPairs)
                {
                    // Keep adding images until we have enough
                    int currentIndex = 0;
                    while (selectedImages.Count < neededPairs)
                    {
                        selectedImages.Add(_imageFiles[currentIndex % _imageFiles.Count]);
                        currentIndex++;
                    }
                }
                else
                {
                    // Randomly select images without replacement
                    var random = new Random();
                    var shuffledImages = _imageFiles.OrderBy(x => random.Next()).ToList();
                    selectedImages = shuffledImages.Take(neededPairs).ToList();
                }

                // Create card pairs
                var cardPairs = new List<CardViewModel>();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing game: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                _elapsedTime++;
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
            // Pause the timer
            _gameTimer.Stop();

            // Create game state to save
            var gameState = new GameState
            {
                Category = _category,
                Rows = _rows,
                Columns = _columns,
                RemainingTime = RemainingTime,
                ElapsedTime = _elapsedTime,
                TotalTime = _totalTime,
                Cards = _cards.Select(c => new CardState
                {
                    Id = c.Id,
                    ImagePath = c.ImagePath,
                    IsFlipped = c.IsFlipped,
                    IsMatched = c.IsMatched
                }).ToList()
            };

            // Save the game
            bool success = _gameSaveService.SaveGame(_currentUser, gameState);

            if (success)
            {
                MessageBox.Show("Game saved successfully.", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to save game.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Resume the timer if game is not over
            if (!IsGameOver)
            {
                _gameTimer.Start();
            }
        }

        private void LoadSavedGame()
        {
            GameState savedGame = _gameSaveService.LoadGame(_currentUser);

            if (savedGame == null)
            {
                // No saved game found, start a new game
                InitializeGame();
                return;
            }

            _isGameLoaded = true;
            RemainingTime = savedGame.RemainingTime;
            _elapsedTime = savedGame.ElapsedTime;
            _totalTime = savedGame.TotalTime;

            // Create cards from saved state
            var cards = new List<CardViewModel>();
            foreach (var cardState in savedGame.Cards)
            {
                var card = new CardViewModel(cardState.Id, cardState.ImagePath)
                {
                    IsFlipped = cardState.IsFlipped,
                    IsMatched = cardState.IsMatched
                };
                card.CardClicked += Card_Clicked;
                cards.Add(card);
            }

            Cards = new ObservableCollection<CardViewModel>(cards);

            // Calculate matches found
            _matchesFound = Cards.Count(c => c.IsMatched) / 2;

            // Start the timer
            _isTimerRunning = true;
            _gameTimer.Start();

            // Update game status
            OnPropertyChanged(nameof(GameStatus));
        }

        // In GameViewModel.ExitGame method:
        private void ExitGame()
        {
            try
            {
                if (!IsGameOver)
                {
                    var result = MessageBox.Show("Are you sure you want to quit the current game? You can save your progress before exiting.",
                        "Confirm Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                        return;

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveGame();
                    }
                }

                _gameTimer.Stop();

                // Create new menu window
                var menuWindow = new MenuWindow();

                // Show it first
                menuWindow.Show();

                // Then close the game window
                var gameWindow = Application.Current.Windows.OfType<GameWindow>().FirstOrDefault(w => w.IsVisible);
                if (gameWindow != null && gameWindow.IsVisible)
                {
                    gameWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exiting game: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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