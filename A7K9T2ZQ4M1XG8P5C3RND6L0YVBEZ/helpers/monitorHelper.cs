using A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.windows.administration;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace A7K9T2ZQ4M1XG8P5C3RND6L0YVBEZ.Helpers
{
    public static class MonitorHelper
    {
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor,
            ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        public static IReadOnlyList<Rect> GetMonitorWorkAreas()
        {
            var result = new List<Rect>();

            EnumDisplayMonitors(
    IntPtr.Zero,
    IntPtr.Zero,
    (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT rcMonitor, IntPtr dwData) =>
    {
        var info = new MONITORINFOEX
        {
            cbSize = Marshal.SizeOf(typeof(MONITORINFOEX))
        };

        if (GetMonitorInfo(hMonitor, ref info))
        {
            var r = info.rcMonitor; // HELE skjermen, ikke bare work area
            result.Add(new Rect(
                r.Left,
                r.Top,
                r.Right - r.Left,
                r.Bottom - r.Top
            ));
        }

        return true;
    },
    IntPtr.Zero);


            return result;
        }
    }
    public static class SelectAndFillMonitor
    {
        public static void ShowFullScreenOnConfiguredScreen(Window window)
        {
            var monitors = MonitorHelper.GetMonitorWorkAreas();

            var screenIndex = AppSettings.TargetEmployeeScreenIndex;
            if (screenIndex < 0 || screenIndex >= monitors.Count)
                screenIndex = 0;

            var target = monitors[screenIndex];

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = target.Left;
            window.Top = target.Top;

            window.Width = target.Width;
            window.Height = target.Height;

            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.NoResize;
            window.Topmost = true;
        }
    }
}
