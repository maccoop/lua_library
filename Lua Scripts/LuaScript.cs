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
    protected LuaTable scriptScopeTable;
    private Action luaStart;
    private Action luaUpdate;
    private Action luaOnDestroy;
    private Action onGUI;
    private Action<Collider> luaOnTriggerEnter;
    private Action<Collider2D> luaOnTriggerEnter2D;
    bool isStart = false;

    public virtual async Task InitLua(string path)
    {
        int cframe = Time.frameCount;
        NewScopeTable();
        await LuaLoader.DownloadAndDoString(path, scriptScopeTable);
        scriptScopeTable.Get("start", out luaStart);
        scriptScopeTable.Get("update", out luaUpdate);
        scriptScopeTable.Get("ondestroy", out luaOnDestroy);
        scriptScopeTable.Get("onTriggerEnter2D", out luaOnTriggerEnter2D);
        scriptScopeTable.Get("onTriggerEnter", out luaOnTriggerEnter);
        scriptScopeTable.Get("ongui", out onGUI);
        if (cframe != Time.frameCount)
            Start();
        return;
    }

    void NewScopeTable()
    {
        scriptScopeTable = LuaLoader.luaEnv.NewTable();
        using (LuaTable meta = LuaLoader.luaEnv.NewTable())
        {
            meta.Set("__index", LuaLoader.luaEnv.Global);
            scriptScopeTable.SetMetaTable(meta);
        }
        scriptScopeTable.Set("self", this);
    }

    private void Start()
    {
        if (luaStart != null && !isStart)
        {
            isStart = true;
            luaStart();
        }
    }

    private void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
    }

    private void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
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
        if (luaOnTriggerEnter != null)
        {
            luaOnTriggerEnter.Invoke(other);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (luaOnTriggerEnter2D != null)
        {
            luaOnTriggerEnter2D.Invoke(collision);
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

    public object CallMethod(string method, params object[] parameters)
    {
        return GetType().GetMethod(method).Invoke(this, parameters);
    }

    public object[] CallLuaFunc(string method, params object[] parameters)
    {
        var lua = scriptScopeTable.Get<LuaFunction>(method);
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
