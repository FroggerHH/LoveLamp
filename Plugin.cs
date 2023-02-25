using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PieceManager;
using ServerSync;
using System;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable CS8632
namespace LoveLamp
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        #region values
        private const string ModName = "LoveLamp", ModVersion = "1.0.1", ModGUID = "com.Frogger." + ModName;
        private static readonly Harmony harmony = new(ModGUID);
        public static Plugin _self;
        internal BuildPiece piece;
        #endregion
        #region ConfigSettings
        static string ConfigFileName = "com.Frogger.LoveLamp.cfg";
        DateTime LastConfigChange;
        public static readonly ConfigSync configSync = new(ModName) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = _self.Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }
        void SetCfgValue<T>(Action<T> setter, ConfigEntry<T> config)
        {
            setter(config.Value);
            config.SettingChanged += (_, _) => setter(config.Value);
        }
        public enum Toggle
        {
            On = 1,
            Off = 0
        }
        public class ConfigurationManagerAttributes
        {
            public int? Order;
            public bool? HideSettingName;
            public bool? HideDefaultButton;
            public string? DispName;
            public Action<ConfigEntryBase>? CustomDrawer;
        }
        #endregion
        #region values
        internal static ConfigEntry<float> radiusConfig;
        internal static ConfigEntry<float> chestRadiusConfig;
        internal static ConfigEntry<int> boostLevelConfig;
        #endregion

        private void Awake()
        {
            _self = this;
            #region config
            Config.SaveOnConfigSet = false;

            radiusConfig = config("General", "Lamp Radius", 10f, "");
            chestRadiusConfig = config("General", "Find Chest Radius", 2f, "");
            boostLevelConfig = config("Boosts", "Level", 1, "");




            SetupWatcherOnConfigFile();
            Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };
            Config.SaveOnConfigSet = true;
            Config.Save();
            #endregion

            piece = new("lovelamp", "JF_LoveLamp");
            piece.Name
                .English("Lamp of Love")
                .Russian("Лампа любви");
            piece.Description
                .English("")
                .Russian("");
            piece.RequiredItems.Add("FineWood", 4, true);
            piece.RequiredItems.Add("Stone", 6, true);
            piece.RequiredItems.Add("Ruby", 1, true);
            piece.Crafting.Set(CraftingTable.Workbench);
            piece.Category.Add(BuildPieceCategory.Furniture);
            MaterialReplacer.RegisterGameObjectForShaderSwap(piece.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            harmony.PatchAll();
        }

        #region tools
        public void Debug(string msg)
        {
            Logger.LogInfo(msg);
        }
        public void DebugError(string msg)
        {
            Logger.LogError($"{msg} Write to the developer and moderator if this happens often.");
        }
        #endregion
        #region Config
        public void SetupWatcherOnConfigFile()
        {
            FileSystemWatcher fileSystemWatcherOnConfig = new(Paths.ConfigPath, ConfigFileName);
            fileSystemWatcherOnConfig.Changed += ConfigChanged;
            fileSystemWatcherOnConfig.IncludeSubdirectories = true;
            fileSystemWatcherOnConfig.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcherOnConfig.EnableRaisingEvents = true;
        }
        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            if((DateTime.Now - LastConfigChange).TotalSeconds <= 5.0)
            {
                return;
            }
            LastConfigChange = DateTime.Now;
            try
            {
                Config.Reload();
                Debug("Reloading Config...");
            }
            catch
            {
                DebugError("Can't reload Config");
            }
        }
        private void UpdateConfiguration()
        {
            Task task = null;
            task = Task.Run(() =>
            {
                foreach(LoveLamp loveLamp in LoveLamp.all)
                {
                    loveLamp.radius = radiusConfig.Value;
                    loveLamp.m_areaMarker.m_radius = radiusConfig.Value;
                }
            });

            Task.WaitAll();
            Debug("Configuration Received");
        }
        #endregion
    }
}