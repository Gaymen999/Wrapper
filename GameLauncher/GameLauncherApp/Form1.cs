using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GameLauncherApp.Core;

namespace GameLauncherApp
{
    public partial class Form1 : Form
    {
        private readonly LauncherEngine _launcherEngine;
        private readonly List<GameProfile> _gameProfiles;

        public Form1()
        {
            InitializeComponent();
            _launcherEngine = new LauncherEngine();
            _gameProfiles = new List<GameProfile>();
            Logger.LogInfo("Application UI Initialized.");
        }

        private void btnAddGame_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                    openFileDialog.Title = "Select Game Executable";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedPath = openFileDialog.FileName;
                        string fileName = Path.GetFileNameWithoutExtension(selectedPath);

                        // Create a profile with default optimizations
                        GameProfile newProfile = CreateDefaultProfile(fileName, selectedPath);
                        
                        _gameProfiles.Add(newProfile);
                        lstGames.Items.Add(newProfile.GameName);
                        
                        Logger.LogInfo($"Game added to library: {newProfile.GameName} ({selectedPath})");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to add game to library", ex);
                MessageBox.Show("Error adding game. Check log for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLaunch_Click(object sender, EventArgs e)
        {
            if (lstGames.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a game from the list first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                GameProfile selectedProfile = _gameProfiles[lstGames.SelectedIndex];
                
                SetUiState(false);
                lblStatus.Text = $"Status: Running {selectedProfile.GameName}";
                Logger.LogInfo($"User requested launch for: {selectedProfile.GameName}");

                await _launcherEngine.LaunchGameAsync(selectedProfile);

                lblStatus.Text = "Status: Ready";
                SetUiState(true);
            }
            catch (Exception ex)
            {
                Logger.LogError("Critical UI failure during game launch sequence", ex);
                lblStatus.Text = "Status: Launch Error";
                SetUiState(true);
            }
        }

        private GameProfile CreateDefaultProfile(string name, string path)
        {
            // Default Optimization: SMT Bypass for 6C/12T (0x555) and F -> V remapping
            return new GameProfile
            {
                GameName = name,
                ExePath = path,
                CpuAffinityMask = 0x555, // 010101010101 in binary: targets Threads 0, 2, 4, 6, 8, 10
                PriorityClass = ProcessPriorityClass.High,
                KeyRemappings = new Dictionary<int, int>
                {
                    { 0x46, 0x56 } // 'F' (0x46) -> 'V' (0x56)
                },
                BackgroundProcessesToSuspend = new List<string>() // Empty by default for stability
            };
        }

        private void SetUiState(bool enabled)
        {
            btnLaunch.Enabled = enabled;
            btnAddGame.Enabled = enabled;
            lstGames.Enabled = enabled;
        }
    }
}
