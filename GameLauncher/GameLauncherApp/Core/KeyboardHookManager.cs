using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameLauncherApp.Core
{
    public class KeyboardHookManager : IDisposable
    {
        private NativeMethods.LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;
        private uint _targetProcessId;
        private Dictionary<int, int> _remappings;
        private bool _isDisposed = false;

        public KeyboardHookManager(uint targetProcessId, Dictionary<int, int> remappings)
        {
            _targetProcessId = targetProcessId;
            _remappings = remappings ?? new Dictionary<int, int>();
            _proc = HookCallback;
            _hookId = SetHook(_proc);
            Logger.LogInfo($"Keyboard hook established for PID: {_targetProcessId}");
        }

        private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if (IsTargetWindowActive() && _remappings.ContainsKey(vkCode))
                {
                    int targetKey = _remappings[vkCode];
                    Logger.LogInfo($"Remapping key: {vkCode} -> {targetKey}");
                    SendKey(targetKey);
                    return (IntPtr)1; // Swallow the original key
                }
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool IsTargetWindowActive()
        {
            IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero) return false;

            NativeMethods.GetWindowThreadProcessId(foregroundWindow, out uint activePid);
            return activePid == _targetProcessId;
        }

        private void SendKey(int vkCode)
        {
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];
            
            // Key Down
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = (ushort)vkCode;
            
            // Key Up
            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = (ushort)vkCode;
            inputs[1].u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _isDisposed = true;
                Logger.LogInfo("Keyboard hook released.");
            }
        }
    }
}
