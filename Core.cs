using HarmonyLib;
using Il2Cpp;
using Il2CppDmm.Games.Store;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Networking;

[assembly: MelonInfo(
    typeof(LOAssetReplacer.Core),
    "LOAssetReplacer",
    "1.1.0",
    "kautism",
    "https://github.com/darkautism/LOAssetReplacer"
)]
[assembly: MelonGame("PiG Corporation", "LastOrigin_R")]

namespace LOAssetReplacer
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("LOAssetReplacer Initialized.");
        }

        [HarmonyPatch(
            typeof(DownloadHandlerAssetBundle),
            nameof(DownloadHandlerAssetBundle.GetContent)
        )]
        public class AssetReplacer
        {
            [HarmonyPrefix]
            internal static bool Prefix(
                ref AssetBundle __result,
                UnityWebRequest www,
                DownloadHandlerAssetBundle __instance
            )
            {
                string assetBundleName = System.IO.Path.GetFileName(www.GetUrl());
                if (System.IO.Directory.Exists("LOAssetReplacer/" + assetBundleName))
                {
                    string[] filePaths = Directory.GetFiles(
                        "LOAssetReplacer\\" + assetBundleName,
                        "__data",
                        SearchOption.AllDirectories
                    );
                    if (filePaths.Length == 0)
                        return true;
                    DownloadHandler
                        .GetCheckedDownloader<DownloadHandlerAssetBundle>(www)
                        .assetBundle.Unload(true);
                    AssetBundle ab = KautismAssetLoader(filePaths[0]);
                    MelonLogger.Msg($"Load from mod: {assetBundleName}");
                    __result = ab;
                    return false;
                }
                return true;
            }
        }

        public static AssetBundle KautismAssetLoader(string path) {
            path = Path.Join(MelonEnvironment.GameRootDirectory, path);
            string url = "file:///" + path.Replace("\\", "/");
            UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(url);
            var op = req.SendWebRequest();
            // 同步等待完成
            while (!op.isDone) ;
            if (req.result != UnityWebRequest.Result.Success)
            {
                MelonLogger.Error("載入失敗: " + req.error);
            }
            return DownloadHandlerAssetBundle.GetContent(req);
        }
    }
}
