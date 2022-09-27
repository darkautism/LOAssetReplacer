using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace LOAssetReplacer{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal new static ManualLogSource Log;
        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
            if (!Directory.Exists("mod"))
                Directory.CreateDirectory("mod");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(DownloadHandlerAssetBundle), nameof(DownloadHandlerAssetBundle.GetContent))]
    public static class AssetReplacer
    {
        static bool Prefix(ref AssetBundle __result, UnityWebRequest www, DownloadHandlerAssetBundle __instance)
        {
            string assetBundleName = System.IO.Path.GetFileName(www.GetUrl());
            if (System.IO.Directory.Exists("mod/" + assetBundleName))
            {
                string[] filePaths = Directory.GetFiles("mod\\" + assetBundleName, "__data", SearchOption.AllDirectories);
                if (filePaths.Length == 0)
                    return true;
                DownloadHandler.GetCheckedDownloader<DownloadHandlerAssetBundle>(www).assetBundle.Unload(true);
                AssetBundle ab = AssetBundle.LoadFromFile(filePaths[0]);
                Plugin.Log.LogInfo($"Load from mod: {assetBundleName}");
                __result = ab;
                return false;
            }
            return true;
        }
    }
}

