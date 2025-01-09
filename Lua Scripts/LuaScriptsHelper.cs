using FogTeam.GameEngine.Network;
using HSGameEngine.GameEngine.Network;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using UnityEngine;
using XLua;

public static class LuaScriptsHelper
{
    [CSharpCallLua] // Thêm thuộc tính này để XLua có thể gọi delegate từ Lua
    public delegate void IntCallback(int value);
    [CSharpCallLua]
    public delegate void SocketConnectEventArgsCallback(SocketConnectEventArgs value);

    [LuaCallCSharp]
    public static XElement[] GetElementsAsArray(XElement xml, string nodeName)
    {
        return xml.Elements(XName.Get(nodeName)).ToArray();
    }

    [LuaCallCSharp]
    public static byte[] IntToBytes(params int[] obj)
    {
        var cmd = obj.ToList();
        var result = DataHelper.ObjectToBytes<List<int>>(cmd);
        return result;
    }

    [LuaCallCSharp]
    public static Dictionary<int, List<int>> ByteToDictionaryListIntInt(byte[] e)
    {
        return DataHelper.BytesToObject<Dictionary<int, List<int>>>(e, 0, e.Length);
    }

    public static int Enum2Int(Enum e)
    {
        return Convert.ToInt32(e);
    }

    [LuaCallCSharp]
    public static void SendSimpleData(TCPGameServerCmds id, params int[] parameter)
    {
        foreach(var e in parameter)
        {
            Debug.Log(e);
        }
        var cmd = IntToBytes(parameter);
        GameInstance.Game.GameClient.SendSimpleData(id, cmd);
    }

    [LuaCallCSharp]
    public static void SendSimpleDataWithCallback(TCPGameServerCmds id, Action<SocketConnectEventArgs> action, params int[] parameter)
    {
        var cmd = IntToBytes(parameter);
        GameInstance.Game.GameClient.SendSimpleDataWithCallback((int)id, cmd, action);
    }
}
