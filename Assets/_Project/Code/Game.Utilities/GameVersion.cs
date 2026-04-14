using UnityEngine;

namespace Game.Utilities
{
    public static class GameVersion
    {
        public static string Version => Application.version;
        public static string BuildNumber => GameVersionGenerated.BuildNumber;
        public static string BuildTimestamp => GameVersionGenerated.BuildTimestamp;

        public static string Full => $"v{Version}#{BuildNumber} ({BuildTimestamp})";
    }
}