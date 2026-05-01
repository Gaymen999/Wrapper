using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameLauncherApp.Core
{
    public class LauncherEngine
    {
        private readonly ProcessOptimizer _optimizer;
        private readonly MemoryOptimizer _memoryOptimizer;
        private KeyboardHookManager _hookManager;

        public LauncherEngine()
        {
            _optimizer = new ProcessOptimizer();
            _memoryOptimizer = new MemoryOptimizer();
        }

        /// <summary>
        /// Orchestrates the game launch sequence, applies CPU/Process optimizations, 
        /// flushes system RAM, and initializes the low-level keyboard hook.
        /// </summary>
        /// <param name="profile">The game profile containing execution and optimization settings.</param>
        public async Task LaunchGameAsync(GameProfile profile)
        {
            Logger.LogInfo($"Initiating launch sequence for: {profile.GameName}");

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
                    Logger.LogError($"Failed to start process: {profile.ExePath}");
                    return;
                }

                Logger.LogInfo($"Game process started with PID: {gameProcess.Id}");

                // 1. Apply Process Optimizations (Priority, Affinity, Suspension)
                try
                {
                    _optimizer.ApplyOptimizations(gameProcess, profile);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error applying process optimizations", ex);
                }

                // 2. Execute RAM Flushing (Memory Optimization)
                try
                {
                    _memoryOptimizer.FlushMemory((uint)gameProcess.Id);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error executing memory flush", ex);
                }

                // 3. Setup Keyboard Hook for remapping
                try
                {
                    _hookManager = new KeyboardHookManager((uint)gameProcess.Id, profile.KeyRemappings);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error initializing keyboard hook manager", ex);
                }

                // 4. Asynchronous Wait for process exit and subsequent cleanup
                await Task.Run(() =>
                {
                    try
                    {
                        gameProcess.WaitForExit();
                        Logger.LogInfo($"Game process {gameProcess.Id} has exited.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error while waiting for game process {gameProcess.Id} exit", ex);
                    }
                    finally
                    {
                        CleanupResources();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError("Critical failure in LauncherEngine.LaunchGameAsync", ex);
            }
        }

        /// <summary>
        /// Ensures all hooks are detached and suspended processes are resumed upon game closure.
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                if (_hookManager != null)
                {
                    _hookManager.Dispose();
                    _hookManager = null;
                }

                _optimizer.RestoreProcesses();
                Logger.LogInfo("Post-game cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during resource cleanup", ex);
            }
        }
    }
}
