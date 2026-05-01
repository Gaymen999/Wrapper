using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using GameLauncherApp.Core;

namespace GameLauncherApp
{
    public partial class Form1 : Form
    {
        private LauncherEngine _engine;

        public Form1()
        {
            InitializeComponent();
            _engine = new LauncherEngine();
            Logger.LogInfo("Application UI Initialized.");
        }

        private async void btnLaunch_Click(object sender, EventArgs e)
        {
            // Example: Remap 'F' (0x46) to 'V' (0x56)
            var profile = new GameProfile
            {
                GameName = "Test Notepad",
                ExePath = @"C:\Windows\System32\notepad.exe",
                CpuAffinityMask = 0x01, // Use only the first core
                PriorityClass = ProcessPriorityClass.High,
                KeyRemappings = new Dictionary<int, int>
                {
                    { 0x46, 0x56 } // F -> V
                },
                BackgroundProcessesToSuspend = new List<string> { "CalculatorApp" } // Example process name
            };

            btnLaunch.Enabled = false;
            lblStatus.Text = "Status: Game Running...";
            
            await _engine.LaunchGameAsync(profile);
            
            lblStatus.Text = "Status: Game Closed";
            btnLaunch.Enabled = true;
        }
    }
}
