using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using XLua;
using Json = UnityEngine.JsonUtility;

public delegate void LuaStatus(float percent, string status);
public delegate void LuaPending(string status);

public class LuaLoader : MonoBehaviour
{
#if UNITY_EDITOR
    public const string PATH = "C:/Users/Admin/Project/LuaScript/KV1/";
    public const bool CACHE_CODE = false;
#else
    public const string PATH = "url to file";
    public const bool CACHE = true;
#endif
    public LuaEnv LuaEnv = new LuaEnv();
    public string configPath;
    public Action onLuaLoaded;
    public LuaStatus onLuaLoading;
    public LuaPending onLuaPending;
    private static Queue<string> _queueUpdateCode;
    private static Task _updateCacheTask = Task.CompletedTask;
    public static LuaEnv CurrentEnv { get; private set; }
    static LuaLoader()
    {
        _queueUpdateCode = new();
    }
    public static async Task DownloadAndDoString(string path, LuaEnv env, LuaTable table)
    {
        try
        {
            string code = "";
            if (!CACHE_CODE || !File.Exists(Path.Combine(Application.persistentDataPath, path)))
            {
                string currentPath = path;
                if (!currentPath.Contains("http") && !File.Exists(currentPath))
                {
                    currentPath = Path.Combine(PATH, path);
                }
                var request = UnityWebRequest.Get(currentPath);
                await request.SendWebRequest();
                code = request.downloadHandler.text;
                if (string.IsNullOrEmpty(code))
                    throw new NullReferenceException($"{path} hasn' code!");
                if (CACHE_CODE)
                {
                    SaveCache(path, code);
                }
            }
            else
            {
                code = File.ReadAllText(Path.Combine(Application.persistentDataPath, path));
                AddQueueUpdate(path);
            }
            Debug.Log("Env: " + env);
            Debug.Log("path: " + path);
            Debug.Log("table: " + table);
            env.DoString(code, path, table);
        }
        catch (Exception es)
        {
            Debug.LogError("Error Lua Loader : " + es.Message);
        }
    }
    private static async void SaveCache(string path, string code)
    {
        string filePath = Path.Combine(Application.persistentDataPath, path);
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(filePath, code);
    }
    public static async Task Preload(string path)
    {
        try
        {
            if (!CACHE_CODE)
                return;
            string code = "";
            if (!File.Exists(Path.Combine(Application.persistentDataPath, path)))
            {
                string currentPath = path;
                if (!currentPath.Contains("http") && !File.Exists(currentPath))
                {
                    currentPath = Path.Combine(PATH, path);
                }
                var request = UnityWebRequest.Get(currentPath);
                await request.SendWebRequest();
                code = request.downloadHandler.text;
                if (string.IsNullOrEmpty(code))
                    Debug.LogError($"'{path}' hasn' code!");
                else SaveCache(path, code);
            }
        }
        catch (Exception es)
        {
            Debug.LogError("Error Lua Loader : " + es.Message);
        }
    }
    private static void AddQueueUpdate(string path)
    {
        if (!CACHE_CODE)
            return;
        if (!_queueUpdateCode.Contains(path))
        {
            _queueUpdateCode.Enqueue(path);
            UpdateCache();
        }
    }
    private static async void UpdateCache()
    {
        if (!_updateCacheTask.IsCompleted)
            return;

        _updateCacheTask = TaskUpdateCache();
        await _updateCacheTask;
    }
    private static async Task TaskUpdateCache()
    {
        while (_queueUpdateCode.Count != 0)
        {
            string path = _queueUpdateCode.Dequeue();
            string currentPath = path;
            if (!currentPath.Contains("http") && !File.Exists(currentPath))
            {
                currentPath = Path.Combine(PATH, path);
            }
            string result = null;
            using (var request = UnityWebRequest.Get(currentPath))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.ConnectionError)
                {
                    result = request.downloadHandler.text;
                    SaveCache(path, result);
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log($"{path} is update success!!");
            }
        }
        return;
    }

    async void Awake()
    {
        LuaEnv = new();
        CurrentEnv = LuaEnv;
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
                await DownloadAndDoString(path, LuaEnv, null);
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
                onLuaLoading.Invoke(i * 1f / hotPath.Length * 100, "Fix Bug");
                await DownloadAndDoString(path, LuaEnv, null);

            }
        }
        /// Init preload
        /// 
        {
            var path = "";
            var scriptName = "";
            var preload = XMLReader.GetPreload();
            i = 0;
            foreach (var item in preload)
            {
                await CheckConnection();
                i++;
                scriptName = item.Split('@')[0];
                path = item.Split('@')[1];
                onLuaLoading.Invoke(i * 1f / preload.Length * 100, "Create Cache");
                await Preload(path);

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
                await obj.InitLua(path, LuaEnv);
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
                await obj.InitLua(path, LuaEnv);
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
