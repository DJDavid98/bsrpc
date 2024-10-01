using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.Util;

namespace bsrpc.UI
{
    internal class SettingsMenuManager
    {
        private static bool DidInit { get; set; } = false;

        private const string MenuName = "bsrpc";
        private const string ResourcePath = "bsrpc.UI.SettingsViewController.bsml";

        public static void Initialize()
        {
            if (!DidInit)
            {
                MainMenuAwaiter.MainMenuInitializing += InitOnMainMenuLoaded;
                DidInit = true;
            }
        }

        public static void Disable()
        {
            MainMenuAwaiter.MainMenuInitializing -= InitOnMainMenuLoaded;
            BSMLSettings.Instance.RemoveSettingsMenu(Settings.instance);
        }

        private static void InitOnMainMenuLoaded()
        {
            BSMLSettings.Instance.AddSettingsMenu(MenuName, ResourcePath, Settings.instance);
        }
    }
}
