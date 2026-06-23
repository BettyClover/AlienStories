using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AlienStories.Views;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
        StartButton.Click += OnStartClick;
    }

    private void OnStartClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}