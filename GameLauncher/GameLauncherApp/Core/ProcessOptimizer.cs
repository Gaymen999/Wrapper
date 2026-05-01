using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameLauncherApp.Core
{
    public class ProcessOptimizer
    {
        private List<Process> _suspendedProcesses = new List<Process>();

        public void ApplyOptimizations(Process gameProcess, GameProfile profile)
        {
            try
            {
                // 1. Set Priority
                gameProcess.PriorityClass = profile.PriorityClass;
                Logger.LogInfo($"Priority set to {profile.PriorityClass} for PID: {gameProcess.Id}");

                // 2. Set Affinity
                if (profile.CpuAffinityMask != -1)
                {
                    gameProcess.ProcessorAffinity = (IntPtr)profile.CpuAffinityMask;
                    Logger.LogInfo($"CPU Affinity set to mask {profile.CpuAffinityMask} for PID: {gameProcess.Id}");
                }

                // 3. Suspend Background Processes
                SuspendProcesses(profile.BackgroundProcessesToSuspend);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to apply optimizations", ex);
            }
        }

        private void SuspendProcesses(List<string> processNames)
        {
            foreach (var name in processNames)
            {
                var processes = Process.GetProcessesByName(name);
                foreach (var proc in processes)
                {
                    try
                    {
                        NativeMethods.NtSuspendProcess(proc.Handle);
                        _suspendedProcesses.Add(proc);
                        Logger.LogInfo($"Suspended background process: {proc.ProcessName} (PID: {proc.Id})");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Could not suspend {name}", ex);
                    }
                }
            }
        }

        public void RestoreProcesses()
        {
            foreach (var proc in _suspendedProcesses)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        NativeMethods.NtResumeProcess(proc.Handle);
                        Logger.LogInfo($"Resumed background process: {proc.ProcessName} (PID: {proc.Id})");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Could not resume process {proc.Id}", ex);
                }
            }
            _suspendedProcesses.Clear();
        }
    }
}
