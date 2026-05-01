using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameLauncherApp.Core
{
    public class LauncherEngine
    {
        private ProcessOptimizer _optimizer;
        private KeyboardHookManager _hookManager;

        public LauncherEngine()
        {
            _optimizer = new ProcessOptimizer();
        }

        public async Task LaunchGameAsync(GameProfile profile)
        {
            Logger.LogInfo($"Launching game: {profile.GameName} at {profile.ExePath}");

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(profile.ExePath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(profile.ExePath)
                };

                Process gameProcess = Process.Start(startInfo);
                if (gameProcess == null)
                {
                    Logger.LogError("Process.Start returned null.");
                    return;
                }

                Logger.LogInfo($"Game started with PID: {gameProcess.Id}");

                // Apply Optimizations (Priority, Affinity, Suspension)
                _optimizer.ApplyOptimizations(gameProcess, profile);

                // Setup Keyboard Hook
                _hookManager = new KeyboardHookManager((uint)gameProcess.Id, profile.KeyRemappings);

                // Wait for exit in background
                await Task.Run(() =>
                {
                    gameProcess.WaitForExit();
                    Logger.LogInfo($"Game process {gameProcess.Id} exited.");
                    
                    // Cleanup
                    _hookManager.Dispose();
                    _optimizer.RestoreProcesses();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("Critical error during game launch", ex);
            }
        }
    }
}
