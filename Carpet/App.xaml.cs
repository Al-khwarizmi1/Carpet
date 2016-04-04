using System.Linq;
using System.Windows;

namespace Carpet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            if (e.Args.Any(_ => _ == "-silent"))
            {
                window.WindowState = WindowState.Minimized;
                window.ShowInTaskbar = false;
            }

            if (e.Args.Any(_ => _ == "-autostart"))
            {
                window.EnsureAppIsInStartup();
            }

            window.Show();
        }
    }
}
