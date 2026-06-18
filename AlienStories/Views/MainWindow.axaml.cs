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

    public MainWindow()
    {
        InitializeComponent();

        _db = new DatabaseService();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        // Подписываемся на событие прыжка
        _viewModel.CreatureJumped += OnCreatureJumped;

        // Подписываемся на событие обновления коллекции
        _viewModel.CollectionUpdated += OnCollectionUpdated;

        DrawStars();
        GameCanvas.PointerPressed += OnCanvasPointerPressed;
        StartGameLoop();

        // Создаём начальных существ
        SpawnInitialAliens();
    }

    private void SpawnInitialAliens()
    {
        // Проверяем, сколько мест свободно
        var freeSlots = 10 - _viewModel.FriendsCount;
        var count = Math.Min(3, freeSlots); // максимум 3, но не больше свободных мест

        for (int i = 0; i < count; i++)
        {
            SpawnAlien();
        }

        // Если мест нет, показываем сообщение
        if (freeSlots <= 0)
        {
            _viewModel.GameStatusText = "😅 В приюте нет мест! Отпусти кого-нибудь, чтобы пригласить нового друга.";
        }
    }

    private void OnCollectionUpdated()
    {
        // Очищаем всех существ с экрана
        foreach (var alien in _floatingAliens.ToList())
        {
            RemoveAlien(alien);
        }

        // Создаём новых
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
            // Проверяем, есть ли свободные места
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

        // Проверяем, есть ли свободные места
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
                // Пытаемся пригласить
                var success = _viewModel.TryCaptureCreature(alien.Catalog);

                if (success)
                {
                    // Удаляем с экрана
                    RemoveAlien(alien);
                }
                else
                {
                    // Если не получилось (переполнено), показываем сообщение
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
        // Просто обновляем статус
        _viewModel.GameStatusText = $"🌈 {creature.Nickname} подпрыгнул от радости! ✨";
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