using DataPuller.Data;
using Discord;
using DiscordCore;
using IPA;
using System;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace bsrpc
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private const long DiscordClientId = 1028340906740420711;
        private DiscordInstance _discord;
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Log = logger;
            Log.Info("bsrpc initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            new GameObject("bsrpcController").AddComponent<BsrpcController>();

            MapData.Instance.OnUpdate += UpdateRichPresence;
            LiveData.Instance.OnUpdate += UpdateRichPresence;

            _discord = DiscordManager.instance.CreateInstance(new DiscordSettings
            {
                appId = DiscordClientId,
                handleInvites = false,
                modId = nameof(bsrpc),
                modName = nameof(bsrpc)
            });
        }

        private void UpdateRichPresence(string jsonData)
        {
            UpdateRichPresence();
        }

        private void UpdateRichPresence()
        {
            var activity = GetActivityData();

            _discord.UpdateActivity(activity);
        }

        private bool IsMultiplayer()
        {
            return MapData.Instance.IsMultiplayer;
        }

        private ActivityAssets GetActivityAssets()
        {
            var assets = new ActivityAssets();
            if (MapData.Instance.InLevel)
            {
                assets.LargeImage = RichPresenceAssetKeys.TheFirst;

                switch (MapData.Instance.MapType)
                {
                    case "Standard":
                        assets.SmallImage = RichPresenceAssetKeys.Standard;
                        assets.SmallText = "Standard";
                        break;
                    case "OneSaber":
                        assets.SmallImage = RichPresenceAssetKeys.OneSaber;
                        assets.SmallText = "One Saber";
                        break;
                    case "NoArrows":
                        assets.SmallImage = RichPresenceAssetKeys.NoArrows;
                        assets.SmallText = "No Arrows";
                        break;
                    case "360Degree":
                        assets.SmallImage = RichPresenceAssetKeys.ThreeSixty;
                        assets.SmallText = "360°";
                        break;
                    case "90Degree":
                        assets.SmallImage = RichPresenceAssetKeys.Ninety;
                        assets.SmallText = "90°";
                        break;
                    default:
                        assets.SmallImage = RichPresenceAssetKeys.BlankMapType;
                        assets.SmallText = MapData.Instance.MapType;
                        break;
                }
            }
            else
            {
                if (IsMultiplayer())
                {
                    assets.LargeImage = RichPresenceAssetKeys.MultiplayerLobby;
                }
                else
                {
                    assets.LargeImage = RichPresenceAssetKeys.MainMenu;
                }
            }
            if (assets.LargeImage != null)
            {
                assets.LargeText = $"Game version: {MapData.GameVersion}";
            }

            return assets;
        }

        private Activity GetActivityData()
        {
            var activity = new Activity();
            activity.Assets = GetActivityAssets();
            if (MapData.Instance.InLevel)
            {
                var playState = DataMappers.GetPlayState() + DataMappers.GetModifiersState();
                var lobbyType = IsMultiplayer() ? "Multiplayer" : "Singleplayer";
                var rankedStatus = MapData.Instance.Star > 0 ? $" ⭐{MapData.Instance.Star:N}" : "";
                var playDetail = "";

                var difficulty = DataMappers.GetReadableDifficulty(MapData.Instance.Difficulty);
                var songSubName = MapData.Instance.SongSubName.Length > 0 ? $" {MapData.Instance.SongSubName}" : "";
                var mapper = MapData.Instance.Mapper.Length > 0 ? $" [{MapData.Instance.Mapper}]" : "";
                activity.Details = $"{MapData.Instance.SongName}{songSubName} by {MapData.Instance.SongAuthor}{mapper} ({difficulty}{rankedStatus})";

                if (LiveData.Instance.TimeElapsed > 0)
                {
                    var rank = DataMappers.GetReadableRank(LiveData.Instance.Rank);
                    var score = LiveData.Instance.Score > 0 ? $" {LiveData.Instance.Score}" : "";
                    var combo = LiveData.Instance.Combo > 0 ? $" x{LiveData.Instance.Combo:N0}" : "";
                    var accuracy = LiveData.Instance.Accuracy;
                    playDetail = $"{score}{combo} {accuracy:F2}% ({rank})";
                    if (!MapData.Instance.LevelPaused)
                    {
                        var elapsedTime = Convert.ToDouble(LiveData.Instance.TimeElapsed);
                        activity.Timestamps.End = DateTimeToUnixTimestamp(DateTime.Now.AddSeconds(-elapsedTime)
                            .AddSeconds(MapData.Instance.Duration));
                    }
                }

                activity.State = $"{playState} {lobbyType}{playDetail}";
            }
            else
            {
                if (IsMultiplayer())
                {
                    activity.State = "Multiplayer Lobby";
                }
                else
                {
                    activity.State = "Main Menu";
                }
            }

            return activity;
        }

        private long DateTimeToUnixTimestamp(DateTime ticks)
        {
            return ((DateTimeOffset)ticks).ToUnixTimeMilliseconds();
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            MapData.Instance.OnUpdate -= UpdateRichPresence;
            LiveData.Instance.OnUpdate -= UpdateRichPresence;
            _discord.DestroyInstance();
        }
    }
}
