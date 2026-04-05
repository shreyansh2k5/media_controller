// File: UI/MainForm.cs
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniFlyout.Core;
using MiniFlyout.Infrastructure;

namespace MiniFlyout.UI
{
    public class MainForm : Form
    {
        private readonly IMediaService _mediaService;
        private Point _dragStart;
        private ToolTip _trackInfoToolTip = null!;
        
        // Explicitly declared as WinForms Timer to avoid System.Threading ambiguity
        private System.Windows.Forms.Timer _topMostEnforcer = null!;

        // Constructor Injection
        public MainForm(IMediaService mediaService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            
            InitUI();
            EnableDrag();
            EnableTopMostEnforcer();
            EnableHoverInfo();
        }

        private void InitUI()
        {
            Width = 130;
            Height = 40;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            
            // Background must be dark for Acrylic blur to look correct
            BackColor = Color.FromArgb(10, 10, 10); 

            // Rounded corners
            Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Position on the bottom left, directly over the taskbar
            var screen = Screen.PrimaryScreen!;
            var bounds = screen.Bounds;
            var workingArea = screen.WorkingArea;
            
            // Calculate taskbar height (Full Monitor Height - Usable Desktop Height)
            int taskbarHeight = bounds.Height - workingArea.Height;
            
            // Center it vertically inside the taskbar, fallback to bottom if taskbar is hidden
            int yPos = taskbarHeight > 0 
                ? workingArea.Height + (taskbarHeight - Height) / 2 
                : bounds.Height - Height - 5;

            StartPosition = FormStartPosition.Manual;
            // Positioned strictly on the leftmost edge of the screen
            Location = new Point(0, yPos);

            _trackInfoToolTip = new ToolTip
            {
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White,
                UseAnimation = true,
                UseFading = true
            };

            // Buttons Container - Switched to TableLayoutPanel for perfect 1/3 symmetry
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 3,
                Padding = new Padding(0)
            };
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            // Initialize controls with Segoe Fluent Icons: Prev (\uE892), Play/Pause (\uE768), Next (\uE893)
            // Anchor = None ensures they sit dead-center inside their grid columns
            var prev = new StyledButton("\uE892") { Anchor = AnchorStyles.None, Margin = new Padding(0) };
            var play = new StyledButton("\uE768") { Anchor = AnchorStyles.None, Margin = new Padding(0) };
            var next = new StyledButton("\uE893") { Anchor = AnchorStyles.None, Margin = new Padding(0) };

            // Wire up the abstract media service
            prev.Click += (s, e) => _mediaService.Previous();
            play.Click += (s, e) => _mediaService.PlayPause();
            next.Click += (s, e) => _mediaService.Next();

            buttonPanel.Controls.Add(prev, 0, 0);
            buttonPanel.Controls.Add(play, 1, 0);
            buttonPanel.Controls.Add(next, 2, 0);

            Controls.Add(buttonPanel);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyAcrylicBlur();
        }

        private void ApplyAcrylicBlur()
        {
            // Apply Windows 11 Acrylic styling
            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = NativeMethods.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = unchecked((int)0x99000000) // Hex ARGB (Dark gray with 60% opacity)
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new NativeMethods.WindowCompositionAttributeData
            {
                Attribute = NativeMethods.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(this.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        private void EnableDrag()
        {
            MouseDown += HandleMouseDown;
            MouseMove += HandleMouseMove;
        }

        private void HandleMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _dragStart = e.Location;
        }

        private void HandleMouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Location = new Point(
                    Location.X + e.X - _dragStart.X,
                    Location.Y + e.Y - _dragStart.Y
                );
            }
        }

        private void EnableTopMostEnforcer()
        {
            // Taskbar aggressively fights for Z-order; this timer maintains widget visibility
            _topMostEnforcer = new System.Windows.Forms.Timer { Interval = 2000 };
            _topMostEnforcer.Tick += (s, e) => 
            {
                NativeMethods.SetWindowPos(this.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
            };
            _topMostEnforcer.Start();
        }

        private void EnableHoverInfo()
        {
            MouseEnter += async (s, e) => await FetchTrackInfoAsync();
            // Trigger tooltip updates seamlessly across the buttons
            foreach (Control ctrl in Controls)
            {
                ctrl.MouseEnter += async (s, e) => await FetchTrackInfoAsync();
                foreach (Control child in ctrl.Controls)
                {
                    child.MouseEnter += async (s, e) => await FetchTrackInfoAsync();
                }
            }
        }

        private async Task FetchTrackInfoAsync()
        {
            var track = await _mediaService.GetCurrentTrackAsync();
            string tooltipText = track != null ? $"{track.Artist} - {track.Title}" : "No media playing";
            
            _trackInfoToolTip.SetToolTip(this, tooltipText);
            foreach (Control ctrl in Controls)
            {
                _trackInfoToolTip.SetToolTip(ctrl, tooltipText);
                foreach (Control child in ctrl.Controls)
                {
                    _trackInfoToolTip.SetToolTip(child, tooltipText);
                }
            }
        }
    }
}