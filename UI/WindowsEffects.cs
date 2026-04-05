// File: UI/WindowEffects.cs
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MiniFlyout.UI
{
    public static class WindowEffects
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // --- NEW API CALLS FOR FULLSCREEN DETECTION ---
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        // ----------------------------------------------

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public static void EnableAmbientAcrylic(IntPtr handle, Color ambientColor, byte opacity = 0x60)
        {
            var accent = new AccentPolicy
            {
                AccentState = 4, 
                GradientColor = unchecked((opacity << 24) | (ambientColor.B << 16) | (ambientColor.G << 8) | ambientColor.R)
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = 19, 
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        public static void ApplyNativeRoundedCorners(IntPtr handle)
        {
            int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
            int cornerPref = 3; 
            DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));
        }

        public static void EnforceTopMost(IntPtr handle)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        public static void ShowOverlay(IntPtr handle) => ShowWindow(handle, 4); 
        
        public static void HideOverlay(IntPtr handle) => ShowWindow(handle, 0); 

        // --- THE SMART FULLSCREEN DETECTOR ---
        public static bool IsForegroundFullScreen()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero) return false;

                IntPtr desktopWindow = GetDesktopWindow();
                IntPtr shellWindow = GetShellWindow();

                // If the user is just staring at their empty desktop, it's not a full-screen app
                if (foregroundWindow == desktopWindow || foregroundWindow == shellWindow)
                    return false;

                GetWindowRect(foregroundWindow, out RECT rect);
                
                var screen = System.Windows.Forms.Screen.FromHandle(foregroundWindow);

                // If you have multiple monitors, we only want to hide the widget if the 
                // full-screen app is playing on the Primary Screen (where the widget lives!)
                if (!screen.Primary) return false;
                
                // Check if the focused window's dimensions perfectly match or exceed the screen boundaries
                return (rect.Left <= screen.Bounds.Left &&
                        rect.Top <= screen.Bounds.Top &&
                        rect.Right >= screen.Bounds.Right &&
                        rect.Bottom >= screen.Bounds.Bottom);
            }
            catch
            {
                return false;
            }
        }
    }
}