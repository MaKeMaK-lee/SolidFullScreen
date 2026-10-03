using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SolidFullScreen;

public partial class MainWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_SHOWWINDOW = 0x0040;

    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    private static readonly Brush Fill = new SolidColorBrush(Color.FromRgb(30, 144, 255));

    private bool _isFullscreen;
    private Rect _savedBounds;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Клик — развернуть на все мониторы";
        Width = 400;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Fill;
        MouseLeftButtonDown += (_, __) => Toggle();
    }

    private void Toggle()
    {
        if (_isFullscreen)
            ExitFullscreen();
        else
            EnterFullscreen();
    }

    private void EnterFullscreen()
    {
        _isFullscreen = true;
        _savedBounds = new Rect(Left, Top, Width, Height);

        // Меняем стиль ДО получения HWND — при этом WPF пересоздаёт окно
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ShowInTaskbar = false;

        var hwnd = new WindowInteropHelper(this).Handle;
        int x = GetSystemMetrics(SM_XVIRTUALSCREEN);
        int y = GetSystemMetrics(SM_YVIRTUALSCREEN);
        int w = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        int h = GetSystemMetrics(SM_CYVIRTUALSCREEN);

        // Растягиваем на весь виртуальный рабочий стол
        SetWindowPos(hwnd, HWND_TOPMOST, x, y, w, h, SWP_SHOWWINDOW);
    }

    private void ExitFullscreen()
    {
        _isFullscreen = false;
        Topmost = false;
        WindowStyle = WindowStyle.SingleBorderWindow;
        ResizeMode = ResizeMode.CanResize;
        ShowInTaskbar = true;

        Left = _savedBounds.Left;
        Top = _savedBounds.Top;
        Width = _savedBounds.Width;
        Height = _savedBounds.Height;
    }
}