using System.Collections.Generic;

namespace GameLauncherApp.Core
{
    public class GameProfile
    {
        public string GameName { get; set; }
        public string ExePath { get; set; }
        public long CpuAffinityMask { get; set; } = -1; // -1 means all cores
        public System.Diagnostics.ProcessPriorityClass PriorityClass { get; set; } = System.Diagnostics.ProcessPriorityClass.High;
        public Dictionary<int, int> KeyRemappings { get; set; } = new Dictionary<int, int>();
        public List<string> BackgroundProcessesToSuspend { get; set; } = new List<string>();
    }
}
