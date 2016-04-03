using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Carpet
{
    public class SystemTray
    {
        private readonly Window _window;
        private NotifyIcon _sysTrayIcon;

        private static readonly string BaseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public SystemTray(Window window)
        {
            _window = window;
            _window.Closing += Window_Closing;
            _window.StateChanged += Window_StateChanged;
            CreateIcon();
        }

        public void CreateIcon()
        {
            _sysTrayIcon = new NotifyIcon();
            _sysTrayIcon.Text = @"Carpet";
            _sysTrayIcon.Icon = new System.Drawing.Icon(Path.Combine(BaseDir, @"carpet.ico"), 40, 40);
            _sysTrayIcon.Visible = true;
            _sysTrayIcon.DoubleClick += _sysTrayIcon_DoubleClick;
        }

        private void _sysTrayIcon_DoubleClick(object sender, System.EventArgs e)
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.ShowInTaskbar = true;
                _window.WindowState = WindowState.Normal;
            }
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.ShowInTaskbar = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _sysTrayIcon.Dispose();
        }
    }
}
