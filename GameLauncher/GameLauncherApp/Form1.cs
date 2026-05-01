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
        private List<GameProfile> _games;

        public Form1()
        {
            InitializeComponent();
            _launcherEngine = new LauncherEngine();
            
            // Load persistent profiles on startup
            _games = ProfileManager.LoadProfiles();
            UpdateGameList();
            
            Logger.LogInfo("Application UI Initialized and Profiles Loaded.");
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

                        // Create a profile with user-configurable defaults (Clean Slate)
                        GameProfile newProfile = new GameProfile
                        {
                            GameName = fileName,
                            ExePath = selectedPath,
                            CpuAffinityMask = 0x555, // 6 Physical Cores on 12-thread CPU
                            PriorityClass = ProcessPriorityClass.High,
                            KeyRemappings = new Dictionary<int, int>(),
                            BackgroundProcessesToSuspend = new List<string>()
                        };
                        
                        _games.Add(newProfile);
                        
                        // Persist the updated list immediately
                        ProfileManager.SaveProfiles(_games);
                        UpdateGameList();
                        
                        Logger.LogInfo($"Game added and persisted: {newProfile.GameName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to add and persist new game profile", ex);
                MessageBox.Show("Error adding game. Check app_execution.log for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                GameProfile selectedProfile = _games[lstGames.SelectedIndex];
                
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
                MessageBox.Show("A critical error occurred. Please check the log file.", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateGameList()
        {
            try
            {
                lstGames.Items.Clear();
                foreach (var profile in _games)
                {
                    lstGames.Items.Add(profile.GameName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to update ListBox UI", ex);
            }
        }

        private void SetUiState(bool enabled)
        {
            btnLaunch.Enabled = enabled;
            btnAddGame.Enabled = enabled;
            lstGames.Enabled = enabled;
        }
    }
}
