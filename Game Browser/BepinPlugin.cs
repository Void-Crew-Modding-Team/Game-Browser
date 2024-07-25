using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using VoidManager.CustomGUI;
using VoidManager.MPModChecks;
using VoidManager.Utilities;

namespace Game_Browser
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class BepinPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private void Awake()
        {
            BepinPlugin.Log = base.Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "Mest.GameBrowser");
            Game_Browser.Config.Load(this);
            new GameObject("GameBrowser", new Type[]
            {
                typeof(GameBrowserUI)
            }).hideFlags = HideFlags.HideAndDontSave;
            base.Logger.LogInfo("Plugin Mest.GameBrowser is loaded!");
        }
    }
    public class VoidManagerPlugin : VoidManager.VoidPlugin
    {
        public override MultiplayerType MPType => MultiplayerType.Client;
    }

    internal class Config : ModSettingsMenu
    {
        public override string Name() => "Game Browser Config";
        public override void Draw()
        {
            GUITools.DrawCheckbox("Show Full Rooms", ref Config.showFullRooms);
            GUITools.DrawCheckbox("Show Empty Rooms", ref Config.showEmptyRooms);
        }
        internal static void Load(BepinPlugin plugin)
        {
            Config.showFullRooms = plugin.Config.Bind<bool>("GameBrowser", "showFullRooms", false);
            Config.showEmptyRooms = plugin.Config.Bind<bool>("GameBrowser", "showEmptyRooms", false);
        }

        internal static ConfigEntry<bool> showFullRooms;
        internal static ConfigEntry<bool> showEmptyRooms;
    }
}