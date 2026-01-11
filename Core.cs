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

        // 快取字典：存 AssetBundle 實體
        public static readonly Dictionary<string, AssetBundle> s_bundleCache = 
            new Dictionary<string, AssetBundle>();

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


        [HarmonyPatch(typeof(DownloadHandlerAssetBundle), nameof(DownloadHandlerAssetBundle.GetContent))]
        public class AssetReplacer
        {
            [HarmonyPrefix]
            internal static bool Prefix(ref AssetBundle __result, UnityWebRequest www)
            {
                string assetBundleName = System.IO.Path.GetFileName(www.url);

                // --- 第一階段：從 AB 快取拿 (避免重複處理) ---
                if (s_bundleCache.TryGetValue(assetBundleName, out AssetBundle cachedAb) && cachedAb != null)
                {
                    __result = cachedAb;
                    return false; // 攔截成功
                }

                // --- 第二階段：從預載入字典 s_req 拿 ---
                if (s_req.TryGetValue(assetBundleName, out Req reqData))
                {
                    try
                    {
                        // 同步等待預載入完成
                        while (!reqData.op.isDone) { /* IL2CPP 安全等待 */ }

                        if (reqData.req.result == UnityWebRequest.Result.Success)
                        {
                            AssetBundle newAb = DownloadHandlerAssetBundle.GetContent(reqData.req);
                            if (newAb != null)
                            {
                                // 存入永久快取，下次直接走第一階段
                                s_bundleCache[assetBundleName] = newAb;
                                __result = newAb;
                                MelonLogger.Msg($"[Mod] From s_req to Cache: {assetBundleName}");
                            }
                        }
                        else
                        {
                            MelonLogger.Error($"[Mod] Preload failed: {reqData.req.error}");
                            return true; // 預載失敗，退回原生邏輯
                        }
                    }
                    finally
                    {
                        //從 s_req 拿完資料後，一定要 Dispose 並移除，釋放 Native 記憶體
                        if (reqData.req != null)
                        {
                            reqData.req.Dispose();
                        }
                        s_req.Remove(assetBundleName);
                        MelonLogger.Msg($"[Mod] Disposed s_req: {assetBundleName}");
                    }

                    return false; // 攔截成功
                }

                // --- 第三階段：直接用原生 ---
                return true;
            }
        }
    }
}
