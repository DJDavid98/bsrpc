using BeatSaberMarkupLanguage.Settings;
using bsrpc.UI;
using DataPuller.Data;
using DiscordCore;
using IPA;
using IPA.Config.Stores;
using System;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace bsrpc
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private DiscordInstance _discord;
        private DateTime _lastSceneSwitchTime;
        private DataMappers.Scenes _lastScene = DataMappers.Scenes.Unknown;
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config conf)
        {
            Log = logger;
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            Log.Info("bsrpc initialized.");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            new GameObject("bsrpcController").AddComponent<BsrpcController>();

            MapData.Instance.OnUpdate += UpdateRichPresence;
            LiveData.Instance.OnUpdate += UpdateRichPresence;

            CreateDiscordManager();
            PluginConfig.Instance.OnReloaded += HandleConfigUpdate;
            PluginConfig.Instance.OnChanged += HandleConfigUpdate;
        }

        [OnEnable]
        public void OnEnable()
        {
            SettingsMenuManager.Initialize();
        }

        [OnDisable]
        public void OnDisable()
        {
            SettingsMenuManager.Disable();
        }

        private void HandleConfigUpdate()
        {
            if (PluginConfig.Instance.DiscordClientId != _discord.settings.appId)
            {
                // Recreate Discord connection on App ID change
                _discord.DestroyInstance();
                CreateDiscordManager();
            }
            // Force update to Rich Presence
            UpdateRichPresence();
        }

        private void CreateDiscordManager()
        {
            _discord = DiscordManager.instance.CreateInstance(new DiscordSettings
            {
                appId = PluginConfig.Instance.DiscordClientId,
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
            var activity = DataMappers.GetActivityData();
            var currentScene = DataMappers.GetCurrentScene();
            if (_lastScene != currentScene)
            {
                _lastScene = currentScene;
                _lastSceneSwitchTime = DateTime.Now;
            }
            if (PluginConfig.Instance.ShowElapsedTimes && currentScene != DataMappers.Scenes.Playing)
            {
                activity.Timestamps.Start = DataMappers.DateTimeToUnixTimestamp(_lastSceneSwitchTime);
            }

            _discord.UpdateActivity(activity);
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            MapData.Instance.OnUpdate -= UpdateRichPresence;
            LiveData.Instance.OnUpdate -= UpdateRichPresence;
            PluginConfig.Instance.OnReloaded -= HandleConfigUpdate;
            PluginConfig.Instance.OnChanged -= HandleConfigUpdate;
            _discord.DestroyInstance();
        }
    }
}
