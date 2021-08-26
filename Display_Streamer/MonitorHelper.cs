using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;

namespace Display_Streamer
{

    public class DeviceInfo
    {
        public string DeviceName { get; set; }
        public int VerticalResolution { get; set; }
        public int HorizontalResolution { get; set; }
        public Rectangle MonitorArea { get; set; }
    }

    class MonitorHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct MONITORINFOEX
        {
            public int Size;
            public Rect Monitor;
            public Rect WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }


        private static List<DeviceInfo> devices;

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("User32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        public enum DeviceCap
        {
            DektopVertRes = 117,
            DesktopHorzRes = 118
        }

        public MonitorHelper()
        {
        }

        public static List<DeviceInfo> getScreenDevices()
        {
            devices = new List<DeviceInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);
            return devices;
        }

        private static bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            var monitorInfo = new MONITORINFOEX();
            monitorInfo.Size = Marshal.SizeOf(typeof(MONITORINFOEX));

            bool success = GetMonitorInfo(hMonitor, ref monitorInfo);
            if (success)
            {
                var dc = CreateDC(monitorInfo.DeviceName, monitorInfo.DeviceName, null, IntPtr.Zero);
                var di = new DeviceInfo
                {
                    DeviceName = monitorInfo.DeviceName,
                    MonitorArea = new Rectangle(monitorInfo.Monitor.left, monitorInfo.Monitor.top, monitorInfo.Monitor.right - monitorInfo.Monitor.right, monitorInfo.Monitor.bottom - monitorInfo.Monitor.top),
                    VerticalResolution = GetDeviceCaps(dc, (int)DeviceCap.DektopVertRes),
                    HorizontalResolution = GetDeviceCaps(dc, (int)DeviceCap.DesktopHorzRes)
                };
                ReleaseDC(IntPtr.Zero, dc);
                devices.Add(di);

            }
            return true;
        }
    }
}
