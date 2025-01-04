using System;
using UnityEngine;
using XLua;

public class ServiceImplement : LuaScript, IEService
{
    public string ServiceName;
    public void Register()
    {
        ServiceLocator.Instance.Register(ServiceName,this);
    }

    public object[] CallMethod(string methodName, params object[] parameters)
    {
        var funct = this.scriptScopeTable.Get<LuaFunction>(methodName);
        return funct.Call(parameters);
    }
}
