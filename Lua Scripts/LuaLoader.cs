using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using XLua;

public delegate void LuaStatus(float percent, string status);
public delegate void LuaPending(string status);

public class LuaLoader : MonoBehaviour
{
    public string configPath;
    public static LuaEnv luaEnv = new LuaEnv();
    public Action onLuaLoaded;
    public LuaStatus onLuaLoading;
    public LuaPending onLuaPending;

    public static async Task DownloadAndDoString(string path, LuaTable table)
    {
        try
        {
            var request = UnityWebRequest.Get(path);
            await request.SendWebRequest();
            LuaLoader.luaEnv.DoString(request.downloadHandler.text, path, table);
        }
        catch (Exception es)
        {
            Debug.LogError("Error Lua Loader : " + es.Message);
        }
    }

    async void Awake()
    {
        onLuaLoading += OnLuaLoading;
        onLuaLoaded += OnLuaLoaded;
        onLuaPending += OnLuaPending;
        await XMLReader.Init(configPath);
        int i = 0;
        /// Init require
        /// 
        {
            var path = "";
            var scriptName = "";
            var require = XMLReader.GetRequire();
            i = 0;
            foreach (var item in require)
            {
                await CheckConnection();
                i++;
                scriptName = item.Split('@')[0];
                path = item.Split('@')[1];
                onLuaLoading.Invoke(i * 1f / require.Length * 100, "Library");
                await DownloadAndDoString(path, null);
            }
        }

        /// Init hotfix
        /// 
        {
            var path = "";
            var scriptName = "";
            var hotPath = XMLReader.GetHotFix();
            i = 0;
            foreach (var item in hotPath)
            {
                await CheckConnection();
                i++;
                scriptName = item.Split('@')[0];
                path = item.Split('@')[1];
                Debug.Log("Init Hotfix: " + scriptName);
                onLuaLoading.Invoke(i * 1f / hotPath.Length * 100, "Fix Bug");
                await DownloadAndDoString(path, null);

            }
        }
        /// Init Service
        /// 
        {
            var path = "";
            var serviceName = "";
            var servicePaths = XMLReader.GetServices();
            i = 0;
            foreach (var item in servicePaths)
            {
                await CheckConnection();
                i++;
                serviceName = item.Split('@')[0];
                path = item.Split('@')[1];
                var obj = new GameObject(serviceName).AddComponent<ServiceImplement>();
                obj.ServiceName = serviceName;
                obj.Register();
                onLuaLoading.Invoke(i * 1f / servicePaths.Length * 100, "Service");
                await obj.InitLua(path);
                Debug.Log("Init Service: " + serviceName);
            }
        }
        /// Init Main
        /// 
        {
            var path = "";
            var scriptName = "";
            var mainPath = XMLReader.GetMain();
            i = 0;
            foreach (var item in mainPath)
            {
                await CheckConnection();
                i++;
                scriptName = item.Split('@')[0];
                path = item.Split('@')[1];
                var obj = new GameObject(scriptName).AddComponent<LuaScript>();
                onLuaLoading.Invoke(i * 1f / mainPath.Length * 100, "Object");
                await obj.InitLua(path);
            }
        }
        onLuaLoaded?.Invoke();
    }

    async Task CheckConnection()
    {
        bool result = Application.internetReachability == NetworkReachability.NotReachable;
        while (result)
        {
            onLuaPending.Invoke("Networking");
            await Task.Delay(3000);
            result = Application.internetReachability == NetworkReachability.NotReachable;
        }
        return;
    }

    private void OnLuaLoading(float percent, string status)
    {
        Debug.Log($"{status} Loading ... {percent}%");
    }
    private void OnLuaPending(string status)
    {
        Debug.Log($"{status} Pending ...");
    }

    private void OnLuaLoaded()
    {
        Debug.Log($"Lua Loaded");
    }
}
