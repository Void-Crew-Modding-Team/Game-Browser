using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using VoidManager.MPModChecks;

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
}