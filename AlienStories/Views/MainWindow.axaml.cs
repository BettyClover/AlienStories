using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlienStories.Models;
using AlienStories.Services;
using AlienStories.ViewModels;

namespace AlienStories.Views;

public partial class MainWindow : Window
{
    private readonly DatabaseService _db;
    private readonly MainWindowViewModel _viewModel;
    private readonly List<FloatingAlien> _floatingAliens = new();
    private readonly Random _random = new();
    private DispatcherTimer? _gameLoop;
    private DispatcherTimer? _spawnTimer;
    private DispatcherTimer? _gameUpdateTimer;

    public MainWindow()
    {
        InitializeComponent();

        _db = new DatabaseService();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        _viewModel.CreatureJumped += OnCreatureJumped;
        _viewModel.CollectionUpdated += OnCollectionUpdated;

        DrawStars();
        GameCanvas.PointerPressed += OnCanvasPointerPressed;
        StartGameLoop();

        SpawnInitialAliens();

        InitStarGame();

        InitMemoryGame();
    }

    private async void OnCreatureHugged(CapturedCreature creature)
    {
        _viewModel.GameStatusText = $"🤗 Ты обнял {creature.Nickname}! ❤️";

        AlienCatControl? control = null;
        foreach (var alien in _floatingAliens)
        {
            if (alien.Catalog.Id == creature.CatalogId)
            {
                control = GameCanvas.Children
                    .FirstOrDefault(c => c.Tag == alien) as AlienCatControl;
                break;
            }
        }

        if (control == null)
        {
            await ShowHeartsInCenter();
        }
        else
        {
            await ShowHearts(control);
        }
    }

    private async Task ShowHeartsInCenter()
    {
        var canvas = GameCanvas;
        if (canvas == null) return;

        var random = new Random();
        var center = new Point(canvas.Width / 2, canvas.Height / 2);

        for (int i = 0; i < 8; i++)
        {
            var heart = new TextBlock
            {
                Text = "❤️",
                FontSize = 20 + random.Next(20),
                Opacity = 1,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(
                        (byte)random.Next(200, 255),
                        (byte)random.Next(50, 150),
                        (byte)random.Next(50, 150)
                    )
                )
            };

            var offsetX = random.Next(-100, 100);
            var offsetY = random.Next(-50, 50);

            Canvas.SetLeft(heart, center.X + offsetX);
            Canvas.SetTop(heart, center.Y + offsetY);
            canvas.Children.Add(heart);

            var startY = Canvas.GetTop(heart);
            for (int step = 0; step < 40; step++)
            {
                await Task.Delay(20);
                Canvas.SetTop(heart, startY - step * 2);
                heart.Opacity = 1 - (step / 40.0);
                Canvas.SetLeft(heart, Canvas.GetLeft(heart) + (step % 2 == 0 ? 0.5 : -0.5));
            }

            canvas.Children.Remove(heart);
        }
    }

    private async Task ShowHearts(AlienCatControl cat)
    {
        var canvas = GameCanvas;
        if (canvas == null) return;

        var random = new Random();
        var catPosition = new Point(
            Canvas.GetLeft(cat) + cat.Width / 2,
            Canvas.GetTop(cat) + cat.Height / 2
        );

        for (int i = 0; i < 6; i++)
        {
            var heart = new TextBlock
            {
                Text = "❤️",
                FontSize = 16 + random.Next(16),
                Opacity = 1,
                Foreground = new SolidColorBrush(
                    Color.FromRgb(
                        (byte)random.Next(200, 255),
                        (byte)random.Next(50, 150),
                        (byte)random.Next(50, 150)
                    )
                )
            };

            var offsetX = random.Next(-40, 40);
            var offsetY = random.Next(-20, 10);

            Canvas.SetLeft(heart, catPosition.X + offsetX);
            Canvas.SetTop(heart, catPosition.Y + offsetY);
            canvas.Children.Add(heart);

            var startY = Canvas.GetTop(heart);
            for (int step = 0; step < 30; step++)
            {
                await Task.Delay(20);
                Canvas.SetTop(heart, startY - step * 2);
                heart.Opacity = 1 - (step / 30.0);
            }

            canvas.Children.Remove(heart);
        }
    }

    // ============ ОСТАЛЬНЫЕ МЕТОДЫ ============

    private void SpawnInitialAliens()
    {
        var freeSlots = 10 - _viewModel.FriendsCount;
        var count = Math.Min(3, freeSlots);

        for (int i = 0; i < count; i++)
        {
            SpawnAlien();
        }

        if (freeSlots <= 0)
        {
            _viewModel.GameStatusText = "😅 В приюте нет мест! Отпусти кого-нибудь, чтобы пригласить нового друга.";
        }
    }

    private void OnCollectionUpdated()
    {
        foreach (var alien in _floatingAliens.ToList())
        {
            RemoveAlien(alien);
        }

        SpawnInitialAliens();
    }

    private void DrawStars()
    {
        if (GameCanvas == null) return;

        for (int i = 0; i < 150; i++)
        {
            var star = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = 1 + _random.Next(2),
                Height = 1 + _random.Next(2),
                Fill = new SolidColorBrush(Color.FromRgb(
                    (byte)_random.Next(200, 256),
                    (byte)_random.Next(200, 256),
                    (byte)_random.Next(200, 256)
                )),
                Opacity = 0.3 + _random.NextDouble() * 0.7
            };

            Canvas.SetLeft(star, _random.Next(0, 800));
            Canvas.SetTop(star, _random.Next(0, 500));
            GameCanvas.Children.Add(star);
        }
    }

    private void StartGameLoop()
    {
        _gameLoop = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameLoop.Tick += (s, e) => UpdateAliens();
        _gameLoop.Start();

        _spawnTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5 + _random.NextDouble() * 5)
        };
        _spawnTimer.Tick += (s, e) =>
        {
            if (_viewModel.FriendsCount < 10)
            {
                SpawnAlien();
            }
        };
        _spawnTimer.Start();
    }

    private void SpawnAlien()
    {
        if (GameCanvas == null) return;

        if (_viewModel.FriendsCount >= 10) return;

        var catalogList = _db.GetAllCatalog();
        if (catalogList.Count == 0) return;

        var catalogItem = catalogList[_random.Next(catalogList.Count)];

        var alien = new FloatingAlien
        {
            Catalog = catalogItem,
            X = _random.Next(50, (int)GameCanvas.Width - 50),
            Y = _random.Next(50, (int)GameCanvas.Height - 50),
            SpeedX = (float)(_random.NextDouble() * 2 - 1) * 100,
            SpeedY = (float)(_random.NextDouble() * 2 - 1) * 100,
            Size = 50 + _random.Next(20),
            IsHappy = false,
            Color = Color.Parse(catalogItem.ColorHex)
        };

        _floatingAliens.Add(alien);
        GameCanvas.Children.Add(CreateAlienControl(alien));
    }

    private AlienCatControl CreateAlienControl(FloatingAlien alien)
    {
        var control = new AlienCatControl
        {
            Width = alien.Size,
            Height = alien.Size,
            Color = alien.Color.ToString(),
            IsHappy = alien.IsHappy,
            Tag = alien
        };

        Canvas.SetLeft(control, alien.X - alien.Size / 2);
        Canvas.SetTop(control, alien.Y - alien.Size / 2);

        return control;
    }

    private void UpdateAliens()
    {
        if (GameCanvas == null) return;

        var dt = 0.016f;

        foreach (var alien in _floatingAliens.ToList())
        {
            alien.X += alien.SpeedX * dt;
            alien.Y += alien.SpeedY * dt;

            var halfSize = alien.Size / 2;
            if (alien.X < halfSize) { alien.X = halfSize; alien.SpeedX *= -1; }
            if (alien.X > GameCanvas.Width - halfSize) { alien.X = GameCanvas.Width - halfSize; alien.SpeedX *= -1; }
            if (alien.Y < halfSize) { alien.Y = halfSize; alien.SpeedY *= -1; }
            if (alien.Y > GameCanvas.Height - halfSize) { alien.Y = GameCanvas.Height - halfSize; alien.SpeedY *= -1; }

            var control = GameCanvas.Children
                .FirstOrDefault(c => c.Tag == alien) as AlienCatControl;

            if (control != null)
            {
                Canvas.SetLeft(control, alien.X - alien.Size / 2);
                Canvas.SetTop(control, alien.Y - alien.Size / 2);
            }
        }
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (GameCanvas == null) return;

        var point = e.GetPosition(GameCanvas);

        foreach (var alien in _floatingAliens.ToList())
        {
            var distance = Math.Sqrt(
                Math.Pow(point.X - alien.X, 2) +
                Math.Pow(point.Y - alien.Y, 2)
            );

            if (distance < alien.Size / 2)
            {
                var success = _viewModel.TryCaptureCreature(alien.Catalog);

                if (success)
                {
                    RemoveAlien(alien);
                }
                else
                {
                    _viewModel.GameStatusText = "😅 Твой приют переполнен! Отпусти кого-нибудь в коллекции.";
                }
                break;
            }
        }
    }

    private void RemoveAlien(FloatingAlien alien)
    {
        if (GameCanvas == null) return;

        var control = GameCanvas.Children
            .FirstOrDefault(c => c.Tag == alien) as AlienCatControl;

        if (control != null)
        {
            GameCanvas.Children.Remove(control);
        }

        _floatingAliens.Remove(alien);
    }

    private async void OnCreatureJumped(CapturedCreature creature)
    {
        _viewModel.GameStatusText = $"🌈 {creature.Nickname} подпрыгнул от радости! ✨";

        foreach (var alien in _floatingAliens)
        {
            if (alien.Catalog.Id == creature.CatalogId)
            {
                var control = GameCanvas.Children
                    .FirstOrDefault(c => c.Tag == alien) as AlienCatControl;
                if (control != null)
                {
                    await ShowHearts(control);
                }
                break;
            }
        }
    }

    // ============ ИГРА "ПОЙМАЙ ЗВЕЗДУ" ============

    private readonly List<FallingStar> _gameStars = new();
    private DispatcherTimer? _gameTimer;
    private DispatcherTimer? _gameSpawnTimer;
    private int _gameScore = 0;
    private int _gameTimeLeft = 30;
    private bool _isGamePlaying = false;

    private void InitStarGame()
    {
        StarGameCanvas.PointerPressed += OnGameCanvasPointerPressed;
        GameStartButton.Click += OnGameStartClick;

        // Обновляем таймеры UI
        var scoreText = this.FindControl<TextBlock>("GameScoreText");
        var timerText = this.FindControl<TextBlock>("GameTimerText");

        UpdateGameUI();
    }

    private void OnGameStartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isGamePlaying)
        {
            StopGame();
        }
        else
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        _isGamePlaying = true;
        _gameScore = 0;
        _gameTimeLeft = 30;
        _gameStars.Clear();
        StarGameCanvas.Children.Clear();
        GameStartButton.Content = "⏹️ Стоп";
        UpdateGameUI();

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _gameTimer.Tick += (s, e) => OnGameTimerTick();
        _gameTimer.Start();

        _gameSpawnTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(800)
        };
        _gameSpawnTimer.Tick += (s, e) => SpawnGameStar();
        _gameSpawnTimer.Start();

        var updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        updateTimer.Tick += (s, e) => UpdateGameStars();
        updateTimer.Start();
        _gameUpdateTimer = updateTimer;
    }

    private void StopGame()
    {
        _isGamePlaying = false;
        _gameTimer?.Stop();
        _gameSpawnTimer?.Stop();
        _gameUpdateTimer?.Stop();

        GameStartButton.Content = "▶️ Начать";

        foreach (var star in _gameStars)
        {
            StarGameCanvas.Children.Remove(star.Control);
        }
        _gameStars.Clear();
    }

    private void OnGameTimerTick()
    {
        _gameTimeLeft--;
        UpdateGameUI();

        if (_gameTimeLeft <= 0)
        {
            StopGame();
            _viewModel.AddStarDust(_gameScore);
            _viewModel.GameStatusText = $"🌟 Ты поймал {_gameScore} звёзд! +{_gameScore} ✨";
        }
    }

    private void SpawnGameStar()
    {
        if (!_isGamePlaying || StarGameCanvas == null) return;

        var size = 20 + _random.Next(15);
        var x = _random.Next(20, (int)StarGameCanvas.Width - 20);
        var speed = 1.5 + _random.NextDouble() * 2;

        var star = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromRgb(
                (byte)_random.Next(200, 255),
                (byte)_random.Next(200, 255),
                (byte)_random.Next(50, 100)
            )),
            Opacity = 1,
            Tag = _gameStars.Count
        };

        var fallingStar = new FallingStar
        {
            Control = star,
            X = x,
            Y = -size,
            Speed = speed,
            Size = size
        };

        _gameStars.Add(fallingStar);

        Canvas.SetLeft(star, fallingStar.X);
        Canvas.SetTop(star, fallingStar.Y);
        StarGameCanvas.Children.Add(star);
    }

    private void UpdateGameStars()
    {
        if (!_isGamePlaying || StarGameCanvas == null) return;

        foreach (var star in _gameStars.ToList())
        {
            star.Y += star.Speed;

            Canvas.SetTop(star.Control, star.Y);
            Canvas.SetLeft(star.Control, star.X);

            if (star.Y > StarGameCanvas.Height + 50)
            {
                StarGameCanvas.Children.Remove(star.Control);
                _gameStars.Remove(star);
            }
        }
    }

    private void OnGameCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_isGamePlaying) return;

        var point = e.GetPosition(StarGameCanvas);

        for (int i = _gameStars.Count - 1; i >= 0; i--)
        {
            var star = _gameStars[i];

            var distance = Math.Sqrt(
                Math.Pow(point.X - star.X, 2) +
                Math.Pow(point.Y - star.Y, 2)
            );

            if (distance < star.Size / 2)
            {

                StarGameCanvas.Children.Remove(star.Control);
                _gameStars.RemoveAt(i);
                _gameScore++;

                _viewModel.AddStarDust(1);

                UpdateGameUI();


                var flash = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new SolidColorBrush(Color.FromRgb(255, 255, 200)),
                    Opacity = 0.8
                };
                Canvas.SetLeft(flash, star.X - 15);
                Canvas.SetTop(flash, star.Y - 15);
                StarGameCanvas.Children.Add(flash);

                Dispatcher.UIThread.Post(async () =>
                {
                    await Task.Delay(150);
                    StarGameCanvas.Children.Remove(flash);
                });

                break;
            }
        }
    }

    private void UpdateGameUI()
    {
        var scoreText = this.FindControl<TextBlock>("GameScoreText");
        var timerText = this.FindControl<TextBlock>("GameTimerText");

        if (scoreText != null) scoreText.Text = _gameScore.ToString();
        if (timerText != null) timerText.Text = _gameTimeLeft.ToString();
    }

    // ============ ИГРА "МЕМОРИ" ============

    private List<MemoryCard> _memoryCards = new();
    private MemoryCard? _firstSelected;
    private MemoryCard? _secondSelected;
    private bool _isMemoryLocked = false;
    private int _memoryPairsFound = 0;
    private int _memoryMoves = 0;
    private int _totalMemoryPairs = 16;
    private readonly string[] _memoryEmojis = {
    "🐱", "✨", "🌿", "🐈", "🌙", "🌈",
    "⭐", "🌸", "🍀", "🌺", "🦋", "🌊",
    "🍄", "🌻", "🐝", "🪐"
};

    private void InitMemoryGame()
    {
        MemoryStartButton.Click += OnMemoryStartClick;
        StartNewMemoryGame();
    }

    private void OnMemoryStartClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StartNewMemoryGame();
    }

    private void StartNewMemoryGame()
    {
        MemoryCardGrid.Children.Clear();
        _memoryCards.Clear();
        _firstSelected = null;
        _secondSelected = null;
        _isMemoryLocked = false;
        _memoryPairsFound = 0;
        _memoryMoves = 0;
        UpdateMemoryUI();

        // Создаём пары карточек (16 пар = 32 карточки)
        var cardList = new List<string>();
        for (int i = 0; i < _totalMemoryPairs; i++)
        {
            cardList.Add(_memoryEmojis[i % _memoryEmojis.Length]);
            cardList.Add(_memoryEmojis[i % _memoryEmojis.Length]);
        }

        // Перемешиваем
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        }

        // Заполняем сетку 4 строки x 8 колонок = 32 карточки
        int index = 0;
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var card = new MemoryCard
                {
                    Emoji = cardList[index],
                    IsFlipped = false,
                    IsMatched = false
                };

                var button = new Button
                {
                    Content = "❓",
                    FontSize = 34,
                    Background = new SolidColorBrush(Color.Parse("#2a2a3e")),
                    Foreground = Brushes.White,
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(5),
                    DataContext = card
                };
                button.Click += OnMemoryCardClick;

                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);
                MemoryCardGrid.Children.Add(button);

                card.Button = button;
                _memoryCards.Add(card);
                index++;
            }
        }
    }



    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }

    private async void OnMemoryCardClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isMemoryLocked) return;

        var button = sender as Button;
        if (button == null) return;

        var card = button.DataContext as MemoryCard;
        if (card == null || card.IsFlipped || card.IsMatched) return;

        card.IsFlipped = true;
        button.Content = card.Emoji;
        button.Background = new SolidColorBrush(Color.Parse("#4a6fa5"));

        if (_firstSelected == null)
        {
            _firstSelected = card;
        }
        else if (_secondSelected == null && _firstSelected != card)
        {
            _secondSelected = card;
            _memoryMoves++;
            UpdateMemoryUI();

            if (_firstSelected.Emoji == _secondSelected.Emoji)
            {
                _firstSelected.IsMatched = true;
                _secondSelected.IsMatched = true;
                _memoryPairsFound++;
                UpdateMemoryUI();

                _firstSelected.Button.Background = new SolidColorBrush(Color.Parse("#27ae60"));
                _secondSelected.Button.Background = new SolidColorBrush(Color.Parse("#27ae60"));

                _firstSelected = null;
                _secondSelected = null;

                if (_memoryPairsFound == _totalMemoryPairs)
                {
                    ShowMemoryWinDialog();
                }
            }
            else
            {
                _isMemoryLocked = true;
                await Task.Delay(500);

                _firstSelected.IsFlipped = false;
                _secondSelected.IsFlipped = false;
                _firstSelected.Button.Content = "❓";
                _firstSelected.Button.Background = new SolidColorBrush(Color.Parse("#2a2a3e"));
                _secondSelected.Button.Content = "❓";
                _secondSelected.Button.Background = new SolidColorBrush(Color.Parse("#2a2a3e"));

                _firstSelected = null;
                _secondSelected = null;
                _isMemoryLocked = false;
            }
        }
    }

    private void ShowMemoryWinDialog() // 👈 УБРАЛИ async
    {
        var score = _totalMemoryPairs * 2 - _memoryMoves;
        if (score < 0) score = 1;

        var dialog = new Window
        {
            Title = "🎉 Ты победил!",
            Width = 350,
            Height = 200,
            Background = new SolidColorBrush(Color.Parse("#1a1a2e"))
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"🎉 Ты нашёл все {_totalMemoryPairs} пар за {_memoryMoves} ходов!",
            FontSize = 18,
            Foreground = Brushes.White,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"⭐ +{score} звёздной пыли!",
            FontSize = 16,
            Foreground = Brushes.Gold,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        });

        var okButton = new Button
        {
            Content = "✨ Отлично!",
            Background = new SolidColorBrush(Color.Parse("#4a6fa5")),
            Foreground = Brushes.White,
            Padding = new Thickness(15, 8, 15, 8),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        okButton.Click += (s, e) => dialog.Close();
        stackPanel.Children.Add(okButton);

        dialog.Content = stackPanel;
        dialog.Show();

        _viewModel.AddStarDust(score);
        _viewModel.GameStatusText = $"🧠 Ты нашёл все пары! +{score} ✨";
    }

    private void UpdateMemoryUI()
    {
        var pairsText = this.FindControl<TextBlock>("MemoryPairsText");
        var movesText = this.FindControl<TextBlock>("MemoryMovesText");

        if (pairsText != null)
            pairsText.Text = $"{_memoryPairsFound}/{_totalMemoryPairs}";
        if (movesText != null)
            movesText.Text = _memoryMoves.ToString();
    }

    private class MemoryCard
    {
        public string Emoji { get; set; } = "";
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
        public Button? Button { get; set; }
    }

    private class FallingStar
    {
        public Avalonia.Controls.Shapes.Ellipse Control { get; set; } = new();
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public double Size { get; set; }
    }

    private class FloatingAlien
    {
        public CreatureCatalog Catalog { get; set; } = new();
        public double X { get; set; }
        public double Y { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }
        public double Size { get; set; }
        public bool IsHappy { get; set; }
        public Color Color { get; set; }
    }
}