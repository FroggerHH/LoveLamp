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
        internal static ConfigEntry<float> updateIntervalConfig;
        internal static ConfigEntry<int> maxCreaturesConfig;
        internal static ConfigEntry<int> requiredLovePointsConfig;
        internal static ConfigEntry<float> pregnancyDurationConfig;
        internal static ConfigEntry<float> tamingTimeConfig;
        internal static ConfigEntry<float> levelUpFactorConfig;
        internal static ConfigEntry<float> healthConfig;
        internal static ConfigEntry<float> speedConfig;
        internal static ConfigEntry<float> jumpForceConfig;
        internal static ConfigEntry<string> namePostfixConfig;
        internal static ConfigEntry<int> boostLevelConfig;
        internal static ConfigEntry<int> maxFuelConfig;
        internal static ConfigEntry<int> secPerFuelConfig;
        internal static float radius;
        internal static float chestRadius;
        internal static float updateInterval;
        internal static int maxCreatures;
        internal static int requiredLovePoints;
        internal static float pregnancyDuration;
        internal static float tamingTime;
        internal static float levelUpFactor;
        internal static float health;
        internal static float speed;
        internal static float jumpForce;
        internal static string namePostfix;
        internal static int boostLevel;
        internal static int maxFuel;
        internal static int secPerFuel;
        #endregion

        private void Awake()
        {
            _self = this;
            #region config
            Config.SaveOnConfigSet = false;

            radiusConfig = config("General", "Lamp Radius", 10f, new ConfigDescription("", new AcceptableValueRange<float>(5f, 25)));
            chestRadiusConfig = config("General", "Find Chest Radius", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.5f, 25)));
            boostLevelConfig = config("Boosts", "Add Level", 1, new ConfigDescription("", new AcceptableValueRange<int>(1, 5)));
            updateIntervalConfig = config("Boosts", "Update Interval", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            maxCreaturesConfig = config("Boosts", "Max Creatures", 2, new ConfigDescription("", new AcceptableValueRange<int>(1, 20)));
            requiredLovePointsConfig = config("Boosts", "Required Love Points", 2, new ConfigDescription("", new AcceptableValueRange<int>(1, 5)));
            pregnancyDurationConfig = config("Boosts", "Pregnancy Duration", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            tamingTimeConfig = config("Boosts", "Taming Time", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            levelUpFactorConfig = config("Boosts", "Level Up Factor", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            healthConfig = config("Boosts", "Health", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            speedConfig = config("Boosts", "Speed", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            jumpForceConfig = config("Boosts", "Jump Force", 2f, new ConfigDescription("", new AcceptableValueRange<float>(1.01f, 5f)));
            namePostfixConfig = config("Boosts", "Name Postfix", "Boosted", "");
            secPerFuelConfig = config("Boosts", "Sec Per Fuel", 400, new ConfigDescription("", new AcceptableValueRange<int>(120, 40000)));
            maxFuelConfig = config("Boosts", "Max Fuel", 50, new ConfigDescription("", new AcceptableValueRange<int>(25, 100)));



            SetupWatcherOnConfigFile();
            Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };
            Config.SaveOnConfigSet = true;
            Config.Save();
            #endregion

            #region piece
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
            #endregion
            //PrefabManager.RegisterPrefab("lovelamp","vfx_addFuel");
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
                LoveLamp.UnBoostAll();

                radius = radiusConfig.Value;
                chestRadius = chestRadiusConfig.Value;
                updateInterval = updateIntervalConfig.Value;
                maxCreatures = maxCreaturesConfig.Value;
                requiredLovePoints = requiredLovePointsConfig.Value;
                pregnancyDuration = pregnancyDurationConfig.Value;
                tamingTime = tamingTimeConfig.Value;
                levelUpFactor = levelUpFactorConfig.Value;
                health = healthConfig.Value;
                speed = speedConfig.Value;
                jumpForce = jumpForceConfig.Value;
                namePostfix = namePostfixConfig.Value;
                boostLevel = boostLevelConfig.Value;
                maxFuel = maxFuelConfig.Value;
                secPerFuel = secPerFuelConfig.Value;

                foreach(LoveLamp loveLamp in LoveLamp.all)
                {
                    loveLamp.radius = radius;
                    loveLamp.m_areaMarker.m_radius = radius;
                    loveLamp.m_maxFuel = maxFuel;
                    loveLamp.m_secPerFuel = secPerFuel;
                }
            });

            Task.WaitAll();
            Debug("Configuration Received");
        }
        #endregion
    }
}