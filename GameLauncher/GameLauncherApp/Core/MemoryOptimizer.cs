using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.Generic;

namespace GameLauncherApp.Core
{
    public class MemoryOptimizer
    {
        /// <summary>
        /// Iterates through background processes and flushes their working set to page file.
        /// This frees up physical RAM for the target game.
        /// </summary>
        /// <param name="targetPid">The Process ID of the game to exclude from flushing.</param>
        public void FlushMemory(uint targetPid)
        {
            Logger.LogInfo("Starting RAM optimization sequence...");

            Process[] allProcesses = Process.GetProcesses();

            foreach (Process process in allProcesses)
            {
                try
                {
                    // Skip the target game process and critical system processes
                    if (process.Id == targetPid || IsProtectedProcess(process))
                    {
                        continue;
                    }

                    // EmptyWorkingSet requires a handle with PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION 
                    // and PROCESS_SET_QUOTA access. Process.Handle usually provides sufficient access if running as admin.
                    bool success = NativeMethods.EmptyWorkingSet(process.Handle);
                    
                    if (success)
                    {
                        Logger.LogInfo($"Successfully flushed memory for: {process.ProcessName} (PID: {process.Id})");
                    }
                }
                catch (Win32Exception)
                {
                    // Silently ignore access denied errors for elevated/system processes
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Unexpected error flushing memory for PID {process.Id}", ex);
                }
                finally
                {
                    // Dispose of the process object to free managed resources immediately
                    process.Dispose();
                }
            }

            Logger.LogInfo("RAM optimization sequence completed.");
        }

        /// <summary>
        /// Checks if a process is a critical system process that should not be touched.
        /// </summary>
        private bool IsProtectedProcess(Process process)
        {
            // Filter out system idle, kernel, and initial processes
            if (process.Id <= 10) return true;

            try
            {
                string processName = process.ProcessName.ToLower();
                
                // Known critical system processes
                if (processName == "lsass" || 
                    processName == "wininit" || 
                    processName == "smss" || 
                    processName == "csrss" || 
                    processName == "services" ||
                    processName == "winlogon")
                {
                    return true;
                }

                // Any process running from the System32 directory is typically sensitive
                string fileName = process.MainModule.FileName.ToLower();
                if (fileName.Contains("c:\\windows\\system32"))
                {
                    return true;
                }
            }
            catch
            {
                // If we cannot access the process name or module (due to high integrity),
                // treat it as a protected process to be safe.
                return true;
            }

            return false;
        }
    }
}