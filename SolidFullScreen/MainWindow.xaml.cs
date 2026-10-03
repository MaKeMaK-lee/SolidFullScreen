
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Runtime.InteropServices;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace SolidFullScreen
{
    /// <summary>
    /// Writed by deepseek
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_SHOWWINDOW = 0x0040;

        // Цвет заливки
        private static readonly System.Windows.Media.Brush MainColor =
            new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));

        private readonly List<Window> _extraWindows = new();
        private bool _isFullscreen;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Solid Full Screens";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = MainColor;
            MouseLeftButtonDown += OnAnyClick;
        }

        private void OnAnyClick(object sender, MouseButtonEventArgs e)
        {
            if (_isFullscreen)
                ExitFullscreen();
            else
                EnterFullscreen();
        }

        private void EnterFullscreen()
        {
            _isFullscreen = true;

            var primary = WinForms.Screen.PrimaryScreen!;
            var others = WinForms.Screen.AllScreens.Where(s => !s.Equals(primary)).ToList();

            // Главное окно — на основной монитор
            MakeFullscreenOn(this, primary);

            // Отдельные окна для остальных мониторов
            foreach (var screen in others)
            {
                var grid = new Grid { Background = MainColor };
                grid.MouseLeftButtonDown += OnAnyClick;

                var w = new Window
                {
                    Content = grid,
                    Background = MainColor,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                w.Show();
                MakeFullscreenOn(w, screen);
                _extraWindows.Add(w);
            }
        }

        private void ExitFullscreen()
        {
            _isFullscreen = false;

            // Закрываем вспомогательные окна асинхронно,
            // чтобы не уничтожать окно прямо внутри его же обработчика клика
            var toClose = _extraWindows.ToList();
            _extraWindows.Clear();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var w in toClose)
                    w.Close();
            }));

            // Возвращаем главное окно в обычное состояние
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            Topmost = false;
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Width = 400;
            Height = 300;
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }

        private static void MakeFullscreenOn(Window w, WinForms.Screen screen)
        {
            w.WindowState = WindowState.Normal;
            w.WindowStyle = WindowStyle.None;
            w.ResizeMode = ResizeMode.NoResize;
            w.Topmost = true;
            w.ShowInTaskbar = false;

            // Позиционируем через WinAPI в физических пикселях —
            // это корректно работает при любом DPI и на любом мониторе
            var hwnd = new WindowInteropHelper(w).Handle;
            var b = screen.Bounds;
            SetWindowPos(hwnd, HWND_TOPMOST, b.Left, b.Top, b.Width, b.Height, SWP_SHOWWINDOW);
        }
    }

}