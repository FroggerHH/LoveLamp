using BepInEx;
using HarmonyLib;
using PieceManager;

#pragma warning disable CS8632
namespace LoveLamp
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        #region values
        private const string ModName = "LoveLamp", ModVersion = "1.0.0", ModGUID = "com.LoveLamp." + ModName;
        private static readonly Harmony harmony = new(ModGUID);
        public static Plugin _self;
        internal BuildPiece piece;
        #endregion
        /*#region ConfigSettings
        static string ConfigFileName = "com.Frogger.EpicMMONotifier.cfg";
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
        #region values
        static ConfigEntry<string> titleMessegesListConfig;
        static ConfigEntry<string> descriptionMessegesListConfig;
        static ConfigEntry<string> usernamesListConfig;
        static ConfigEntry<string> colorsListConfig;
        static ConfigEntry<string> webhookConfig;
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
        public List<string> titleMessegesList = new() { "Level UP!", " New level!", "Greetings new level!" };
        public List<string> usernamesList = new() { "EpicMMONotifier", " EpicMMO", "Notifier" };
        public List<string> descriptionMessegesList = new() { "#PLAYERNICK has risen to level #NEWLEVEL!", "#PLAYERNICK has received level #NEWLEVEL!", "#PLAYERNICK is now level #NEWLEVEL!" };
        public List<string> colorsList = new() { "1752220", "3447003", "15844367", "15105570", "7506394" };
        public string webhook;
        #endregion
        #endregion*/

        private void Awake()
        {
            _self = this;
            /* #region config
             Config.SaveOnConfigSet = false;

             titleMessegesListConfig = config("General", "Title Messeges", "Level UP! , New level! , Greetings new level!", "");
             descriptionMessegesListConfig = config("General", "Description Messeges", "#PLAYERNICK has risen to level #NEWLEVEL! , #PLAYERNICK has received level #NEWLEVEL! , #PLAYERNICK is now level #NEWLEVEL!", "");
             usernamesListConfig = config("General", "Usernames", "EpicMMONotifier, EpicMMO, Notifier", "");
             colorsListConfig = config("General", "Colors", "1752220, 3447003, 15844367, 15105570, 7506394", "Attention! Use only the colors from this table! \nhttps://gist.github.com/thomasbnt/b6f455e2c7d743b796917fa3c205f812?permalink_comment_id=3656937#gistcomment-3656937");
             webhookConfig = config("General", "Webhook", "", "");

             SetupWatcherOnConfigFile();

             Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };

             Config.SaveOnConfigSet = true;
             Config.Save();
             #endregion*/

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
        /* #region Config
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
                 titleMessegesList = new();
                 string titleMessegesListString = titleMessegesListConfig.Value;
                 string[] titleMesseges = titleMessegesListString.Split(',');
                 foreach(string msg in titleMesseges) titleMessegesList.Add(msg);

                 descriptionMessegesList = new();
                 string descriptionMessegesListString = descriptionMessegesListConfig.Value;
                 string[] descriptionMesseges = descriptionMessegesListString.Split(',');
                 foreach(string msg in descriptionMesseges) descriptionMessegesList.Add(msg);

                 usernamesList = new();
                 string usernamesListString = usernamesListConfig.Value;
                 string[] usernameMesseges = usernamesListString.Split(',');
                 foreach(string msg in usernameMesseges) usernamesList.Add(msg);

                 colorsList = new();
                 string colorsListString = colorsListConfig.Value;
                 string[] colorsMesseges = colorsListString.Split(',');
                 foreach(string msg in colorsMesseges) colorsList.Add(msg);

                 webhook = webhookConfig.Value;
             });

             Task.WaitAll();
             Debug("Configuration Received");
         }
         #endregion*/
    }
}