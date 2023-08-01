using BeatSaberMarkupLanguage.Attributes;

namespace bsrpc.UI
{
    internal class Settings : PersistentSingleton<Settings>
    {
        [UIValue("multiplayer-detection")]
        public bool multiplayerDetection { get { return PluginConfig.Instance.MultiplayerDetection; } set { PluginConfig.Instance.MultiplayerDetection = value; } }

        [UIValue("multiplayer-party-info")]
        public bool multiplayerPartyInfo { get { return PluginConfig.Instance.MultiplayerPartyInfo; } set { PluginConfig.Instance.MultiplayerPartyInfo = value; } }

        [UIValue("large-image-cover")]
        public bool largeImageCover { get { return PluginConfig.Instance.LargeImageSongCover; } set { PluginConfig.Instance.LargeImageSongCover = value; } }

        [UIValue("show-elapsed-times")]
        public bool showElapsedTimes { get { return PluginConfig.Instance.ShowElapsedTimes; } set { PluginConfig.Instance.ShowElapsedTimes = value; } }

        [UIValue("small-image-map-type")]
        public bool smallImageMapType { get { return PluginConfig.Instance.ShowMapType; } set { PluginConfig.Instance.ShowMapType = value; } }
    }
}
