// File: Infrastructure/AutorunManager.cs
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace MiniFlyout.Infrastructure
{
    public static class AutorunManager
    {
        private const string AppName = "MiniFlyout";

        public static bool IsAutorunEnabled()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutorun(bool enable)
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (enable)
                {
                    // Wrap the path in quotes to prevent issues with spaces in folder names
                    key.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update startup settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}