using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameLauncherApp.Core
{
    public class ProcessOptimizer
    {
        private List<Process> _suspendedProcesses = new List<Process>();
        private const int STATUS_ACCESS_DENIED = unchecked((int)0xC0000022);

        public void ApplyOptimizations(Process gameProcess, GameProfile profile)
        {
            try
            {
                gameProcess.PriorityClass = profile.PriorityClass;
                Logger.LogInfo($"Priority set to {profile.PriorityClass} for PID: {gameProcess.Id}");

                // Ryzen 5 7500F SMT Isolation Logic: 6 Physical Cores, 12 Threads
                // Binary: 0101 0101 0101 -> Hex: 0x555
                // Assigning only to physical cores (Threads 0, 2, 4, 6, 8, 10) to reduce L3 thrashing
                long optimizedAffinityMask = 0x555; 
                gameProcess.ProcessorAffinity = (IntPtr)optimizedAffinityMask;
                Logger.LogInfo($"CPU Affinity isolated to Physical Cores (Mask 0x555) for PID: {gameProcess.Id}");

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
                        int status = NativeMethods.NtSuspendProcess(proc.Handle);
                        if (status == STATUS_ACCESS_DENIED)
                        {
                            Logger.LogWarning($"Access Denied (0xC0000022) when trying to suspend kernel/protected process: {proc.ProcessName}");
                            continue;
                        }
                        else if (status != 0)
                        {
                            Logger.LogWarning($"Failed to suspend {proc.ProcessName}. NTSTATUS: {status}");
                            continue;
                        }

                        _suspendedProcesses.Add(proc);
                        Logger.LogInfo($"Suspended background process: {proc.ProcessName} (PID: {proc.Id})");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Exception during suspend for {name}", ex);
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