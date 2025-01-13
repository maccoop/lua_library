
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XLua;

public static class LuaScriptsHelper
{

    [LuaCallCSharp]
    public static object ArrayToListNonGeneric(Type type, object array)
    {
        Debug.Log($"Cal method with type {type}, params arrays ");
        var method = typeof(LuaScriptsHelper)
            .GetMethod("ArrayToList")
            .MakeGenericMethod(type);
        return method.Invoke(null, new object[] { array });
    }

    [LuaCallCSharp]
    public static List<T> ArrayToList<T>(params T[] array)
    {
        foreach (var item in array)
            Debug.Log(item);
        return array.ToList();
    }

    public static int Enum2Int(Enum e){
        return Convert.ToInt32(e);
    }
}
