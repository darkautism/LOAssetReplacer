using HarmonyLib;
using Il2Cpp;
using Il2CppDmm.Games.Store;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.Networking;
using static LOAssetReplacer.Core;

[assembly: MelonInfo(
    typeof(LOAssetReplacer.Core),
    "LOAssetReplacer",
    "1.1.1",
    "kautism",
    "https://github.com/darkautism/LOAssetReplacer"
)]
[assembly: MelonGame("", "")]

namespace LOAssetReplacer
{
    public class Core : MelonMod
    {
        public struct Req {
            public UnityWebRequest req;
            public UnityWebRequestAsyncOperation op;
        }
        // (延遲載入)
        public static readonly Dictionary<string, Req> s_req =
            new Dictionary<string, Req>();

        // 對應 bundle 名稱 -> 原始檔案路徑（第一個 __data）
        public static readonly Dictionary<string, string> s_bundlePaths =
            new Dictionary<string, string>();

        public override void OnInitializeMelon()
        {
            ScanModFolders();
            LoggerInstance.Msg("LOAssetReplacer Initialized.");
        }

        private void ScanModFolders()
        {
            string root = Path.Combine(MelonEnvironment.GameRootDirectory, "LOAssetReplacer");
            if (!Directory.Exists(root))
                return;

            foreach (var dir in Directory.GetDirectories(root))
            {
                string bundleName = Path.GetFileName(dir);
                
                // 找到 __data 檔案（可根據需求改成其他檔名匹配）
                string[] filePaths = Directory.GetFiles(dir, "__data", SearchOption.AllDirectories);
                if (filePaths.Length == 0)
                    continue;

                if (!s_req.ContainsKey(bundleName))
                {
                    s_bundlePaths[bundleName] = filePaths[0];
                    string url = "file:///" + filePaths[0].Replace("\\", "/");
                    UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(url);
                    UnityWebRequestAsyncOperation op = req.SendWebRequest();
                    s_req[bundleName] = new Req{ 
                        req = req, op = op,
                    };
                }
            }
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
                    if (s_req.ContainsKey(assetBundleName) ) {
                        DownloadHandler
                            .GetCheckedDownloader<DownloadHandlerAssetBundle>(www)
                            .assetBundle.Unload(true);
                        // 同步等待完成
                        while (!s_req[assetBundleName].op.isDone) ;

                        AssetBundle ab = DownloadHandlerAssetBundle.GetContent(s_req[assetBundleName].req);
                        __result = ab;

                    MelonLogger.Msg($"Load from mod: {assetBundleName}");
                    return false;
                }
                    
                
                return true;
            }
        }
    }
}
