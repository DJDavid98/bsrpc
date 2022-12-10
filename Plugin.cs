using DataPuller.Data;
using Discord;
using DiscordCore;
using IPA;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace bsrpc
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        private DiscordInstance _discord;
        private const long DiscordClientId = 1028340906740420711;
        private Timer _updateTimer;

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
                modName = nameof(bsrpc),
            });

            SetUpdateTimer(0);
        }

        // Update rich presence regularly on a schedule even if the update events are not fired
        private void SetUpdateTimer(int debounceMs = 500)
        {
            _updateTimer?.Dispose();
            _updateTimer = new Timer((e) => UpdateRichPresence(), null, debounceMs, 5000);
        }

        private void UpdateRichPresence(string jsonData)
        {
            UpdateRichPresence();
        }
        private void UpdateRichPresence()
        {
            var activity = GetActivityData();

            _discord.UpdateActivity(activity);
            SetUpdateTimer();
        }

        private string GetReadableDifficulty(string originalDifficulty)
        {
            switch (originalDifficulty)
            {
                case "ExpertPlus":
                    return "Expert+";
                default:
                    return originalDifficulty;
            }
        }

        private string GetReadableRank(string originalRank)
        {
            switch (originalRank)
            {
                case "SSS":
                    // Match in-game rank shown during zen mode
                    return "E";
                default:
                    return originalRank;
            }
        }

        private string GetPlayState()
        {
            if (MapData.Instance.LevelPaused)
            {
                return "⏸️";
            }

            if (MapData.Instance.LevelFailed)
            {
                return "☠️";
            }

            if (MapData.Instance.LevelFinished)
            {

                return "🎉";
            }

            if (LiveData.Instance.TimeElapsed > 0)
            {
                return "▶️";
            }

            return "⌛";
        }

        private string GetModifiersState()
        {
            StringBuilder modifiersString = new StringBuilder();
            if (LiveData.Instance.TimeElapsed > 0)
            {
                if (MapData.Instance.Modifiers.ZenMode)
                {
                    modifiersString.Append("🧘");
                }
                else
                {
                    // Life modifiers
                    if (MapData.Instance.Modifiers.NoFailOn0Energy || MapData.Instance.Modifiers.OneLife || MapData.Instance.Modifiers.FourLives)
                    {
                        if (MapData.Instance.Modifiers.NoFailOn0Energy)
                        {
                            modifiersString.Append("🛡");
                        }
                        else if (MapData.Instance.Modifiers.OneLife)
                        {
                            modifiersString.Append("1🤍");
                        }
                        else if (MapData.Instance.Modifiers.FourLives)
                        {
                            modifiersString.Append("4🤍");
                        }
                        modifiersString.Append(" ");
                    }

                    // Difficulty decreasing modifiers
                    if (MapData.Instance.Modifiers.NoBombs || MapData.Instance.Modifiers.NoWalls || MapData.Instance.Modifiers.NoArrows)
                    {
                        modifiersString.Append("🚫");
                        if (MapData.Instance.Modifiers.NoBombs)
                        {
                            modifiersString.Append("💣");
                        }
                        if (MapData.Instance.Modifiers.NoWalls)
                        {
                            modifiersString.Append("🧱");
                        }
                        if (MapData.Instance.Modifiers.NoArrows)
                        {
                            modifiersString.Append("🔽");
                        }
                        modifiersString.Append(" ");
                    }

                    // Difficulty increasing modifiers
                    if (MapData.Instance.Modifiers.GhostNotes || MapData.Instance.Modifiers.DisappearingArrows)
                    {
                        if (MapData.Instance.Modifiers.GhostNotes)
                        {
                            modifiersString.Append("👻");
                        }
                        else if (MapData.Instance.Modifiers.DisappearingArrows)
                        {
                            modifiersString.Append("🟦");
                        }
                        modifiersString.Append(" ");
                    }

                    if (MapData.Instance.Modifiers.SmallNotes || MapData.Instance.Modifiers.ProMode || MapData.Instance.Modifiers.StrictAngles)
                    {
                        if (MapData.Instance.Modifiers.SmallNotes)
                        {
                            modifiersString.Append("🔹");
                        }
                        if (MapData.Instance.Modifiers.ProMode)
                        {
                            modifiersString.Append("⚔️");
                        }
                        if (MapData.Instance.Modifiers.StrictAngles)
                        {
                            modifiersString.Append("📐");
                        }
                        modifiersString.Append(" ");
                    }
                }

                // Speed modifiers
                if (MapData.Instance.Modifiers.SlowerSong)
                {
                    modifiersString.Append("🐌");
                }
                else if (MapData.Instance.Modifiers.FasterSong)
                {
                    modifiersString.Append("⏩");
                }
                if (MapData.Instance.Modifiers.SuperFastSong)
                {
                    modifiersString.Append("⏩");
                }
            }

            return modifiersString.ToString().Trim();
        }

        private ActivityAssets GetActivityAssets()
        {
            var assets = new ActivityAssets();
            if (MapData.Instance.InLevel)
            {
                assets.LargeImage = MapData.Instance.CoverImage ?? RichPresenceAssetKeys.TheFirst;

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
                if (MapData.Instance.IsMultiplayer)
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

        private Discord.Activity GetActivityData()
        {
            var activity = new Discord.Activity();
            activity.Assets = GetActivityAssets();
            if (MapData.Instance.InLevel)
            {
                var playState = GetPlayState() + GetModifiersState();
                var lobbyType = MapData.Instance.IsMultiplayer ? "Multiplayer" : "Singleplayer";
                var rankedStatus = MapData.Instance.Star > 0 ? $" ⭐{MapData.Instance.Star:N}" : "";
                var playDetail = "";

                var difficulty = GetReadableDifficulty(MapData.Instance.Difficulty);
                var songSubName = MapData.Instance.SongSubName.Length > 0 ? $" {MapData.Instance.SongSubName}" : "";
                var mapper = MapData.Instance.Mapper.Length > 0 ? $" [{MapData.Instance.Mapper}]" : "";
                activity.Details = $"{MapData.Instance.SongName}{songSubName} by {MapData.Instance.SongAuthor}{mapper} ({difficulty}{rankedStatus})";

                if (LiveData.Instance.TimeElapsed > 0)
                {
                    var rank = GetReadableRank(LiveData.Instance.Rank);
                    var score = LiveData.Instance.Score;
                    var combo = LiveData.Instance.Combo > 0 ? $" x{LiveData.Instance.Combo:N0}" : "";
                    var accuracy = LiveData.Instance.Accuracy;
                    playDetail = $" {score:N0}{combo} {accuracy:F2}% ({rank})";
                    if (!MapData.Instance.LevelPaused)
                    {
                        var elapsedTime = Convert.ToDouble(LiveData.Instance.TimeElapsed);
                        activity.Timestamps.End = DateTimeToUnixTimestamp(DateTime.Now.AddSeconds(-elapsedTime).AddSeconds(MapData.Instance.Duration));
                    }
                }

                activity.State = $"{playState} {lobbyType}{playDetail}";
            }
            else
            {
                if (MapData.Instance.IsMultiplayer)
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

        }
    }
}
