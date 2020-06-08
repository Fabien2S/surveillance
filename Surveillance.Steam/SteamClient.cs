using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace Surveillance.Steam
{
    public static class SteamClient
    {
        public static ulong ActiveUser => (ulong) (GetValue("ActiveProcess", "ActiveUser", 0) | 76561197960265728L);
        public static bool IsConnected => (uint) GetValue("ActiveProcess", "ActiveUser", 0) != 0;
        public static bool IsLaunched => GetValue("ActiveProcess", "pid", 0) != 0;

        public static bool IsGame(uint game, AppState state)
        {
            var value = Enum.GetName(typeof(AppState), state);
            if (value == null)
                throw new InvalidEnumArgumentException("Invalid AppState");
            return GetValue("Apps\\" + game, value, 0) == 1;
        }

        private static T GetValue<T>(string key, string value, T defaultValue)
        {
            return (T) Registry.GetValue(
                $"HKEY_CURRENT_USER\\Software\\Valve\\Steam\\{key}",
                value,
                defaultValue
            );
        }

        public enum AppState
        {
            Installed,
            Running,
            Updating
        }
    }
}