using IPA;
using Newtonsoft.Json;
using System;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace bsrpc
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        private PluginSocketData mapDataSource;
        private PluginSocketData liveDataSource;
        private MapData mapData;
        private LiveData liveData;
        private Discord.Discord discord;
        private static long DiscordClientId = 1028340906740420711;

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log.Info("bsrpc initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            new GameObject("bsrpcController").AddComponent<bsrpcController>();

            mapDataSource = new PluginSocketData("MapData", Log);
            mapDataSource.Update += UpdateMapData;

            liveDataSource = new PluginSocketData("LiveData", Log);
            liveDataSource.Update += UpdateLiveData;

            discord = new Discord.Discord(DiscordClientId, (UInt64)Discord.CreateFlags.Default);
        }

        private void UpdateMapData(string jsonData)
        {
            try
            {
                mapData = Deserialize<MapData>("map data", jsonData);
            }
            catch (Exception e)
            {
                Log.Error($"Error while updating map data: {e.Message}");
            }
            UpdateRichPresence();
        }
        private void UpdateLiveData(string jsonData)
        {
            try
            {
                liveData = Deserialize<LiveData>("live data", jsonData);
            }
            catch (Exception e)
            {
                Log.Error($"Error while updating live data: {e.Message}");
            }
            UpdateRichPresence();
        }

        private T Deserialize<T>(string type, string jsonData)
        {
            Log.Debug($"Updating {type}: {jsonData}");
            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.Error += (sender, e) =>
            {
                Log.Error($"Deserialize: {e}");
            };
            return JsonConvert.DeserializeObject<T>(jsonData, settings);
        }

        private void UpdateRichPresence()
        {
            Log.Debug("Updating rich presence");
            var activityManager = discord.GetActivityManager();
            if (activityManager == null)
            {
                Log.Error("Could not get Discord ActivityManager");
                return;
            }
            try
            {

                var activity = GetActivityData();
                Log.Debug($"Gathered activity data: {JsonConvert.SerializeObject(activity)}");

                try
                {
                    activityManager.UpdateActivity(activity, (result) =>
                    {
                        if (result == Discord.Result.Ok)
                        {
                            Log.Debug("Sucessfully updated Discord Activity");
                        }
                        else
                        {
                            Log.Error("Failed to update Discord Activity");
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Error while updating rich presence: {e.Message}\n{e.Source}");
                }
            }
            catch (NullReferenceException e)
            {
                Log.Error($"Null reference error while updating rich presence: {e.Message}\n{e.Source}");
            }
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

        private string GetPlayState()
        {
            if (mapData.LevelPaused)
            {
                return "Paused";
            }

            if (mapData.LevelFailed)
            {
                return "Failed";
            }

            if (mapData.LevelFinished)
            {

                return "Finished";
            }

            if (liveData != null)
            {
                return "Playing";
            }

            return "Waiting";
        }

        private Discord.Activity GetActivityData()
        {
            Log.Debug("Building activity data");
            Log.Debug($"mapData = {JsonConvert.SerializeObject(mapData)}");
            Log.Debug($"liveData = {JsonConvert.SerializeObject(liveData)}");
            var activity = new Discord.Activity();
            if (mapData != null)
            {
                activity.Name = $"Beat Saber {mapData.GameVersion}";
                if (mapData.InLevel)
                {
                    var playState = GetPlayState();
                    var lobbyType = mapData.IsMultiplayer ? "Multiplayer" : "Solo";
                    var rankedStatus = mapData.Star > 0 ? "Ranked" : "Unranked";
                    activity.State = $"{playState} - {lobbyType} ({rankedStatus})";

                    var difficulty = GetReadableDifficulty(mapData.Difficulty);
                    activity.Details = $"{mapData.SongName} by {mapData.SongAuthor} ({difficulty})";

                    if (liveData != null && !mapData.LevelPaused)
                    {
                        activity.Timestamps.Start = DateTime.UtcNow.Ticks - liveData.TimeElapsed;
                        activity.Timestamps.End = activity.Timestamps.Start + mapData.Length;
                    }
                }
                else
                {
                    if (mapData.IsMultiplayer)
                    {
                        activity.State = "Multiplayer Lobby";
                    }
                    else
                    {
                        activity.State = "Main Menu";
                    }
                }
            }
            return activity;
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            discord.Dispose();
            mapDataSource.Cleanup();
            liveDataSource.Cleanup();
        }
    }
}
