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

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private System.Timers.Timer? _hungerTimer;

    [ObservableProperty]
    private ObservableCollection<CapturedCreature> _capturedCreatures = new();

    [ObservableProperty]
    private string _gameStatusText = "Нажми на одинокого друга, чтобы пригласить его! ✨";

    [ObservableProperty]
    private int _friendsCount;

    [ObservableProperty]
    private string _friendsLimitText = "👥 0 / 10";

    [ObservableProperty]
    private bool _isGameActive = true;

    [ObservableProperty]
    private bool _isCollectionActive;

    public ICommand SwitchToGameCommand { get; }
    public ICommand SwitchToCollectionCommand { get; }
    public ICommand HugCommand { get; }
    public ICommand FeedCommand { get; }
    public ICommand StoryCommand { get; }
    public ICommand ReleaseCommand { get; }

    public event Action<CapturedCreature>? CreatureJumped;
    public event Action? CollectionUpdated;

    public MainWindowViewModel()
    {
        _db = new DatabaseService();
        LoadCollection();

        SwitchToGameCommand = new RelayCommand(SwitchToGame);
        SwitchToCollectionCommand = new RelayCommand(SwitchToCollection);
        HugCommand = new RelayCommand<CapturedCreature>(HugCreature);
        FeedCommand = new RelayCommand<CapturedCreature>(FeedCreature);
        StoryCommand = new RelayCommand<CapturedCreature>(TellStory);
        ReleaseCommand = new RelayCommand<CapturedCreature>(ReleaseCreature);

        StartHungerTimer();
    }

    private void StartHungerTimer()
    {
        _hungerTimer = new System.Timers.Timer();
        _hungerTimer.Interval = 60000;
        _hungerTimer.Elapsed += (s, e) => DecreaseHunger();
        _hungerTimer.Start();
    }

    private void DecreaseHunger()
    {
        var all = _db.GetAllCaptured();

        foreach (var creature in all)
        {
            creature.Hunger = Math.Max(0, creature.Hunger - 5);
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

    private void LoadCollection()
    {
        var all = _db.GetAllCaptured();
        CapturedCreatures = new ObservableCollection<CapturedCreature>(all);
        FriendsCount = CapturedCreatures.Count;
        FriendsLimitText = $"👥 {FriendsCount} / 10";
    }

    private void SwitchToGame()
    {
        IsGameActive = true;
        IsCollectionActive = false;
    }

    private void SwitchToCollection()
    {
        IsGameActive = false;
        IsCollectionActive = true;
        LoadCollection();
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

        GameStatusText = $"🎉 Ты пригласил {catalog.Name}! У тебя теперь {FriendsCount} друзей!";
        CollectionUpdated?.Invoke();
        return true;
    }

    private void HugCreature(CapturedCreature? creature)
    {
        if (creature == null) return;

        creature.TimesHugged++;
        creature.LastHugged = DateTime.Now;
        _db.UpdateCreature(creature);
        LoadCollection();

        CreatureJumped?.Invoke(creature);
        GameStatusText = $"🤗 Ты обнял {creature.Nickname}! ❤️";
    }

    private void FeedCreature(CapturedCreature? creature)
    {
        if (creature == null) return;

        creature.Hunger = Math.Min(100, creature.Hunger + 20);
        creature.TimesFed++;
        creature.LastFed = DateTime.Now;
        _db.UpdateCreature(creature);
        LoadCollection();

        CreatureJumped?.Invoke(creature);
        GameStatusText = $"🍎 Ты покормил {creature.Nickname}! 🌟";
    }

    private void TellStory(CapturedCreature? creature)
    {
        if (creature == null || creature.Catalog == null) return;

        creature.TimesHeardStory++;
        _db.UpdateCreature(creature);
        LoadCollection();

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
            Content = "✅ Да, отпустить",
            Background = new SolidColorBrush(Color.Parse("#e74c3c")),
            Foreground = Brushes.White,
            Padding = new Thickness(15, 8, 15, 8)
        };
        yesButton.Click += (s, e) =>
        {
            _db.DeleteCaptured(creature.Id);
            LoadCollection();
            GameStatusText = $"🕊️ Ты отпустил {creature.Nickname} на свободу. Он будет помнить тебя! ✨";
            CollectionUpdated?.Invoke();
            confirm.Close();
        };

        var noButton = new Button
        {
            Content = "❌ Нет, оставить",
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