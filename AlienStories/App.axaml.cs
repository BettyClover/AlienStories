using AlienStories.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AlienStories
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Создаём главное окно
                var mainWindow = new MainWindow
                {
                    DataContext = new ViewModels.MainWindowViewModel(),
                };
                desktop.MainWindow = mainWindow;
                mainWindow.Show();

                // Показываем приветственное окно поверх главного
                var welcomeWindow = new WelcomeWindow();
                welcomeWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}