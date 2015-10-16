// Helpers/Settings.cs
using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;

namespace XFStreamingAudio.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;
        private const string bandwidthSwitchStateKey = "bandwidthSwitchState";
        private static readonly bool bandwidthSwitchStateDefault = true;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

        public static bool BandwidthSwitchState
        {
            get { return AppSettings.GetValueOrDefault<bool>(bandwidthSwitchStateKey, bandwidthSwitchStateDefault); }
            set { AppSettings.AddOrUpdateValue<bool>(bandwidthSwitchStateKey, value); }
        }

    }
}