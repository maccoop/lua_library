using System;
using UnityEngine;

public class TimeCount
{
    static DateTime d1;
    public static void Ping()
    {
        d1 = DateTime.Now;
    }

    public static void Pong(string rname)
    {
        Debug.Log(rname +" running need: " + (DateTime.Now - d1).TotalMilliseconds);
    }
}
