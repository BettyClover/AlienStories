using AlienStories.Models;
using AlienStories.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AlienStories.ViewModels;

public class AlbumItem
{
    public CreatureCatalog Catalog { get; set; } = new();
    public bool IsCaptured { get; set; }
    public CapturedCreature? CapturedCreature { get; set; }

    public string DisplayColor => IsCaptured ? Catalog.ColorHex : "#1a1a2e";
    public string DisplayName => IsCaptured ? Catalog.Name : "???";
    public string DisplayStatus => IsCaptured ? "✅ Пойман" : "❌ Не пойман";
    public string DisplayForeground => IsCaptured ? "White" : "#555";
    public string RarityName => Catalog.Rarity switch
    {
        1 => "Обычный",
        2 => "Необычный",
        3 => "Редкий",
        4 => "Легендарный",
        _ => "Неизвестно"
    };
    public string RarityColor => Catalog.Rarity switch
    {
        1 => "#4CAF50",
        2 => "#2196F3",
        3 => "#9C27B0",
        4 => "#FFD700",
        _ => "#888"
    };
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private System.Timers.Timer? _hungerTimer;
    private System.Timers.Timer? _giftTimer;
    private readonly Random _random = new();

    [ObservableProperty]
    private ObservableCollection<CapturedCreature> _capturedCreatures = new();

    [ObservableProperty]
    private ObservableCollection<AlbumItem> _albumItems = new();

    [ObservableProperty]
    private string _gameStatusText = "Нажми на одинокого друга, чтобы пригласить его! ✨";

    [ObservableProperty]
    private string _albumStatusText = "📖 Собери всех инопланетян!";

    [ObservableProperty]
    private int _friendsCount;

    [ObservableProperty]
    private string _friendsLimitText = "👥 0 / 10";

    [ObservableProperty]
    private bool _isGameActive = true;

    [ObservableProperty]
    private bool _isCollectionActive;

    [ObservableProperty]
    private bool _isAlbumActive;

    [ObservableProperty]
    private bool _isStarCatchActive;

    [ObservableProperty]
    private bool _isMemoryActive; // 👈 ДОБАВЛЕНО

    [ObservableProperty]
    private int _starDust;

    [ObservableProperty]
    private string _starDustText = "✨ 0";

    public ICommand SwitchToGameCommand { get; }
    public ICommand SwitchToCollectionCommand { get; }
    public ICommand SwitchToAlbumCommand { get; }
    public ICommand SwitchToStarCatchCommand { get; }
    public ICommand SwitchToMemoryCommand { get; } // 👈 ДОБАВЛЕНО
    public ICommand HugCommand { get; }
    public ICommand FeedCommand { get; }
    public ICommand StoryCommand { get; }
    public ICommand ReleaseCommand { get; }
    public ICommand SuperFeedCommand { get; }

    public event Action<CapturedCreature>? CreatureJumped;
    public event Action<CapturedCreature>? CreatureHugged;
    public event Action? CollectionUpdated;

    public MainWindowViewModel()
    {
        _db = new DatabaseService();

        StarDust = _db.GetStarDust();
        StarDustText = $"✨ {StarDust}";

        LoadCollection();
        LoadAlbum();

        SwitchToGameCommand = new RelayCommand(SwitchToGame);
        SwitchToCollectionCommand = new RelayCommand(SwitchToCollection);
        SwitchToAlbumCommand = new RelayCommand(SwitchToAlbum);
        SwitchToStarCatchCommand = new RelayCommand(SwitchToStarCatch);
        SwitchToMemoryCommand = new RelayCommand(SwitchToMemory); // 👈 ДОБАВЛЕНО
        HugCommand = new RelayCommand<CapturedCreature>(HugCreature);
        FeedCommand = new RelayCommand<CapturedCreature>(FeedCreature);
        StoryCommand = new RelayCommand<CapturedCreature>(TellStory);
        ReleaseCommand = new RelayCommand<CapturedCreature>(ReleaseCreature);
        SuperFeedCommand = new RelayCommand(SuperFeed);

        StartHungerTimer();
        StartGiftTimer();
    }

    private void StartHungerTimer()
    {
        _hungerTimer = new System.Timers.Timer();
        _hungerTimer.Interval = 10000;
        _hungerTimer.Elapsed += (s, e) => DecreaseHunger();
        _hungerTimer.Start();
    }

    private void DecreaseHunger()
    {
        var all = _db.GetAllCaptured();

        foreach (var creature in all)
        {
            creature.Hunger = Math.Max(0, creature.Hunger - 1);
            _db.UpdateCreature(creature);
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LoadCollection();

            var hungry = all.Any(c => c.Hunger < 30);
            if (hungry)
            {
                GameStatusText = "😢 Кто-то из друзей проголодался! Покорми их!";
            }
        });
    }

    private void StartGiftTimer()
    {
        _giftTimer = new System.Timers.Timer();
        _giftTimer.Interval = 30000 + _random.Next(30000);
        _giftTimer.Elapsed += (s, e) => GiveGift();
        _giftTimer.Start();
    }

    private void GiveGift()
    {
        var all = _db.GetAllCaptured();
        if (all == null || all.Count == 0) return;

        var creature = all[_random.Next(all.Count)];
        var giftType = _random.Next(3);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            switch (giftType)
            {
                case 0:
                    AddStarDust(5);
                    GameStatusText = $"🎁 {creature.Nickname} подарил тебе 5 ✨!";
                    break;
                case 1:
                    AddStarDust(10);
                    GameStatusText = $"🎁 {creature.Nickname} подарил тебе 10 ✨!";
                    break;
                case 2:
                    GameStatusText = $"💕 {creature.Nickname} говорит: 'Ты мой самый лучший друг!'";
                    break;
            }
        });

        _giftTimer.Interval = 30000 + _random.Next(30000);
        _giftTimer.Start();
    }

    private void LoadCollection()
    {
        var all = _db.GetAllCaptured();
        CapturedCreatures = new ObservableCollection<CapturedCreature>(all);
        FriendsCount = CapturedCreatures.Count;
        FriendsLimitText = $"👥 {FriendsCount} / 10";
    }

    private void LoadAlbum()
    {
        var allCatalog = _db.GetAllCatalog();
        var captured = _db.GetAllCaptured();

        AlbumItems = new ObservableCollection<AlbumItem>();

        foreach (var catalog in allCatalog)
        {
            var capturedItem = captured.FirstOrDefault(c => c.CatalogId == catalog.Id);
            AlbumItems.Add(new AlbumItem
            {
                Catalog = catalog,
                IsCaptured = capturedItem != null,
                CapturedCreature = capturedItem
            });
        }

        var total = AlbumItems.Count;
        var capturedCount = AlbumItems.Count(a => a.IsCaptured);
        AlbumStatusText = $"📖 Собрано: {capturedCount} из {total}";
    }

    private void SwitchToGame()
    {
        IsGameActive = true;
        IsCollectionActive = false;
        IsAlbumActive = false;
        IsStarCatchActive = false;
        IsMemoryActive = false;
    }

    private void SwitchToCollection()
    {
        IsGameActive = false;
        IsCollectionActive = true;
        IsAlbumActive = false;
        IsStarCatchActive = false;
        IsMemoryActive = false;
        LoadCollection();
    }

    private void SwitchToAlbum()
    {
        IsGameActive = false;
        IsCollectionActive = false;
        IsAlbumActive = true;
        IsStarCatchActive = false;
        IsMemoryActive = false;
        LoadAlbum();
    }

    private void SwitchToStarCatch()
    {
        IsGameActive = false;
        IsCollectionActive = false;
        IsAlbumActive = false;
        IsStarCatchActive = true;
        IsMemoryActive = false;
    }

    private void SwitchToMemory()
    {
        IsGameActive = false;
        IsCollectionActive = false;
        IsAlbumActive = false;
        IsStarCatchActive = false;
        IsMemoryActive = true;
    }

    public void AddStarDust(int amount)
    {
        StarDust += amount;
        StarDustText = $"✨ {StarDust}";
        _db.SaveStarDust(StarDust);
    }

    private void SuperFeed()
    {
        if (StarDust < 20)
        {
            GameStatusText = "😅 Нужно 20 ✨ для супер-кормёжки!";
            return;
        }

        var all = _db.GetAllCaptured();
        var hungry = all.Any(c => c.Hunger < 100);

        if (!hungry)
        {
            GameStatusText = "🌟 Все друзья уже сыты!";
            return;
        }

        StarDust -= 20;
        StarDustText = $"✨ {StarDust}";
        _db.SaveStarDust(StarDust);

        foreach (var creature in all)
        {
            creature.Hunger = 100;
            creature.TimesFed++;
            creature.LastFed = DateTime.Now;
            _db.UpdateCreature(creature);
        }

        LoadCollection();
        GameStatusText = "🌟 Все друзья сыты и счастливы! -20 ✨";
    }

    public bool TryCaptureCreature(CreatureCatalog catalog)
    {
        if (CapturedCreatures.Count >= 10)
        {
            return false;
        }
        var newFriend = new CapturedCreature
        {
            CatalogId = catalog.Id,
            Nickname = catalog.Name,
            CaptureDate = DateTime.Now,
            Size = 0.8 + new Random().NextDouble() * 0.4,
            IsShiny = new Random().Next(100) < 10,
            Hunger = 100
        };

        _db.AddCaptured(newFriend);
        LoadCollection();
        LoadAlbum();

        AddStarDust(10);
        GameStatusText = $"🎉 Ты пригласил {catalog.Name}! +10 ✨";

        return true;
    }

    private void HugCreature(CapturedCreature? creature)
    {
        if (creature == null) return;

        creature.TimesHugged++;
        creature.LastHugged = DateTime.Now;
        _db.UpdateCreature(creature);
        LoadCollection();

        AddStarDust(5);
        CreatureHugged?.Invoke(creature);
        GameStatusText = $"🤗 Ты обнял {creature.Nickname}! +5 ✨";
    }

    private void FeedCreature(CapturedCreature? creature)
    {
        if (creature == null) return;

        creature.Hunger = Math.Min(100, creature.Hunger + 20);
        creature.TimesFed++;
        creature.LastFed = DateTime.Now;
        _db.UpdateCreature(creature);
        LoadCollection();

        AddStarDust(5);
        CreatureJumped?.Invoke(creature);
        GameStatusText = $"🍎 Ты покормил {creature.Nickname}! +5 ✨";
    }

    private void TellStory(CapturedCreature? creature)
    {
        if (creature == null || creature.Catalog == null) return;

        creature.TimesHeardStory++;
        _db.UpdateCreature(creature);
        LoadCollection();

        AddStarDust(3);
        GameStatusText = $"📖 Ты послушал историю {creature.Nickname}! +3 ✨";

        var storyWindow = new Window();
        storyWindow.Title = $"📖 История {creature.Nickname}";
        storyWindow.Width = 400;
        storyWindow.Height = 300;
        storyWindow.Background = new SolidColorBrush(Color.Parse("#1a1a2e"));

        var storyText = new TextBlock();
        storyText.Text = creature.Catalog.Story;
        storyText.FontSize = 16;
        storyText.Foreground = Brushes.White;
        storyText.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        storyText.TextAlignment = Avalonia.Media.TextAlignment.Center;
        storyText.Margin = new Thickness(0, 0, 0, 20);

        var closeButton = new Button();
        closeButton.Content = "✨ Спасибо за историю!";
        closeButton.Background = new SolidColorBrush(Color.Parse("#4a6fa5"));
        closeButton.Foreground = Brushes.White;
        closeButton.Padding = new Thickness(15, 10, 15, 10);
        closeButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        closeButton.Click += (s, e) => storyWindow.Close();

        var stackPanel = new StackPanel();
        stackPanel.Margin = new Thickness(20);
        stackPanel.Children.Add(storyText);
        stackPanel.Children.Add(closeButton);

        storyWindow.Content = stackPanel;
        storyWindow.Show();
    }

    private void ReleaseCreature(CapturedCreature? creature)
    {
        if (creature == null) return;

        var confirm = new Window
        {
            Title = "🕊️ Отпустить друга?",
            Width = 350,
            Height = 200,
            Background = new SolidColorBrush(Color.Parse("#1a1a2e"))
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        var text = new TextBlock
        {
            Text = $"Ты уверен, что хочешь отпустить {creature.Nickname}? Он будет скучать по тебе! 💔",
            FontSize = 16,
            Foreground = Brushes.White,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15
        };

        var yesButton = new Button
        {
            Content = "✅ Да",
            Background = new SolidColorBrush(Color.Parse("#e74c3c")),
            Foreground = Brushes.White,
            Padding = new Thickness(15, 8, 15, 8)
        };
        yesButton.Click += (s, e) =>
        {
            _db.DeleteCaptured(creature.Id);
            LoadCollection();
            LoadAlbum();
            GameStatusText = $"Ты отпустил {creature.Nickname} на свободу. Он будет помнить тебя!";
            CollectionUpdated?.Invoke();
            confirm.Close();
        };

        var noButton = new Button
        {
            Content = "❌ Нет",
            Background = new SolidColorBrush(Color.Parse("#4a6fa5")),
            Foreground = Brushes.White,
            Padding = new Thickness(15, 8, 15, 8)
        };
        noButton.Click += (s, e) =>
        {
            GameStatusText = $"❤️ Ты решил оставить {creature.Nickname}!";
            confirm.Close();
        };

        buttonPanel.Children.Add(yesButton);
        buttonPanel.Children.Add(noButton);
        stackPanel.Children.Add(text);
        stackPanel.Children.Add(buttonPanel);
        confirm.Content = stackPanel;
        confirm.Show();
    }
}