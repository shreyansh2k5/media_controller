// File: UI/TrayManager.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using MiniFlyout.Infrastructure;

namespace MiniFlyout.UI
{
    public class TrayManager : IDisposable
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _menu;
        private readonly Form _mainForm;

        public TrayManager(Form mainForm)
        {
            _mainForm = mainForm;
            
            // 1. Build the Dark Theme Menu
            _menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.White,
                ShowImageMargin = false,
                ShowCheckMargin = true
            };

            var autorunItem = new ToolStripMenuItem("Start with Windows")
            {
                CheckOnClick = true,
                Checked = AutorunManager.IsAutorunEnabled()
            };
            autorunItem.CheckedChanged += (s, e) => AutorunManager.SetAutorun(autorunItem.Checked);

            var exitItem = new ToolStripMenuItem("Exit MiniFlyout");
            exitItem.Click += (s, e) => 
            {
                // Added '?' to safely satisfy the compiler's null-reference warning
                if (_trayIcon != null)
                {
                    _trayIcon.Visible = false;
                }
                Application.Exit();
            };

            _menu.Items.Add(autorunItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(exitItem);

            // 2. Initialize the System Tray Icon
            _trayIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? Application.ExecutablePath) ?? SystemIcons.Application,
                ContextMenuStrip = _menu,
                Text = "MiniFlyout - Media Controller",
                Visible = true
            };
            
            // 3. Bind the right-click menu to the main widget as well
            AssignMenuToControl(_mainForm, _menu);
        }

        private void AssignMenuToControl(Control ctrl, ContextMenuStrip menu)
        {
            ctrl.ContextMenuStrip = menu;
            foreach (Control child in ctrl.Controls) 
            {
                AssignMenuToControl(child, menu);
            }
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _menu?.Dispose();
        }
    }
}