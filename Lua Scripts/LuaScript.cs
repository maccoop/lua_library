using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XLua;
using XLuaTest;

public class LuaScript : MonoBehaviour
{
    public Injection[] injections;
    public static float lastGCTime = 0;
    public const float GCInterval = 1;//1 second 
    public string Path;
    protected LuaTable ScriptScopeTable { get; set; }
    private Action _luaAwake;
    private Action _luaStart;
    private Action _luaUpdate;
    private Action _luaOnDestroy;
    private Action onGUI;
    private Action<Collider> _luaOnTriggerEnter;
    private Action<Collider2D> _luaOnTriggerEnter2D;
    private bool _isStart = false;
    private LuaEnv _env;

    [LuaCallCSharp]
    public async void InitLuaDef(string path)
    {
        if (_env == null)
            _env = LuaLoader.CurrentEnv;
        await InitLua(path, _env);
    }

    public virtual async Task InitLua(string path, LuaEnv env)
    {
        _env = env;
        Path = path;
        int cframe = Time.frameCount;
        NewScopeTable();
        await LuaLoader.DownloadAndDoString(path, env, ScriptScopeTable);
        ScriptScopeTable.Get("awake", out _luaAwake);
        ScriptScopeTable.Get("start", out _luaStart);
        ScriptScopeTable.Get("update", out _luaUpdate);
        ScriptScopeTable.Get("ondestroy", out _luaOnDestroy);
        ScriptScopeTable.Get("onTriggerEnter2D", out _luaOnTriggerEnter2D);
        ScriptScopeTable.Get("onTriggerEnter", out _luaOnTriggerEnter);
        ScriptScopeTable.Get("ongui", out onGUI);
        Debug.Log("Get Functions Success: " + path);
        if (_luaAwake != null)
            _luaAwake();
        if (cframe != Time.frameCount)
            Start();
        return;
    }

    void NewScopeTable()
    {
        ScriptScopeTable = _env.NewTable();
        using (LuaTable meta = _env.NewTable())
        {
            meta.Set("__index", _env.Global);
            ScriptScopeTable.SetMetaTable(meta);
        }
        ScriptScopeTable.Set("self", this);
    }

    private void Start()
    {
        if (_luaStart != null && !_isStart)
        {
            _isStart = true;
            _luaStart();
        }
    }

    private void Update()
    {
        if (_luaUpdate != null)
        {
            _luaUpdate();
        }
    }

    private void OnDestroy()
    {
        if (_luaOnDestroy != null)
        {
            _luaOnDestroy();
        }
    }

    private void OnGUI()
    {
        if (onGUI != null)
        {
            onGUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_luaOnTriggerEnter != null)
        {
            _luaOnTriggerEnter.Invoke(other);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_luaOnTriggerEnter2D != null)
        {
            _luaOnTriggerEnter2D.Invoke(collision);
        }
    }

    Dictionary<string, Action> ActionOnEvent = new();

    public void Register(string key, Action action)
    {
        ActionOnEvent.TryAdd(key, action);
    }

    public void CallAction(string key)
    {
        ActionOnEvent[key].Invoke();
    }

    [LuaCallCSharp]
    public object CallMethod(string method, params object[] parameters)
    {
        return GetType().GetMethod(method).Invoke(this, parameters);
    }

    [LuaCallCSharp]
    public object[] CallLuaFunc(string method, params object[] parameters)
    {
        Debug.Log("Lua call Me to Call Lua Func");
        var lua = ScriptScopeTable.Get<LuaFunction>(method);
        return lua.Call(parameters);
    }

    public bool is_Editor()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    public bool is_Android()
    {
#if UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }
}
