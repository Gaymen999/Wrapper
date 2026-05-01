using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLauncherApp.Core
{
    public class KeyboardHookManager : IDisposable
    {
        private NativeMethods.LowLevelKeyboardProc _keyboardProc;
        private NativeMethods.WinEventDelegate _winEventProc;
        
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _winEventHookId = IntPtr.Zero;
        
        private readonly uint _targetProcessId;
        private readonly Dictionary<int, int> _remappings;
        private bool _isDisposed = false;
        private volatile bool _isGameWindowActive = false;

        public KeyboardHookManager(uint targetProcessId, Dictionary<int, int> remappings)
        {
            _targetProcessId = targetProcessId;
            _remappings = remappings ?? new Dictionary<int, int>();
            
            // App çökerse hook'u kaldırmayı garanti altına alıyoruz
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Dispose();

            _winEventProc = WinEventCallback;
            _winEventHookId = NativeMethods.SetWinEventHook(NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);

            _keyboardProc = KeyboardHookCallback;
            _keyboardHookId = SetKeyboardHook(_keyboardProc);
            
            // İlk açılışta aktif mi diye kontrol et
            IntPtr currentForeground = GetForegroundWindowInternal();
            UpdateActiveState(currentForeground);

            Logger.LogInfo($"Zero-latency Keyboard hook established for PID: {_targetProcessId}");
        }

        ~KeyboardHookManager()
        {
            Dispose();
        }

        private IntPtr SetKeyboardHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            UpdateActiveState(hwnd);
        }

        private void UpdateActiveState(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                _isGameWindowActive = false;
                return;
            }
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint activePid);
            _isGameWindowActive = (activePid == _targetProcessId);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindowInternal();

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
            {
                // O(1) hızında sadece bool kontrolü. Mesaj kuyruğu darboğazı engellendi.
                if (_isGameWindowActive) 
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (_remappings.ContainsKey(vkCode))
                    {
                        int targetKey = _remappings[vkCode];
                        SendKey(targetKey);
                        return (IntPtr)1; // Orijinal tuşu yut
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private void SendKey(int vkCode)
        {
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = (ushort)vkCode;
            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = (ushort)vkCode;
            inputs[1].u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;
            NativeMethods.SendInput(2, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_keyboardHookId != IntPtr.Zero) NativeMethods.UnhookWindowsHookEx(_keyboardHookId);
                if (_winEventHookId != IntPtr.Zero) NativeMethods.UnhookWinEvent(_winEventHookId);
                _isDisposed = true;
                GC.SuppressFinalize(this);
                Logger.LogInfo("Keyboard & WinEvent hooks safely released.");
            }
        }
    }
}